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
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return $"File not found '{filePath}'";

        string hash;

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

        return hash;
    }
}
