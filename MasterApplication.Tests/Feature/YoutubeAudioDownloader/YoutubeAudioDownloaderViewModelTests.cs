using Castle.Core.Logging;

using MasterApplication.Feature.YoutubeAudioDownloader;
using MasterApplication.Services.Dialog;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Tests.Feature.YoutubeAudioDownloader;

[ConstructorTests(typeof(YoutubeAudioDownloaderViewModelTests))]
public partial class YoutubeAudioDownloaderViewModelTests
{
    [Fact]
    public async Task LinkViewModel_Download_Execute_DownloadsFile()
    {
        // Arrange
        AutoMocker mocker = new();
        LinkViewModel sut = mocker.CreateInstance<LinkViewModel>();
        sut.Link = @"https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        sut.SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Act
        await sut.VerifyLinkAsync();
        await sut.DownloadCommand.ExecuteAsync(null);

        // Assert
        string pathToFile = Path.Combine(sut.SaveLocation, "Rick Astley - Never Gonna Give You Up (Official Music Video).mp3");
        Assert.True(File.Exists(pathToFile));

        // Cleanup
        if (File.Exists(pathToFile))
            File.Delete(pathToFile);
    }

    [Fact]
    public async Task FileViewModel_Download_Execute_DownloadsFile()
    {
        //Create a temporary file
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string testFilePath = Path.Combine(executingDirectory, "test.txt");

        using (StreamWriter writer = new(testFilePath))
        {
            // Write the text to the file
            writer.WriteLine(@"https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        }

        // Arrange
        AutoMocker mocker = new();
        Mock<ILogger<FileViewModel>> mockLogger = new();
        Mock<IDialogService> mockDialogService = new();
        string[] selectedFilePath = { testFilePath }; // Set the desired file path for testing
        mockDialogService.Setup(x => x.ShowOpenFileDialog("Text files...|*.txt", false))
                         .Returns(selectedFilePath);

        FileViewModel sut = new(mockLogger.Object, mockDialogService.Object);
        await sut.FileLocationCommand.ExecuteAsync(null);
        sut.SaveLocation = executingDirectory;

        // Act
        await sut.DownloadCommand.ExecuteAsync(null);

        // Assert
        string pathToFile = Path.Combine(sut.SaveLocation, "Rick Astley - Never Gonna Give You Up (Official Music Video).mp3");
        Assert.True(File.Exists(pathToFile));

        // Cleanup
        if (File.Exists(testFilePath))
            File.Delete(testFilePath);

        if (File.Exists(pathToFile))
            File.Delete(pathToFile);
    }
}
