using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MasterApplication.UserControls;

public partial class ScreenShotSelection : Window
{
    private System.Windows.Point _startPoint;
    private bool _isSelecting = false;

    public ScreenShotSelection()
    {
        InitializeComponent();

        // Load screenshot into the Image control (for testing, you can load a sample image)
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(executingDirectory, "test.png");
        ScreenshotImage.Source = new BitmapImage(new Uri(filePath));
    }

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

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting)
            return;

        _isSelecting = false;
        //CaptureSelectedRegion();

        Canvas.SetLeft(SelectionRectangle, 0);
        Canvas.SetTop(SelectionRectangle, 0);
        SelectionRectangle.Visibility = Visibility.Collapsed;
    }

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

        // Adjust for the border thickness
        double borderThickness = SelectionRectangle.StrokeThickness;

        // Update the selection rectangle position and size, insetting for the border
        Canvas.SetLeft(SelectionRectangle, x - (borderThickness+1) / 2);
        Canvas.SetTop(SelectionRectangle, y - (borderThickness+1) / 2);
        SelectionRectangle.Width = width + borderThickness+1;
        SelectionRectangle.Height = height + borderThickness+1;

        // Update the clipping region for the overlay
        UpdateOverlayClip(x, y, width, height);
    }

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

    private void CaptureSelectedRegion()
    {
        // Get the selection bounds
        double left = Canvas.GetLeft(SelectionRectangle);
        double top = Canvas.GetTop(SelectionRectangle);
        double width = SelectionRectangle.Width;
        double height = SelectionRectangle.Height;

        // Convert the selected region to the screen coordinates
        int screenX = (int)(left * (SystemParameters.PrimaryScreenWidth / ActualWidth));
        int screenY = (int)(top * (SystemParameters.PrimaryScreenHeight / ActualHeight));
        int screenWidth = (int)(width * (SystemParameters.PrimaryScreenWidth / ActualWidth));
        int screenHeight = (int)(height * (SystemParameters.PrimaryScreenHeight / ActualHeight));

        // Capture the selected region and save it
        using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screenX, screenY, 0, 0, bitmap.Size);
            }

            // Save the selected region to a file
            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "selectedRegion.png");
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            MessageBox.Show($"Screenshot saved to {filePath}");
        }

        Close();
    }
}
