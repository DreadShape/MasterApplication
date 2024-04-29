using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Models;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Md5Hash;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.Md5HashFileGenerator;

public partial class Md5HashFileGeneratorViewModel : ObservableObject
{
    #region Properties

    public NotifyCanExecuteChangedObservableCollection<Md5HashFile> Files { get; }

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IOpenFileDialogService _openFileDialogService;
    private readonly IMd5FileHashService _md5FileHashService;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="Md5HashFileGeneratorViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="openFileDialogService"><see cref="IOpenFileDialogService"/> to open dialog and let the user select files.</param>
    /// <param name="md5FileHashService"><see cref="IMd5FileHashService"/> to calculate the hashes of files.</param>
    /// <param name="soporteAbcContextFactory"><see cref="IDbContextFactory<SoporteAbcContext>"/> factory to be able to create a context to connect and manage the database.</param>
    public Md5HashFileGeneratorViewModel(ILogger<Md5HashFileGeneratorViewModel> logger, IOpenFileDialogService openFileDialogService,
        IMd5FileHashService md5FileHashService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _openFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        _md5FileHashService = md5FileHashService ?? throw new ArgumentNullException(nameof(md5FileHashService));

        Files = new NotifyCanExecuteChangedObservableCollection<Md5HashFile>(() => NotifyCanExecuteChanged(SaveToFileCommand));
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects multiple files to add it to the <see cref="Files"/> collection.
    /// </summary>
    [RelayCommand]
    private void OnSelectFiles()
    {
        string[] files = _openFileDialogService.ShowDialog();
        if (files.Length > 0)
        {
            Files.Clear();

            foreach (string file in files)
            {
                Md5HashFile hashFile = new()
                {
                    Name = file.Split(Path.DirectorySeparatorChar).Last(),
                    Hash = _md5FileHashService.CalculateMd5Hash(file)
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
    private void OnSaveToFile(string path)
    {
        SaveToFileCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region CommandsValidations

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
    private void NotifyCanExecuteChanged(IRelayCommand command) => command.NotifyCanExecuteChanged();

    #endregion
}
