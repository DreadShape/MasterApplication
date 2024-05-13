using MasterApplication.Services.Feature.Md5Hash;

namespace MasterApplication.Tests.Feature.Md5Hash;

[ConstructorTests(typeof(Md5HashFileGeneratorViewModelTests))]
public partial class Md5HashFileGeneratorViewModelTests
{
    [Fact]
    public void SaveCalculatedHashesToFile_Execute_CalculatesMd5HashOfFile()
    {
        // Create temporary file
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(executingDirectory, "test.txt");

        // Create a StreamWriter to write to the file
        using (StreamWriter writer = new(filePath))
        {
            // Write the text to the file
            writer.WriteLine("hash test");
        }

        // Arrange
        Md5HashFileGeneratorService sut = new();

        // Act
        string hashResult = sut.CalculateMd5Hash(filePath);

        // Assert
        Assert.Equal("fdaa3c0c77e0eee73a783f5ef8b8a4fc", hashResult);

        // Delete the temporary file
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
