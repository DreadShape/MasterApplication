using MasterApplication.Feature.YoutubeAudioDownloader;

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
}
