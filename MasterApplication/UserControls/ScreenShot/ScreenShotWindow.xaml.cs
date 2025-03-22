using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Helpers;
using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Services.Feature.MouseClicker;

namespace MasterApplication.UserControls.ScreenShot;

/// <summary>
/// Allows us to select a specific region of a screen shot.
/// </summary>
public partial class ScreenShotWindow : Window
{
    private readonly IKeyboardService _keyboardService;
    private readonly IMessenger _messengerService;
    private System.Windows.Point _startPoint;
    private bool _isSelecting = false;

    /// <summary>
    /// Creates and instance of an <see cref="ScreenShotWindow"/>.
    /// </summary>
    /// <param name="keyboardService"><see cref="IKeyboardService"/> to intercept keyboard presses.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    public ScreenShotWindow(IKeyboardService keyboardService, IMessenger messengerService)
    {
        InitializeComponent();

        _keyboardService = keyboardService;
        _messengerService = messengerService;

        // Hook to the keyboard to be able intercept key presses
        _keyboardService.StartKeyboardHook();
        _keyboardService.KeyPressed -= KeyboardService_KeyPressed;
        _keyboardService.KeyPressed += KeyboardService_KeyPressed;

        Loaded -= ScreenShotWindow_Loaded;
        Loaded += ScreenShotWindow_Loaded;
    }

