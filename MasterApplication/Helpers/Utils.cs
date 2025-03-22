using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;

using Emgu.CV.Structure;

using Emgu.CV;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using FontStyle = System.Drawing.FontStyle;

namespace MasterApplication.Helpers;

/// <summary>
/// Static class witth various miscellaneous methods.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Replaces all the invalid file name characters to '-'.
    /// </summary>
    /// <param name="input">Text to normalize.</param>
    /// <returns>The normalized text.</returns>
    public static string NormalizeFileName(string input)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char invalidChar in invalidChars)
            input = input.Replace(invalidChar, '-');

        input = input.Trim();

        return input;
    }

    /// <summary>
    /// Closes all processes with the same name as the executing executable.
    /// </summary>
    public static void CloseAllProcessesWithSameName()
    {
        string currentProcess = Process.GetCurrentProcess().ProcessName;

        Process[] processes = Process.GetProcessesByName(currentProcess);

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
        int captureX = x ?? 0;
        int captureY = y ?? 0;
        int captureWidth = width ?? (int)SystemParameters.PrimaryScreenWidth;
        int captureHeight = height ?? (int)SystemParameters.PrimaryScreenHeight;
        Rectangle bounds = new(captureX, captureY, captureWidth, captureHeight);

        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (Graphics graphics = Graphics.FromImage(bitmap))
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);

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
        double changeThreshold = 1200.0; // Adjust based on your needs

        return sumOfDifferences > changeThreshold;
    }

    /// <summary>
    /// Transforms a <see cref="Bitmap"/> into a <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="bitmap"><see cref="Bitmap"/> to transform.</param>
    /// <returns></returns>
    public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
    {
        using (MemoryStream memoryStream = new())
        {
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }

    /// <summary>
    /// Transforms a <see cref="Bitmap"/> into a <see cref="byte[]"/>.
    /// </summary>
    /// <param name="bitmap"><see cref="Bitmap"/> to transform.</param>
    /// <returns></returns>
    public static byte[] BitmapToByteArray(Bitmap bitmap)
    {
        using (MemoryStream memoryStream = new())
        {
            bitmap.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Transforms a <see cref="byte[]"/> into a <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="byteArray"><see cref="byte[]"/> to transform.</param>
    /// <returns></returns>
    public static Bitmap ByteArrayToBitmap(byte[] byteArray)
    {
        using (MemoryStream memoryStream = new(byteArray))
        {
            Bitmap bitmap = new(memoryStream);
            return bitmap;
        }
    }

    /// <summary>
    /// Transforms a <see cref="byte[]"/> into a <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="byteArray"><see cref="byte[]"/> to transform.</param>
    /// <returns></returns>
    public static BitmapImage ByteArrayToBitmapImage(byte[] imageData)
    {
        BitmapImage bitmapImage = new();

        using (MemoryStream memoryStream = new(imageData))
        {
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze to make it cross-thread accessible
        }

        return bitmapImage;
    }

    /// <summary>
    /// Generates a black image with a centered text of "Image Not Found".
    /// </summary>
    /// <param name="width">Width of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <returns>A <see cref="byte[]"/> of that image.</returns>
    public static byte[] GenerateNotFoundImage(int width = 500, int height = 400)
    {
        using (Bitmap bmp = new Bitmap(width, height))
        {
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.Black);

                using (Font font = new Font("Arial", 13, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    string message = "Image Not Found";
                    SizeF textSize = gfx.MeasureString(message, font);

                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;

                    gfx.DrawString(message, font, textBrush, x, y);
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
