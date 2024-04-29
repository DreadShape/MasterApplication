using System.IO;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Services.Md5Hash;

public class Md5FileHashService : IMd5FileHashService
{
    private readonly ILogger<Md5FileHashService> _logger;

    public Md5FileHashService(ILogger<Md5FileHashService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string CalculateMd5Hash(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return $"File not found '{filePath}'";

        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MD5 hash for '{filePath}' file.", filePath);
            return "Creating MD5 hash error, read logs for more information";
        }
    }
}
