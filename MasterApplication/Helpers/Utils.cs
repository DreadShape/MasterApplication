using System.IO;

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
}
