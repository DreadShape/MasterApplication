using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Helpers;
using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Services.Feature.MouseClicker;

using Point = System.Windows.Point;

namespace MasterApplication.UserControls.ScreenShot;

/// <summary>
/// Allows us to select a specific region of a screen shot.
/// </summary>
public partial class ScreenShotWindow : Window
{
    private readonly IKeyboardService _keyboardService;
    private readonly IMessenger _messengerService;
    private Point _startPoint;
    private bool _isSelecting = false;
    private Bitmap? _originalScreenshot;

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

        ContentRendered -= ScreenShotWindow_ContentRendered;
        ContentRendered += ScreenShotWindow_ContentRendered;
    }

    /// <summary>
    /// When the window is fully rendered and shown to the user.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ScreenShotWindow_ContentRendered(object? sender, EventArgs e)
    {
        _originalScreenshot = await Task.Run(async () =>
        {
            await Task.Delay(100);
            return TakeScreenShot();
        });

        ScreenshotImage.Source = Utils.BitmapToBitmapImage(_originalScreenshot);

        // Cover the screen with a semi-transparent overlay initially
        OverlayTop.Width = OverlayCanvas.ActualWidth;
        OverlayTop.Height = OverlayCanvas.ActualHeight;

        OverlayLeft.Width = 0;
        OverlayLeft.Height = 0;
        OverlayRight.Width = 0;
        OverlayRight.Height = 0;
        OverlayBottom.Width = 0;
        OverlayBottom.Height = 0;
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

        UpdateOverlayRectangles(_startPoint.X, _startPoint.Y, 0, 0);
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

        Bitmap selectedRegion = null!;
        try
        {
            selectedRegion = CaptureSelectedRegion();
            ScreenShotSelection selectionDialog = new(selectedRegion);
            selectionDialog.OnSelectionAccepted -= ScreenShotSelection_OnSelectionAccepted;
            selectionDialog.OnSelectionAccepted += ScreenShotSelection_OnSelectionAccepted;

            ResetOverlay();
            selectionDialog?.ShowDialog();
            if (selectionDialog?.DialogResult == false)
            {
                selectedRegion?.Dispose();
                return;
            }

            CleanClose();
        }
        finally
        {
            selectedRegion?.Dispose();
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

        Point currentPoint = e.GetPosition(this);

        // Calculate the dimensions of the selection
        double x = Math.Min(currentPoint.X, _startPoint.X);
        double y = Math.Min(currentPoint.Y, _startPoint.Y);
        double width = Math.Abs(currentPoint.X - _startPoint.X);
        double height = Math.Abs(currentPoint.Y - _startPoint.Y);

        // Update the selection rectangle position and size
        Canvas.SetLeft(SelectionRectangle, x);
        Canvas.SetTop(SelectionRectangle, y);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;

        // Update the dark overlay rectangles
        UpdateOverlayRectangles(x, y, width, height);
    }

    /// <summary>
    /// Updates the positions and sizes of the overlay rectangles based on the current selection.
    /// This method darkens the areas outside the selected region by adjusting four overlay rectangles
    /// (top, left, right, bottom) to fit around the user's selection.
    /// </summary>
    /// <param name="x">The X-coordinate of the selection's top-left corner.</param>
    /// <param name="y">The Y-coordinate of the selection's top-left corner.</param>
    /// <param name="width">The width of the selection rectangle.</param>
    /// <param name="height">The height of the selection rectangle.</param>
    private void UpdateOverlayRectangles(double x, double y, double width, double height)
    {
        double canvasWidth = OverlayCanvas.ActualWidth;
        double canvasHeight = OverlayCanvas.ActualHeight;

        // Top rectangle
        Canvas.SetLeft(OverlayTop, 0);
        Canvas.SetTop(OverlayTop, 0);
        OverlayTop.Width = canvasWidth;
        OverlayTop.Height = y;

        // Left rectangle
        Canvas.SetLeft(OverlayLeft, 0);
        Canvas.SetTop(OverlayLeft, y);
        OverlayLeft.Width = x;
        OverlayLeft.Height = height;

        // Right rectangle
        Canvas.SetLeft(OverlayRight, x + width);
        Canvas.SetTop(OverlayRight, y);
        OverlayRight.Width = canvasWidth - (x + width);
        OverlayRight.Height = height;

        // Bottom rectangle
        Canvas.SetLeft(OverlayBottom, 0);
        Canvas.SetTop(OverlayBottom, y + height);
        OverlayBottom.Width = canvasWidth;
        OverlayBottom.Height = canvasHeight - (y + height);
    }

    /// <summary>
    /// Cuts the selected region from the screenshot.
    /// </summary>
    /// <returns>The selected region as a <see cref="Bitmap"/>.</returns>
    private Bitmap CaptureSelectedRegion()
    {
        if (_originalScreenshot == null)
            throw new InvalidOperationException("No screenshot available.");

        double left = Canvas.GetLeft(SelectionRectangle);
        double top = Canvas.GetTop(SelectionRectangle);
        double width = SelectionRectangle.Width - (SelectionRectangle.StrokeThickness + 5);
        double height = SelectionRectangle.Height - (SelectionRectangle.StrokeThickness + 2);

        int screenX = (int)(left * (_originalScreenshot.Width / ActualWidth));
        int screenY = (int)(top * (_originalScreenshot.Height / ActualHeight));
        int screenWidth = (int)(width * (_originalScreenshot.Width / ActualWidth));
        int screenHeight = (int)(height * (_originalScreenshot.Height / ActualHeight));

        Rectangle cropRect = new Rectangle(screenX, screenY, screenWidth, screenHeight);

        Bitmap croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);

        using (Graphics g = Graphics.FromImage(croppedBitmap))
        {
            g.DrawImage(_originalScreenshot, new Rectangle(0, 0, cropRect.Width, cropRect.Height), cropRect, GraphicsUnit.Pixel);
        }

        return croppedBitmap;
    }

    /// <summary>
    /// Secure closing of the form.
    /// </summary>
    private void CleanClose()
    {
        ScreenShotSelection? selectionWindow = Application.Current.Windows.OfType<ScreenShotSelection>().FirstOrDefault();
        if (selectionWindow != null)
        {
            selectionWindow.DialogResult = false;
            selectionWindow.Close();
        }

        if (_keyboardService.IsKeyboardHookAttached())
            _keyboardService.StopKeyboardHook();

        Canvas.SetLeft(SelectionRectangle, 0);
        Canvas.SetTop(SelectionRectangle, 0);
        SelectionRectangle.Visibility = Visibility.Collapsed;
        _keyboardService.KeyPressed -= KeyboardService_KeyPressed;
        ContentRendered -= ScreenShotWindow_ContentRendered;
        _messengerService.Send(new WindowActionMessage(WindowAction.Normal));
        Close();
    }

    /// <summary>
    /// Takes a screenshot of the current screen.
    /// </summary>
    /// <returns>The screenshot of the captured screen as a <see cref="Bitmap"/>.</returns>
    private Bitmap TakeScreenShot()
    {
        Bitmap screenshot = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        using (Graphics g = Graphics.FromImage(screenshot))
        {
            g.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
        }

        return screenshot;
    }

    /// <summary>
    /// Resets the canvas to it's original state.
    /// </summary>
    private void ResetOverlay()
    {
        // Remove previous clipping
        OverlayCanvas.Clip = null;

        // Hide selection rectangle
        SelectionRectangle.Visibility = Visibility.Collapsed;
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        SelectionRectangle.StrokeThickness = 1;

        UpdateOverlayRectangles(0, 0, 0, 0);

        if (_originalScreenshot != null)
            ScreenshotImage.Source = Utils.BitmapToBitmapImage(_originalScreenshot);
    }
}