    /// <summary>
    /// When the form is initialized and rendered. It creates a screenshot of the current screen.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScreenShotWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TakeScreenShot();
    }

    /// <summary>
    /// Intercepts the keyboard key presses. We only listen for the 'Esc' key to close the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyboardService_KeyPressed(object? sender, int e)
    {
        // Escape key
        if (e == 27)
            CleanClose();
    }

    /// <summary>
    /// When the left click of the mouse is being held down. To start cropping the selection of the screnshot taken.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _isSelecting = true;
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        Canvas.SetLeft(SelectionRectangle, _startPoint.X);
        Canvas.SetTop(SelectionRectangle, _startPoint.Y);
        SelectionRectangle.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// When the left click of the mouse is released. Creates the selected crop section of the screenshot.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting)
            return;

        _isSelecting = false;

        Bitmap selectedRegion = CaptureSelectedRegion();
        try
        {
            ScreenShotSelection selectionDialog = new(selectedRegion);
            selectionDialog.OnSelectionAccepted -= ScreenShotSelection_OnSelectionAccepted;
            selectionDialog.OnSelectionAccepted += ScreenShotSelection_OnSelectionAccepted;

            ResetOverlay();
            selectionDialog.ShowDialog();
            if (selectionDialog.DialogResult == false)
            {
                selectedRegion.Dispose();
                return;
            }

            CleanClose();
        }
        finally
        {
            selectedRegion.Dispose();
        }
    }

    /// <summary>
    /// Event to capture the <see cref="AutoClickerTemplate"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScreenShotSelection_OnSelectionAccepted(object? sender, AutoClickerTemplate e)
    {
        _messengerService.Send(e);
    }

    /// <summary>
    /// When the mouse moves over the screen.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting)
            return;

        System.Windows.Point currentPoint = e.GetPosition(this);

        // Calculate the dimensions of the selection
        double x = Math.Min(currentPoint.X, _startPoint.X);
        double y = Math.Min(currentPoint.Y, _startPoint.Y);
        double width = Math.Abs(currentPoint.X - _startPoint.X);
        double height = Math.Abs(currentPoint.Y - _startPoint.Y);

        // Adjust for the border thickness. We add an extra pixel because we lose it when we inset the selection for the border
        double borderThickness = SelectionRectangle.StrokeThickness + 1;

        // Update the selection rectangle position and size, insetting for the border
        Canvas.SetLeft(SelectionRectangle, x - borderThickness / 2);
        Canvas.SetTop(SelectionRectangle, y - borderThickness / 2);
        SelectionRectangle.Width = width + borderThickness;
        SelectionRectangle.Height = height + borderThickness;

        // Update the clipping region for the overlay
        UpdateOverlayClip(x, y, width, height);
    }

    /// <summary>
    /// Updates the newly selected region.
    /// </summary>
    /// <param name="x">Horizontal coordinate.</param>
    /// <param name="y">Vertical coordinate.</param>
    /// <param name="width">Width of the selection.</param>
    /// <param name="height">Height of the selection.</param>
    private void UpdateOverlayClip(double x, double y, double width, double height)
    {
        // Define the selected area as a RectangleGeometry
        RectangleGeometry selectedRegion = new(new Rect(x, y, width, height));

        // Define the entire screen area for clipping
        RectangleGeometry fullRegion = new(new Rect(0, 0, OverlayCanvas.ActualWidth, OverlayCanvas.ActualHeight));

        // Combine the full region and the selected region using Exclude (the area selected will be cut out)
        CombinedGeometry overlayRegion = new(GeometryCombineMode.Exclude, fullRegion, selectedRegion);

        // Apply the clip to the overlay canvas, leaving the selection rectangle intact
        OverlayCanvas.Clip = overlayRegion;
    }

    /// <summary>
    /// Cuts the selected region from the screenshot.
    /// </summary>
    /// <returns>The selected region as a <see cref="Bitmap"/>.</returns>
    private Bitmap CaptureSelectedRegion()
    {
        // Get the selection bounds
        double left = Canvas.GetLeft(SelectionRectangle);
        double top = Canvas.GetTop(SelectionRectangle);
        double width = SelectionRectangle.Width - (SelectionRectangle.StrokeThickness + 5);
        double height = SelectionRectangle.Height - (SelectionRectangle.StrokeThickness + 2);

        // Convert the selected region to the screen coordinates
        int screenX = (int)(left * (SystemParameters.PrimaryScreenWidth / ActualWidth));
        int screenY = (int)(top * (SystemParameters.PrimaryScreenHeight / ActualHeight));
        int screenWidth = (int)(width * (SystemParameters.PrimaryScreenWidth / ActualWidth));
        int screenHeight = (int)(height * (SystemParameters.PrimaryScreenHeight / ActualHeight));

        // Create a bitmap for the selected region
        Bitmap bitmap = new Bitmap(screenWidth, screenHeight);

        // Capture the selected region and draw it into the bitmap
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(screenX, screenY, 0, 0, bitmap.Size);
        }

        return bitmap;
    }

    /// <summary>
    /// Secure closing of the form.
    /// </summary>
    private void CleanClose()
    {
        if (_keyboardService.IsKeyboardHookAttached())
            _keyboardService.StopKeyboardHook();

        Canvas.SetLeft(SelectionRectangle, 0);
        Canvas.SetTop(SelectionRectangle, 0);
        SelectionRectangle.Visibility = Visibility.Collapsed;
        _messengerService.Send(new WindowActionMessage(WindowAction.Maximize));
        Close();
    }

    /// <summary>
    /// Takes a screenshot of the current screen.
    /// </summary>
    private void TakeScreenShot()
    {
        // Capture the screen
        Bitmap screenshot = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        using (Graphics g = Graphics.FromImage(screenshot))
        {
            g.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
        }

        // Set the ImageSource to the captured screenshot
        ScreenshotImage.Source = Utils.BitmapToBitmapImage(screenshot);
    }

    /// <summary>
    /// Resets the canvas to their original state.
    /// </summary>
    private void ResetOverlay()
    {
        OverlayCanvas.Clip = null; // Remove any previous clipping

        // Reset selection rectangle
        SelectionRectangle.Visibility = Visibility.Collapsed;
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        SelectionRectangle.StrokeThickness = 1; // Ensure stroke is back to default

        //Remove the white selection border of the previous selection
        ScreenshotImage.Source = null;
    }
}
