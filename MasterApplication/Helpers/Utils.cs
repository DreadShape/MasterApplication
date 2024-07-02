using System.Diagnostics;
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
}
