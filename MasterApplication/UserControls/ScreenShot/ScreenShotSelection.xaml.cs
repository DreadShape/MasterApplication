using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MasterApplication.Helpers;
using MasterApplication.Models;

namespace MasterApplication.UserControls.ScreenShot;

/// <summary>
/// Interaction logic for ScreenShotSelection.xaml
/// </summary>
public partial class ScreenShotSelection : Window
{
    public event EventHandler<AutoClickerTemplate>? OnSelectionAccepted;

    private System.Windows.Point? _lastClickedPosition;
    private const double TOLERANCE = 0.5;
    private readonly AutoClickerTemplate _template;

    public ScreenShotSelection(Bitmap screenshot)
    {
        InitializeComponent();
        
        // Convert Bitmap to BitmapImage
        BitmapImage bitmapImage = new();
        using (MemoryStream memoryStream = new())
        {
            // Save the bitmap to the MemoryStream in PNG format
            screenshot.Save(memoryStream, ImageFormat.Png);

            // Reset the stream position to the beginning
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Initialize the BitmapImage
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // CacheOption is important to hold the data after stream is closed
            bitmapImage.EndInit();
        }

        // Set the ImageSource to the captured screenshot
        ScreenshotImage.Source = bitmapImage;
        ScreenshotImage.Width = bitmapImage.Width;
        ScreenshotImage.Height = bitmapImage.Height;

        _template = new();
        _template.Image = Utils.BitmapToByteArray(screenshot);
        _template.ClickCoordinates = new System.Windows.Point(bitmapImage.Width / 2, bitmapImage.Height / 2);
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        OnSelectionAccepted?.Invoke(this, _template);
        Close();
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Get the mouse click position relative to the Image control
        System.Windows.Point clickPosition = e.GetPosition(ScreenshotImage);

        // Get the actual size of the image being displayed (accounting for Stretch)
        var bitmapImage = ScreenshotImage.Source as BitmapImage;
        if (bitmapImage == null)
            return;

        // Calculate the displayed image size, taking Stretch into account
        double imageDisplayWidth = ScreenshotImage.ActualWidth;
        double imageDisplayHeight = ScreenshotImage.ActualHeight;

        // Calculate the offset of the image (because it's centered)
        double offsetX = (ClickCanvas.ActualWidth - imageDisplayWidth) / 2;
        double offsetY = (ClickCanvas.ActualHeight - imageDisplayHeight) / 2;

        // Check if the click is within the bounds of the image
        if (clickPosition.X < 0 || clickPosition.Y < 0 ||
            clickPosition.X > imageDisplayWidth || clickPosition.Y > imageDisplayHeight)
            return;

        // Clear the canvas before adding a new circle
        ClickCanvas.Children.Clear();
        
        // Define the circle (Ellipse)
        Ellipse circle = new()
        {
            Width = 20,
            Height = 20,
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Fill = System.Windows.Media.Brushes.Transparent
        };

        // Set the position of the circle on the canvas (centered on the click point)
        double adjustedX = clickPosition.X + offsetX - (circle.Width / 2);
        double adjustedY = clickPosition.Y + offsetY - (circle.Height / 2);

        // Set the circle position
        Canvas.SetLeft(circle, adjustedX);
        Canvas.SetTop(circle, adjustedY);

        // Add the circle to the canvas
        ClickCanvas.Children.Add(circle);

        // Store the last clicked position
        _lastClickedPosition = clickPosition;

        // Update the template with the click coordinates
        _template.ClickCoordinates = _lastClickedPosition.Value;
    }

    private bool IsSamePosition(System.Windows.Point newPosition, System.Windows.Point lastPosition)
    {
        return (Math.Abs(newPosition.X - lastPosition.X) < TOLERANCE) &&
               (Math.Abs(newPosition.Y - lastPosition.Y) < TOLERANCE);
    }
}
