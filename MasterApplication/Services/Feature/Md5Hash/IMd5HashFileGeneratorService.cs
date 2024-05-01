namespace MasterApplication.Services.Feature.Md5Hash;

/// <summary>
/// Service to calculate MD5 hashes of files.
/// </summary>
public interface IMd5HashFileGeneratorService
{
    /// <summary>
    /// Calculates the MD5 hash of a file.
    /// </summary>
    /// <param name="filePath">Path to the file to calculate the MD5 hash</param>
    /// <returns>The Md5 hash of that file</returns>
    public string CalculateMd5Hash(string pathFile);
}
