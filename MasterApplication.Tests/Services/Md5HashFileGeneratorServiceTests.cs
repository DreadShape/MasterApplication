using MasterApplication.Services.Feature.Md5Hash;

namespace MasterApplication.Tests.Services;

[ConstructorTests(typeof(Md5HashFileGeneratorServiceTests))]
public partial class Md5HashFileGeneratorServiceTests
{
    [Fact]
    public void CalculateMd5Hash_Execute_GetsTheMd5HashOfAFile()
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

    [Fact]
    public void CalculateMd5Hash_ThrowsArgumentNullException_WhenFilePathIsNull()
    {
        // Arrange
        Md5HashFileGeneratorService sut = new();
        string emptyFilePath = null!;

        // Act
        ArgumentNullException? exception = null;
        try
        {
            sut.CalculateMd5Hash(emptyFilePath);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("filePath", exception.ParamName);
    }

    [Fact]
    public void CalculateMd5Hash_ThrowsArgumentNullException_WhenFilePathIsEmpty()
    {
        // Arrange
        Md5HashFileGeneratorService sut = new();
        string emptyFilePath = string.Empty;

        // Act
        ArgumentNullException? exception = null;
        try
        {
            sut.CalculateMd5Hash(emptyFilePath);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("filePath", exception.ParamName);
    }

    [Fact]
    public void CalculateMd5Hash_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        string nonExistentFilePath = "nonexistentfile.txt";
        Md5HashFileGeneratorService sut = new();

        // Act
        FileNotFoundException? exception = null;
        try
        {
            sut.CalculateMd5Hash(nonExistentFilePath);
        }
        catch (FileNotFoundException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(nonExistentFilePath, exception.FileName);
    }

    [Fact]
    public void CalculateMd5Hash_ThrowsIOException_WhenFileIsInUse()
    {
        // Arrange
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(executingDirectory, "inuse.txt");

        // Create a file and lock it by opening a file stream
        File.WriteAllText(filePath, "test content");

        IOException? exception = null;

        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Md5HashFileGeneratorService sut = new();

            try
            {
                // Act
                sut.CalculateMd5Hash(filePath);
            }
            catch (IOException ex)
            {
                // Capture the exception
                exception = ex;
            }
        }

        // Cleanup: Ensure the file is deleted after test execution
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Assert
        Assert.NotNull(exception);
    }

    // Optionally, test large file handling if performance is important
    [Fact]
    public void CalculateMd5Hash_CanHandleLargeFiles()
    {
        // Arrange
        string largeFilePath = Path.Combine(Path.GetTempPath(), "largefile.txt");

        // Create a large file (e.g., 100 MB)
        using (FileStream fs = new FileStream(largeFilePath, FileMode.Create))
        {
            byte[] data = new byte[1024 * 1024]; // 1 MB of data
            for (int i = 0; i < 100; i++)
            {
                fs.Write(data, 0, data.Length);
            }
        }

        Md5HashFileGeneratorService sut = new();

        // Act
        string hash = sut.CalculateMd5Hash(largeFilePath);

        // Assert (no expected specific hash; just check it doesn't fail)
        Assert.NotNull(hash);

        // Cleanup
        if (File.Exists(largeFilePath))
            File.Delete(largeFilePath);
    }
}
