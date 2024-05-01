namespace MasterApplication.Models;

/// <summary>
/// Model for a file with the file's name and it's MD5 hash.
/// </summary>
public class Md5HashFile
{
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
