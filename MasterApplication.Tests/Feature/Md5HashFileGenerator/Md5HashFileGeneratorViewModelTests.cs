using MasterApplication.Feature.Md5HashFileGenerator;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Md5Hash;

namespace MasterApplication.Tests.Feature.Md5HashFileGenerator;

//This attribute generates tests for MainWindowViewModel that
//asserts all constructor arguments are checked for null
[ConstructorTests(typeof(MainWindowViewModel))]
public partial class Md5HashFileGeneratorViewModelTests
{
    [Fact]
    public void SelectFilesCommand_Execute_CalculatesFilesHashes()
    {
        //Create temporary file to open and calculate hash.
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(executingDirectory, "test.txt");

        // Create a StreamWriter to write to the file
        using (StreamWriter writer = new(filePath))
        {
            // Write the text to the file
            writer.WriteLine("hash test");
        }

        //Arrange
        AutoMocker mocker = new();
        Md5HashFileGeneratorViewModel viewModel = mocker.CreateInstance<Md5HashFileGeneratorViewModel>();
        Md5FileHashService md5FileHashService = mocker.Get<Md5FileHashService>();
        string hash = md5FileHashService.CalculateMd5Hash(filePath);

        mocker.GetMock<IOpenFileDialogService>()
                .Setup(x => x.ShowDialog())
                .Returns(new string[] { filePath });

        mocker.GetMock<IMd5FileHashService>()
            .Setup(x => x.CalculateMd5Hash(filePath))
            .Returns(hash);

        //Act
        viewModel.SelectFilesCommand.Execute(null);

        // Assert
        // Verify that the Files collection in the view model contains the expected number of items
        Assert.Single(viewModel.Files);

        // Verify that the hash of the file was calculated correctly
        Assert.Equal(hash, viewModel.Files[0].Hash);

        //Delete the temporary file
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
