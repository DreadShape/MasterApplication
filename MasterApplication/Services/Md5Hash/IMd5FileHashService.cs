namespace MasterApplication.Services.Md5Hash;

public interface IMd5FileHashService
{
    string CalculateMd5Hash(string filePath);
}
