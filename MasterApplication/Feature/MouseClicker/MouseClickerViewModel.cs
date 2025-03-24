using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Services.Dialog;
using MasterApplication.UserControls.Dialog;
using MasterApplication.UserControls.ScreenShot;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class MouseClickerViewModel : ObservableObject, IRecipient<WindowActionMessage>
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    public ObservableCollection<AutoClickerSequence> AutoClickerSequences { get; private set; }

    [ObservableProperty]
    private bool _isSequenceComboBoxEnabled;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSequenceCommand))]
    private AutoClickerSequence? _currentSequence;

    [ObservableProperty]
    private bool _isSequenceDetailsVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeTemplateImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeClickCoordinateTemplateImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTemplateImageCommand))]
    private byte[]? _currentShowingImage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextTemplateImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousTemplateImageCommand))]
    private int _currentTemplateImageIndex;

    [ObservableProperty]
    private int _numberOfTemplateImages;

    [ObservableProperty]
    private int _delayBeforeClicking;

    [ObservableProperty]
    private bool _isDelayBeforeClickingTextBoxEnabled;

    [ObservableProperty]
    private bool _isShowCoordinatesToggleButtonVisible;

    #endregion

    #region PrivateFields

    private const string DIALOG_IDENTIFIER = "AutoClickerMenuDialog";
    private const string SEQUENCE_PATH = @"Feature\MouseClicker\Sequences";
    private readonly string _sequencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SEQUENCE_PATH);
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private readonly ScreenShotWindow _screenShotWindow;
    private bool _isChangingExistingTemplateImage = false;












    private bool _isUnsavedChanges = false;
    

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogHost"><see cref="IDialogHost"/> implementation to be able to show the material design dialog host.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IMessenger messenger, IDialogHost dialogHost, ISnackbarMessageQueue snackbarMessageQueue, ScreenShotWindow screenShotWindow)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        //_messenger.Register(this);
        _messenger.Register<AutoClickerTemplate>(this, HandleAutoClickerTemplateMessage);

        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        _screenShotWindow = screenShotWindow ?? throw new ArgumentNullException(nameof(screenShotWindow));

        AutoClickerSequences = new();
        LoadAutoClickerSequences();
        LoadAllSequencesTemplateImagesFromFile();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Loads all the information from the selected sequence.
    /// </summary>
    [RelayCommand]
    private void OnSequenceSelectedItemChanged()
    {
        //TODO: Figure out how to save changes 
        /*if (_isUnsavedChanges)
        {
            ConfirmDialog confirmDialog = new($"There are unsaved changes, do you want to save them before selecting another sequence?");
            object? result = _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER)
                .GetAwaiter()
                .GetResult();

            if (result is bool isSaveChangesCanceled && isSaveChangesCanceled)
            {
                CurrentTemplateImageIndex = 0;
                ShowTemplateImagesForCurrentSequence();
                return;
            }
        }*/
        ShowSequence();
    }

    /// <summary>
    /// Adds a new <see cref="AutoClickerSequence"/> to the list.
    /// </summary>
    [RelayCommand]
    private async Task OnAddSequence()
    {
        IList<string> sequenceNames = AutoClickerSequences.Select(x => x.Name).ToList();
        TextBoxDialog textBoxDialog = new("Add Sequence", "Please introduce a name for the new sequence:", sequenceNames);
        if (await _dialogHost.Show(textBoxDialog, DIALOG_IDENTIFIER) is bool isAddSequenceCanceled && isAddSequenceCanceled)
            return;

        AutoClickerSequence autoClickerSequence = new();
        autoClickerSequence.Name = textBoxDialog.SequenceName;

        AutoClickerSequences.Add(autoClickerSequence);
        CurrentSequence = autoClickerSequence;
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Removes the current selected <see cref="AutoClickerSequence"/> from the list.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteSequence))]
    private async Task OnDeleteSequence()
    {
        ConfirmDialog confirmDialog = new($"Are you sure you want to delete the '{CurrentSequence?.Name}' sequence?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteSequenceCanceled && isDeleteSequenceCanceled)
            return;

        if (!DeleteSequenceFromFile())
        {
            ErrorDialog errorDialog = new($"Error trying to delete the sequence file. Check logs for more information.");
            await _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            return;
        }

        AutoClickerSequences.Remove(CurrentSequence!);
        CurrentSequence = AutoClickerSequences.LastOrDefault();
        IsSequenceComboBoxEnabled = AutoClickerSequences.Any();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Opens the auto clicker menu with all it's options.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenAutoClickerMenu))]
    private void OnOpenAutoClickerMenu()
    {
        _messenger.Send(new WindowActionMessage(Models.Enums.WindowAction.Minimize));
        //_autoClickerMenuView.Show();
        //_autoClickerMenuView.Activate();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Shows the next image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowPreviousImageTemplate))]
    private void OnPreviousTemplateImage()
    {
        CurrentTemplateImageIndex--;
        ShowCurrentTemplateImage();
    }

    /// <summary>
    /// Shows the previous image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowNextImageTemplate))]
    private void OnNextTemplateImage()
    {
        CurrentTemplateImageIndex++;
        ShowCurrentTemplateImage();
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangeTemplateImage))]
    private void OnChangeTemplateImage()
    {
        _isChangingExistingTemplateImage = true;
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        _screenShotWindow.Show();
    }

    /// <summary>
    /// Opens the current template image to change the click coordinates.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangeClickCoordinateTemplateImage))]
    private void OnChangeClickCoordinateTemplateImage()
    {

    }

    /// <summary>
    /// Shows/hides the clicking coordinates on the current template image.
    /// </summary>
    /// <param name="showCoordinates">Flag to show/hide the coordinates on the template image.</param>
    [RelayCommand]
    private void OnShowCoordinatesOnImage(bool showCoordinates)
    {
        var test = showCoordinates;
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand]
    private void OnAddTemplateImage()
    {
        _isChangingExistingTemplateImage = false;
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        _screenShotWindow.ShowDialog();
    }

    /// <summary>
    /// Deletes the current template image from the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteTemplateImage))]
    private async Task OnDeleteTemplateImage()
    {
        ConfirmDialog confirmDialog = new($"Are you sure you want to delete the current template image?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteTemplateImageCanceled && isDeleteTemplateImageCanceled)
            return;

        NumberOfTemplateImages--;
        CurrentSequence?.Templates?.RemoveAt(CurrentTemplateImageIndex);
        if (CurrentTemplateImageIndex > 0)
            CurrentTemplateImageIndex--;

        ShowCurrentTemplateImage();

        if (CurrentSequence?.Templates?.Any() != true)
            IsDelayBeforeClickingTextBoxEnabled = false;

        NotifyCanExecuteChanged(ChangeTemplateImageCommand);
        _isUnsavedChanges = true;
    }

    /// <summary>
    /// Saves the current selected <see cref="AutoClickerSequence"/> to a file.
    /// </summary>
    [RelayCommand]
    private async Task OnSaveSequenceToFile()
    {
        ConfirmDialog confirmDialog = new($"Confirm saving '{CurrentSequence?.Name}' sequence to file?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteSequenceCanceled && isDeleteSequenceCanceled)
            return;

        /*if (await SaveSequenceAndImagesToFile())
            return;*/
    }

    

    

    

    

    

    

    

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Open Menu" button on the UI based on if <see cref="AutoClickerMenuView"/> is visible or not.
    /// </summary>
    /// <returns><see cref="true"/> if <see cref="AutoClickerMenuView"/> isn't visible, <see cref="false"/> if it is.</returns>
    //private bool CanOpenAutoClickerMenu() => _autoClickerMenuView != null && !_autoClickerMenuView.IsVisible && AutoClickerSequences.Any();
    private bool CanOpenAutoClickerMenu() => CurrentSequence != null && CurrentSequence.Templates?.Any() == true;

    /// <summary>
    /// Enables or disables the "Change Template Image" button on the UI based on if there's a showing image.
    /// </summary>
    /// <returns><see cref="true"/> if there's a showing image, <see cref="false"/> if there isn't.</returns>
    private bool CanChangeTemplateImage() => CurrentSequence?.Templates != null && CurrentSequence.Templates.Any();

    /// <summary>
    /// Enables or disables the "Delete Template Image" button on the UI based on if there's a showing image.
    /// </summary>
    /// <returns><see cref="true"/> if there's a showing image, <see cref="false"/> if there isn't.</returns>
    private bool CanDeleteTemplateImage() => CurrentSequence?.Templates != null && CurrentSequence.Templates.Any();

    /// <summary>
    /// Enables or disables the "Next Image" button on the UI based on if it's not on the last image.
    /// </summary>
    /// <returns>'True' if the index is not on the last image, 'False' if it is.</returns>
    private bool CanShowNextImageTemplate() => CurrentTemplateImageIndex < CurrentSequence?.Templates?.Count-1;

    /// <summary>
    /// Enables or disables the "Previous Image" button on the UI based on if it's not on the first image.
    /// </summary>
    /// <returns>'True' if the index is not on the first image, 'False' if it is.</returns>
    private bool CanShowPreviousImageTemplate() => CurrentTemplateImageIndex > 0;

    /// <summary>
    /// Enables or disables the "Add Sequence" button on the UI based on if there's a current selected one.
    /// </summary>
    /// <returns>'True' if there's a current selected sequence, 'False' if it isn't.</returns>
    private bool CanDeleteSequence() => !string.IsNullOrEmpty(CurrentSequence?.Name);

    /// <summary>
    /// Enables or disables the "Change Click Coordinates" button on the UI based on if there's a showing image.
    /// </summary>
    /// <returns>'True' if there's a showing image, 'False' if there isn't.</returns>
    public bool CanChangeClickCoordinateTemplateImage() { IsShowCoordinatesToggleButtonVisible = CurrentShowingImage != null; return CurrentShowingImage != null; }

    #endregion

    #region PublicMethods

    /// <summary>
    /// <see cref="IRecipient{TMessage}"/>' implementation to process different messages received from all parts of the application.
    /// </summary>
    /// <param name="message"><see cref="WindowActionMessage"/> message to bring the window into focus.</param>
    internal void Receive(WindowActionMessage message)
    {
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Loads all the available <see cref="AutoClickerSequence"/>.
    /// </summary>
    private void LoadAutoClickerSequences()
    {
        IList<string> files = Directory.GetDirectories(_sequencePath);
        if (!files.Any())
        {
            _logger.LogWarning("No sequences found in '{sequencePath}'", _sequencePath);
            return;
        }

        AutoClickerSequence? autoClickerSequence = null;
        foreach (string file in files)
        {
            try
            {
                string jsonString = File.ReadAllText(Path.Combine(file, "Sequence.json"));
                autoClickerSequence = JsonSerializer.Deserialize<AutoClickerSequence>(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error trying to load auto clicker sequence for file '{fileName}'. {ex}", Path.GetFileName(file), ex);
            }

            if (autoClickerSequence == null)
                continue;

            AutoClickerSequences.Add(autoClickerSequence);
        }

        IsSequenceComboBoxEnabled = AutoClickerSequences.Any();
    }

    /// <summary>
    /// Loads all the template images of the current sequence from the .json file.
    /// </summary>
    private void LoadAllSequencesTemplateImagesFromFile()
    {
        foreach (AutoClickerSequence sequence in AutoClickerSequences)
        {
            if (sequence.Templates == null)
                continue;

            foreach (AutoClickerTemplate autoClickerTemplate in sequence.Templates)
            {
                try
                {
                    autoClickerTemplate.Image = File.ReadAllBytes(autoClickerTemplate.ImagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error trying to load sequence template image '{path}'. {ex}", autoClickerTemplate.ImagePath, ex);
                }
            }
        }
    }

    /// <summary>
    /// Shows the <see cref="CurrentSequence"/> to the user.
    /// </summary>
    private void ShowSequence()
    {
        try
        {
            ResetSequenceDetails();
            if (CurrentSequence == null)
            {
                IsSequenceDetailsVisible = false;
                return;
            }

            IsSequenceDetailsVisible = true;
            NotifyAllTemplateCommands();
            if (CurrentSequence?.Templates == null || !CurrentSequence.Templates.Any())
                return;

            NumberOfTemplateImages = CurrentSequence.Templates.Count;
            CurrentShowingImage = CurrentSequence.Templates[CurrentTemplateImageIndex].Image;
            DelayBeforeClicking = CurrentSequence.Templates[CurrentTemplateImageIndex].ClickCoordinatesInterval;
            IsDelayBeforeClickingTextBoxEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to load sequence template image. {ex}", ex);
        }
    }

    /// <summary>
    /// Resets all the details of a <see cref="AutoClickerSequence"/> to show to the user.
    /// </summary>
    private void ResetSequenceDetails()
    {
        IsSequenceDetailsVisible = false;
        CurrentShowingImage = null;
        CurrentTemplateImageIndex = 0;
        NumberOfTemplateImages = 0;
        DelayBeforeClicking = 0;
        IsDelayBeforeClickingTextBoxEnabled = false;
        IsShowCoordinatesToggleButtonVisible = false;
    }

    /// <summary>
    /// Notifies all available commands.
    /// </summary>
    private void NotifyAllTemplateCommands()
    {
        NotifyCanExecuteChanged(NextTemplateImageCommand);
        NotifyCanExecuteChanged(PreviousTemplateImageCommand);
        NotifyCanExecuteChanged(ChangeTemplateImageCommand);
        NotifyCanExecuteChanged(ChangeClickCoordinateTemplateImageCommand);
        NotifyCanExecuteChanged(DeleteTemplateImageCommand);
        NotifyCanExecuteChanged(SaveSequenceToFileCommand);
    }

    /// <summary>
    /// Shows the current template image to the user.
    /// </summary>
    private void ShowCurrentTemplateImage()
    {
        if (CurrentSequence?.Templates?.Any() != true)
            return;

        CurrentShowingImage = CurrentSequence.Templates[CurrentTemplateImageIndex].Image;
    }

    /// <summary>
    /// Handles the message received from the <see cref="ScreenShotSelection"/> view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="autoClickerTemplate"><see cref="AutoClickerTemplate"/> with the selection made by the user and where to click on the image.</param>
    private void HandleAutoClickerTemplateMessage(object sender, AutoClickerTemplate autoClickerTemplate)
    {
        if (CurrentSequence!.Templates == null)
            CurrentSequence.Templates = new List<AutoClickerTemplate>();

        if (_isChangingExistingTemplateImage)
        {
            CurrentSequence.Templates[CurrentTemplateImageIndex] = autoClickerTemplate;
            ShowCurrentTemplateImage();
            return;
        }

        // We're adding a new template image after the specified index. We add one to the index to show the correct number on the UI.
        CurrentSequence.Templates.Insert(CurrentTemplateImageIndex, autoClickerTemplate);
        ShowCurrentTemplateImage();
        IsDelayBeforeClickingTextBoxEnabled = true;
    }

    /// <summary>
    /// Deletes the current sequence from file.
    /// </summary>
    private bool DeleteSequenceFromFile()
    {
        try
        {
            string directoryPath = Path.Combine(_sequencePath, CurrentSequence?.Name ?? string.Empty);
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to delete sequence file file. {ex}", ex);
            return false;
        }
    }

    /// <summary>
    /// Saves the current sequence to a file.
    /// </summary>
    private bool SaveSequenceAndImagesToFile()
    {
        return true;
        /*string directoryPath = Path.Combine(_sequencePath, CurrentSequence.Name);

        try
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(CurrentSequence, options);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(Path.Combine(directoryPath, "Sequence.json"), jsonString);
            return true;
        }
        catch (Exception ex)
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            ErrorDialog errorDialog = new($"Error trying to save the sequence to file. Exception: '{ex.Message}'.");
            await _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            _logger.LogError("Error trying to save sequence to file. {ex}", ex);
            return false;
        }*/
    }

    

    



    //TODO: When we add a new sequence if there are thing unsaved on the previous one we have to tell the user so that he can save the changes
    //TODO: When saving all changes to files make sure the naming are correct because we could have added templates in between existing ones
    //  that are already saved to file with their name.
    //TODO: Fix the screnshot selection to not show the white borders

    #endregion

    #region CustomNotifyCanExecutedChanged

    /// <summary>
    /// Custom implementation of <see cref="NotifyCanExecuteChangedObservableCollection{T}"/> event to be able to raise <see cref="INotifyPropertyChangedAttribute"/> passing it whatever <see cref="RelayCommand"/> you need to raised the event.
    /// </summary>
    /// <param name="command"><see cref="RelayCommand"/> to have <see cref="NotifyCanExecuteChangedForAttribute"/> raised.</param>
    private static void NotifyCanExecuteChanged(IRelayCommand command) => command.NotifyCanExecuteChanged();

    #endregion


    void IRecipient<WindowActionMessage>.Receive(WindowActionMessage message)
    {
        throw new NotImplementedException();
    }
}
