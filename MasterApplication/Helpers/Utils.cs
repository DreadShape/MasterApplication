using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;

using Emgu.CV.Structure;

using Emgu.CV;
using System.Drawing.Imaging;

namespace MasterApplication.Helpers;

public static class Utils
{
    /// <summary>
    /// Replaces all the invalid file name characters to '-'.
    /// </summary>
    /// <param name="input">Text to normalize.</param>
    /// <returns>The normalized text.</returns>
    public static string NormalizeFileName(string input)
    {
        // Replace invalid file characters with underscores
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char invalidChar in invalidChars)
            input = input.Replace(invalidChar, '-');

        // Remove leading and trailing whitespaces
        input = input.Trim();

        return input;
    }

    /// <summary>
    /// Closes all processes with the same name as the executing executable.
    /// </summary>
    public static void CloseAllProcessesWithSameName()
    {
        // Get the current process
        string currentProcess = Process.GetCurrentProcess().ProcessName;

        // Get all processes with the same name
        Process[] processes = Process.GetProcessesByName(currentProcess);

        // Iterate through all processes
        foreach (Process process in processes)
        {
            try
            {
                process.Kill();
                process.WaitForExit();
            }
            catch (Exception)
            {
            }
        }
    }

    /// <summary>
    /// Captures the screen within the specified bounds and converts it to a grayscale Emgu CV image.
    /// If no bounds are specified, captures the entire primary screen.
    /// </summary>
    /// <param name="x">The X-coordinate of the top-left corner of the capture area (optional).</param>
    /// <param name="y">The Y-coordinate of the top-left corner of the capture area (optional).</param>
    /// <param name="width">The width of the capture area (optional).</param>
    /// <param name="height">The height of the capture area (optional).</param>
    /// <returns>An Emgu CV Image of type <see cref="Image{Gray, byte}"/>.</returns>
    public static Image<Gray, byte> CaptureScreen(int? x = null, int? y = null, int? width = null, int? height = null)
    {
        // Get the bounds of the capture area
        int captureX = x ?? 0;
        int captureY = y ?? 0;
        int captureWidth = width ?? (int)SystemParameters.PrimaryScreenWidth;
        int captureHeight = height ?? (int)SystemParameters.PrimaryScreenHeight;
        Rectangle bounds = new Rectangle(captureX, captureY, captureWidth, captureHeight);

        // Create a bitmap to store the screenshot
        Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        // Create a graphics object from the bitmap
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // Capture the screen and draw it onto the bitmap
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        }

        // Convert the Bitmap to Emgu CV Image and return it
        Image<Gray, byte> emguImage = bitmap.ToImage<Bgr, byte>().Convert<Gray, byte>();
        bitmap.Dispose();

        return emguImage;
    }

    /// <summary>
    /// Detects changes in two images.
    /// </summary>
    /// <param name="previous">Previous image.</param>
    /// <param name="current">Current image.</param>
    /// <returns></returns>
    public static bool DetectChange(Image<Gray, byte> previous, Image<Gray, byte> current)
    {
        // Calculate the absolute difference between the two images
        Image<Gray, byte> difference = previous.AbsDiff(current);

        // Calculate the sum of the differences
        double sumOfDifferences = CvInvoke.Sum(difference).V0;

        // Define a threshold to decide if there is a significant change
        double changeThreshold = 1300.0; // Adjust based on your needs

        return sumOfDifferences > changeThreshold;
    }
}
