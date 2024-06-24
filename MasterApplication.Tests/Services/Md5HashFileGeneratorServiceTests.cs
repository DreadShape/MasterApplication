using MasterApplication.Features.Md5HashFileGenerator;
using MasterApplication.Models;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.Md5Hash;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Tests.Services;

[ConstructorTests(typeof(Md5HashFileGeneratorServiceTests))]
public partial class Md5HashFileGeneratorServiceTests
{
    [Fact]
    public void SaveCalculatedHashesToFileCommand_Execute_SavesHashesToFile()
    {
        //Arrange
        //Necessary services for the viewModel
        ILogger<Md5HashFileGeneratorViewModel> mockedLogger = Mock.Of<ILogger<Md5HashFileGeneratorViewModel>>();
        Mock<IDialogService> mockedDialogService = new();
        IMd5HashFileGeneratorService mockedMd5Service = Mock.Of<IMd5HashFileGeneratorService>();
        ISnackbarMessageQueue mockedSnackbar = Mock.Of<ISnackbarMessageQueue>();

        //ViewModel
        Md5HashFileGeneratorViewModel sut = new(mockedLogger, mockedDialogService.Object, mockedMd5Service, mockedSnackbar);

        //Necessary actions for the command to not fail such as having a file in the collection and returning a path from a dialog
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(executingDirectory, "hashes.txt");
        mockedDialogService.Setup(x => x.ShowSaveFileDialog()).Returns(filePath);
        Md5HashFile hashFile = new()
        {
            Name = "test",
            Hash = "test",
        };
        sut.Files.Add(hashFile);

        // Act
        sut.SaveCalculatedHashesToFileCommand.Execute(null);

        // Assert
        Assert.True(File.Exists(filePath));

        //Delete the temporary file
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
