using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Models;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.Md5Hash;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.Md5HashFileGenerator;

public partial class Md5HashFileGeneratorViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }
    public NotifyCanExecuteChangedObservableCollection<Md5HashFile> Files { get; }

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IDialogService _dialogService;
    private readonly IMd5HashFileGeneratorService _md5HashFileGeneratorService;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="Md5HashFileGeneratorViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    /// <param name="md5HashFileGeneratorService"><see cref="IMd5HashFileGeneratorService"/> to calculate the MD5 hashes of files.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    /// <param name="soporteAbcContextFactory"><see cref="IDbContextFactory<SoporteAbcContext>"/> factory to be able to create a context to connect and manage the database.</param>
    public Md5HashFileGeneratorViewModel(ILogger<Md5HashFileGeneratorViewModel> logger, IDialogService dialogService,
        IMd5HashFileGeneratorService md5HashFileGeneratorService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialogService = dialogService;
        _md5HashFileGeneratorService = md5HashFileGeneratorService ?? throw new ArgumentNullException(nameof(md5HashFileGeneratorService));

        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        Files = new NotifyCanExecuteChangedObservableCollection<Md5HashFile>(() => NotifyCanExecuteChanged(SaveCalculatedHashesToFileCommand));
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects multiple files to add it to the <see cref="Files"/> collection.
    /// </summary>
    [RelayCommand]
    private void OnSelectFilesAndCalculateHashes()
    {
        string[] selectedFiles = _dialogService.ShowOpenFileDialog();
        if (selectedFiles.Length > 0)
        {
            Files.Clear();

            foreach (string file in selectedFiles)
            {
                Md5HashFile hashFile = new()
                {
                    Name = file.Split(Path.DirectorySeparatorChar).Last(),
                    Hash = _md5HashFileGeneratorService.CalculateMd5Hash(file)
                };

                Files.Add(hashFile);
            }
        }
    }

    /// <summary>
    /// Saves the generated hashes to a file text on the specified path.
    /// </summary>
    /// <param name="path">Where to save the file with all the hashes</param>
    [RelayCommand(CanExecute = nameof(CanSaveToFile))]
    private void OnSaveCalculatedHashesToFile()
    {
        if (!Files.Any())
            throw new ArgumentNullException(nameof(Files));

        string fileSavePath = _dialogService.ShowSaveFileDialog();

        if (string.IsNullOrEmpty(fileSavePath))
            return;

        if (File.Exists(fileSavePath))
            File.Delete(fileSavePath);

        // Create a StreamWriter to write to the file
        using (StreamWriter writer = new(fileSavePath))
        {
            foreach (Md5HashFile file in Files)
            {
                // Write the text to the file
                writer.WriteLine($"Nombre: {file.Name}");
                writer.WriteLine($"MD5: {file.Hash}");
                writer.WriteLine();
            }
        }
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Save to file" button on the UI based on if <see cref="Files"/> is empty or not.
    /// </summary>
    /// <returns>'True' if <see cref="Files"/> has any items inside, 'False' if it doesn't</returns>
    private bool CanSaveToFile() => Files.Any();

    #endregion

    #region ErrorValidations



    #endregion

    #region PrivateMethods

    

    #endregion

    #region CustomNotifyCanExecutedChanged

    /// <summary>
    /// Custom implementation of <see cref="NotifyCanExecuteChangedObservableCollection{T}"/> event to be able to raise <see cref="INotifyPropertyChangedAttribute"/> passing it whatever <see cref="RelayCommand"/> you need to raised the event.
    /// </summary>
    /// <param name="command"><see cref="RelayCommand"/> to have <see cref="NotifyCanExecuteChangedForAttribute"/> raised</param>
    private static void NotifyCanExecuteChanged(IRelayCommand command) => command.NotifyCanExecuteChanged();

    #endregion
}
