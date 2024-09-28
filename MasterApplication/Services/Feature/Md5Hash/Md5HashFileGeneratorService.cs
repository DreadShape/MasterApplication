using System.IO;
using System.Security.Cryptography;

namespace MasterApplication.Services.Feature.Md5Hash;

/// <summary>
/// Implementation of <see cref="IMd5HashFileGeneratorService"/> to calculate MD5 hashes of files.
/// </summary>
public class Md5HashFileGeneratorService : IMd5HashFileGeneratorService
{
    /// <summary>
    /// Calculates the MD5 hash of a file.
    /// </summary>
    /// <param name="filePath">Path to the file to calculate the MD5 hash.</param>
    /// <returns>The Md5 hash of that file.</returns>
    public string CalculateMd5Hash(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "The file path cannot be null or empty.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file was not found.", filePath);

        string hash;

        try
        {
            using (MD5? md5Generator = MD5.Create())
            {
                using (FileStream? fileStream = File.OpenRead(filePath))
                {
                    // Compute the MD5 hash of the file bytes
                    byte[] hashBytes = md5Generator.ComputeHash(fileStream);

                    // Convert the byte array to a hexadecimal string
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Access to the file '{filePath}' is denied.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"An I/O error occurred while reading the file '{filePath}'.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while calculating the MD5 hash.", ex);
        }

        return hash;
    }
}
