using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Models.Structs;
using MasterApplication.Services.Dialog;
using MasterApplication.UserControls.Dialog;
using MasterApplication.UserControls.ScreenShot;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class MouseClickerViewModel : ObservableObject
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
    private int _delayAfterClicking;

    [ObservableProperty]
    private double _matchThreshold;

    [ObservableProperty]
    private bool _isDelayBeforeAndAfterClickingTextBoxEnabled;

    [ObservableProperty]
    private System.Windows.Point clickCoordinates;

    [ObservableProperty]
    private bool _isShowCoordinatesToggleButtonVisible;

    [ObservableProperty]
    private bool _monitorForChange;

    [ObservableProperty]
    private int _monitorForChangeInterval;

    [ObservableProperty]
    private string _startKeybindName;

    [ObservableProperty]
    private string _stopKeybindName;

    #endregion

    #region PrivateFields

    private const string DIALOG_IDENTIFIER = "AutoClickerMenuDialog";
    private const string SEQUENCE_PATH = @"Feature\MouseClicker\Sequences";
    private readonly string _sequencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SEQUENCE_PATH);
    private string _templateImagePath = string.Empty;
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private readonly IScreenShotWindowFactory _screenShotWindowFactory;
    private readonly AutoClickerMenuViewModelFactory _autoClickerMenuViewModelFactory;
    private readonly KeybindDialog _keybindDialog;
    private bool _isChangingExistingTemplateImage = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogHost"><see cref="IDialogHost"/> implementation to be able to show the material design dialog host.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    /// <param name="screenShotWindowFactory"><see cref="IScreenShotWindowFactory"/> that can create a <see cref="ScreenShotWindow"/> instance.</param>
    /// <param name="keybindDialog"><see cref="KeybindDialog"/> to allow the user to set keybindings.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IMessenger messenger, IDialogHost dialogHost, ISnackbarMessageQueue snackbarMessageQueue, IScreenShotWindowFactory screenShotWindowFactory, 
        AutoClickerMenuViewModelFactory autoClickerMenuViewModelFactory, KeybindDialog keybindDialog)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _messenger.Register<ScreenShotMessage>(this, HandleScreenShotMessage);

        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        _screenShotWindowFactory = screenShotWindowFactory ?? throw new ArgumentNullException(nameof(screenShotWindowFactory));
        _autoClickerMenuViewModelFactory = autoClickerMenuViewModelFactory ?? throw new ArgumentNullException(nameof(autoClickerMenuViewModelFactory));
        _keybindDialog = keybindDialog ?? throw new ArgumentNullException(nameof(keybindDialog));

        AutoClickerSequences = new();
        StartKeybindName = string.Empty;
        StopKeybindName = string.Empty;
        MatchThreshold = 0.0;

        ResetSequenceDetails();
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
        _templateImagePath = Path.Combine(_sequencePath, @$"{CurrentSequence?.Name}\Images");
        ResetSequenceDetails();
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
        IsSequenceComboBoxEnabled = true;
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
        CurrentSequence = null;
        IsSequenceComboBoxEnabled = AutoClickerSequences.Any();
        ResetSequenceDetails();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Opens the auto clicker menu with all it's options.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenAutoClickerMenu))]
    private void OnOpenAutoClickerMenu()
    {
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        AutoClickerMenuView view = new AutoClickerMenuView();
        view.DataContext = _autoClickerMenuViewModelFactory.Create(CurrentSequence!);
        view.Show();
        view.Activate();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Shows the next image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowPreviousImageTemplate))]
    private void OnPreviousTemplateImage()
    {
        CurrentTemplateImageIndex--;
        CurrentShowingImage = CurrentSequence?.Templates[CurrentTemplateImageIndex].Image;
        DelayBeforeClicking = CurrentSequence?.Templates[CurrentTemplateImageIndex].DelayBeforeClicking ?? 0;
        MatchThreshold = CurrentSequence?.Templates[CurrentTemplateImageIndex].MatchThreshold ?? 0;
        MonitorForChange = CurrentSequence?.Templates[CurrentTemplateImageIndex].MonitorForChange ?? false;
        MonitorForChangeInterval = CurrentSequence?.Templates[CurrentTemplateImageIndex].MonitorForChangeInterval ?? 0;
    }

    /// <summary>
    /// Shows the previous image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowNextImageTemplate))]
    private void OnNextTemplateImage()
    {
        CurrentTemplateImageIndex++;
        CurrentShowingImage = CurrentSequence?.Templates[CurrentTemplateImageIndex].Image;
        DelayBeforeClicking = CurrentSequence?.Templates[CurrentTemplateImageIndex].DelayBeforeClicking ?? 0;
        MatchThreshold = CurrentSequence?.Templates[CurrentTemplateImageIndex].MatchThreshold ?? 0;
        MonitorForChange = CurrentSequence?.Templates[CurrentTemplateImageIndex].MonitorForChange ?? false;
        MonitorForChangeInterval = CurrentSequence?.Templates[CurrentTemplateImageIndex].MonitorForChangeInterval ?? 0;
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangeTemplateImage))]
    private void OnChangeTemplateImage()
    {
        _isChangingExistingTemplateImage = true;
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        ScreenShotWindow screenShotWindow = _screenShotWindowFactory.Create();
        screenShotWindow.Show();
    }

    /// <summary>
    /// Opens the current template image to change the click coordinates.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangeClickCoordinateTemplateImage))]
    private void OnChangeClickCoordinateTemplateImage()
    {
        var test = "NotImplemented";
    }

    /// <summary>
    /// Shows/hides the clicking coordinates on the current template image.
    /// </summary>
    /// <param name="showCoordinates">Flag to show/hide the coordinates on the template image.</param>
    [RelayCommand]
    private void OnShowCoordinatesOnImage(bool showCoordinates)
    {
        if (showCoordinates)
            ClickCoordinates = CurrentSequence?.Templates[CurrentTemplateImageIndex].ClickCoordinates ?? new System.Windows.Point(0,0);
        else
            ClickCoordinates = new System.Windows.Point(0,0);
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand]
    private void OnAddTemplateImage()
    {
        _isChangingExistingTemplateImage = false;
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        ScreenShotWindow screenShotWindow = _screenShotWindowFactory.Create();
        screenShotWindow.IsSearchingBoundsScreenshot = false;
        screenShotWindow.Show();
        screenShotWindow.Activate();
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

        ShowSequence();
    }

    /// <summary>
    /// Sets the start keybind for the <see cref="AutoClickerSequence"/>.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSetKeybinds))]
    private async Task OnSetKeybind(string keybindType)
    {
        if (await _dialogHost.Show(_keybindDialog, DIALOG_IDENTIFIER) is bool isSettingStartKeybindCanceled && isSettingStartKeybindCanceled)
            return;

        string keyName = _keybindDialog.KeybindKey.KeyName;
        int keyCode = _keybindDialog.KeybindKey.KeyCode;
        Keybind newKeybind = new(keyName, keyCode);

        if (keybindType.Equals("Start", StringComparison.OrdinalIgnoreCase))
        {
            StartKeybindName = keyName;
            CurrentSequence!.StartKeybind = newKeybind;
            return;
        }

        StopKeybindName = keyName;
        CurrentSequence!.StopKeybind = newKeybind;
    }

    /// <summary>
    /// Sets the location of where to find the <see cref="AutoClickerTemplate"/>.
    /// </summary>
    [RelayCommand]
    private void OnSetSearchingRegion()
    {
        _messenger.Send(new WindowActionMessage(WindowAction.Minimize));
        ScreenShotWindow screenShotWindow = _screenShotWindowFactory.Create();
        screenShotWindow.IsSearchingBoundsScreenshot = true;
        screenShotWindow.Show();
        screenShotWindow.Activate();
    }

    /// <summary>
    /// Saves the current selected <see cref="AutoClickerSequence"/> to a file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveSequenceToFile))]
    private async Task OnSaveSequenceToFile()
    {
        ConfirmDialog confirmDialog = new($"Confirm saving '{CurrentSequence?.Name}' sequence to file?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteSequenceCanceled && isDeleteSequenceCanceled)
            return;

        await IsCurrentSequenceSavedToFile();
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Open Menu" button on the UI based on if <see cref="AutoClickerMenuView"/> is visible or not.
    /// </summary>
    /// <returns><see cref="true"/> if <see cref="AutoClickerMenuView"/> isn't visible, <see cref="false"/> if it is.</returns>
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
    /// <returns><see cref="true"/> if the index is not on the last image, <see cref="false"/> if it is.</returns>
    private bool CanShowNextImageTemplate() => CurrentTemplateImageIndex < CurrentSequence?.Templates?.Count - 1;

    /// <summary>
    /// Enables or disables the "Previous Image" button on the UI based on if it's not on the first image.
    /// </summary>
    /// <returns><see cref="true"/> if the index is not on the first image, <see cref="false"/> if it is.</returns>
    private bool CanShowPreviousImageTemplate() => CurrentTemplateImageIndex > 0;

    /// <summary>
    /// Enables or disables the "Add Sequence" button on the UI based on if there's a current selected one.
    /// </summary>
    /// <returns><see cref="true"/> if there's a current selected sequence, <see cref="false"/> if it isn't.</returns>
    private bool CanDeleteSequence() => !string.IsNullOrEmpty(CurrentSequence?.Name);

    /// <summary>
    /// Enables or disables the "Set Start/Stop Keybind" buttons on the UI based on if there's a current selected one.
    /// </summary>
    /// <returns><see cref="true"/> if there's a current selected sequence, <see cref="false"/> if it isn't.</returns>
    private bool CanSetKeybinds() => !string.IsNullOrEmpty(CurrentSequence?.Name);

    /// <summary>
    /// Enables or disables the "Save Sequence to File" button on the UI based on if there's a change in the current sequence.
    /// </summary>
    /// <returns><see cref="true"/> if there's a change in the current sequence, <see cref="false"/> if there isn't.</returns>
    private bool CanSaveSequenceToFile() => IsSequenceChanged();

    /// <summary>
    /// Enables or disables the "Change Click Coordinates" button on the UI based on if there's a showing image.
    /// </summary>
    /// <returns><see cref="true"/> if there's a showing image, <see cref="false"/> if there isn't.</returns>
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
            IsSequenceDetailsVisible = true;
            if (CurrentSequence?.Templates == null || !CurrentSequence.Templates.Any())
            {
                CurrentShowingImage = null;
                IsDelayBeforeAndAfterClickingTextBoxEnabled = false;
                NotifyAllTemplateCommands();
                return;
            }

            NumberOfTemplateImages = CurrentSequence.Templates.Count;
            CurrentShowingImage = CurrentSequence.Templates[CurrentTemplateImageIndex].Image;
            StartKeybindName = CurrentSequence.StartKeybind.KeyName;
            StopKeybindName = CurrentSequence.StopKeybind.KeyName;
            DelayBeforeClicking = CurrentSequence.Templates[CurrentTemplateImageIndex].DelayBeforeClicking;
            DelayAfterClicking = CurrentSequence.Templates[CurrentTemplateImageIndex].DelayAfterClicking;
            MonitorForChange = CurrentSequence.Templates[CurrentTemplateImageIndex].MonitorForChange;
            MonitorForChangeInterval = CurrentSequence.Templates[CurrentTemplateImageIndex].MonitorForChangeInterval;
            MatchThreshold = CurrentSequence.Templates[CurrentTemplateImageIndex].MatchThreshold;
            IsDelayBeforeAndAfterClickingTextBoxEnabled = true;
            NotifyAllTemplateCommands();
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
        MatchThreshold = 0.0;
        MonitorForChange = false;
        MonitorForChangeInterval = 0;
        IsDelayBeforeAndAfterClickingTextBoxEnabled = false;
        IsShowCoordinatesToggleButtonVisible = false;
        StartKeybindName = string.Empty;
        StopKeybindName = string.Empty;
    }

    /// <summary>
    /// Notifies all available commands.
    /// </summary>
    private void NotifyAllTemplateCommands()
    {
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
        NotifyCanExecuteChanged(NextTemplateImageCommand);
        NotifyCanExecuteChanged(PreviousTemplateImageCommand);
        NotifyCanExecuteChanged(ChangeTemplateImageCommand);
        NotifyCanExecuteChanged(ChangeClickCoordinateTemplateImageCommand);
        NotifyCanExecuteChanged(SetKeybindCommand);
        NotifyCanExecuteChanged(DeleteTemplateImageCommand);
        NotifyCanExecuteChanged(SaveSequenceToFileCommand);
    }

    /// <summary>
    /// Handles the message received from the <see cref="ScreenShotSelection"/> view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screenShotMessage"><see cref="ScreenShotMessage"/> with the selection made by the user and where to click on the image and the template.</param>
    private void HandleScreenShotMessage(object sender, ScreenShotMessage screenShotMessage)
    {
        if (screenShotMessage.IsSearchingBoundsScreenshot)
        {
            CurrentSequence!.TemplateSearchBounds = screenShotMessage.TemplateBounds;
            return;
        }

        if (_isChangingExistingTemplateImage)
        {
            CurrentSequence!.Templates[CurrentTemplateImageIndex] = screenShotMessage.AutoClickerTemplate;
            CurrentSequence!.Templates[CurrentTemplateImageIndex].ImagePath = Path.Combine(_templateImagePath, $"{CurrentTemplateImageIndex}.jpg");
            ShowSequence();
            return;
        }

        // We're adding a new template image after the specified index. We add one to the index to show the correct number on the UI.
        screenShotMessage.AutoClickerTemplate.ImagePath = Path.Combine(_templateImagePath, $"{CurrentSequence?.Templates.Count}.jpg");
        CurrentSequence!.Templates.Insert(CurrentSequence.Templates.Count, screenShotMessage.AutoClickerTemplate);
        ShowSequence();
    }

    /// <summary>
    /// Handles the message received from the <see cref="ScreenShotSelection"/> view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="templateBounds"><see cref="Rectangle"/> with the selection made by the user to know the location and size.</param>
    private void HandleTemplateBoundsMessage(object sender, Rectangle templateBounds)
    {
        CurrentSequence!.TemplateSearchBounds = templateBounds;
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
    private async Task<bool> IsCurrentSequenceSavedToFile()
    {
        string directoryPath = Path.Combine(_sequencePath, CurrentSequence?.Name ?? string.Empty);

        try
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(CurrentSequence, options);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(Path.Combine(directoryPath, "Sequence.json"), jsonString);
            await SaveTemplateImagesToFile();
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
        }
    }

    /// <summary>
    /// Saves all the <see cref="AutoClickerTemplate"/> images of the <see cref="CurrentSequence"/> to local files.
    /// </summary>
    private async Task SaveTemplateImagesToFile()
    {
        try
        {
            if (CurrentSequence?.Templates.Any() != true)
                return;

            if (!Directory.Exists(_templateImagePath))
                Directory.CreateDirectory(_templateImagePath);

            foreach (AutoClickerTemplate template in CurrentSequence.Templates)
            {
                if (template.Image != null)
                    File.WriteAllBytes(template.ImagePath, template.Image);
            }
        }
        catch (Exception ex)
        {
            ErrorDialog errorDialog = new($"Error trying to save template images to file. Exception: '{ex.Message}'.");
            await _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            _logger.LogError("Error trying to save template images to file. {ex}", ex);
            throw;
        }
    }

    /// <summary>
    /// Checks to see if there's a change in the current sequence.
    /// </summary>
    /// <returns><see cref="true"/> if the current sequence changed, <see cref="false"/> if it's the same.</returns>
    private bool IsSequenceChanged()
    {
        return true;
        /*if (CurrentSequence?.Name != _originalCurrentSequence?.Name)
            return true;

        if (!Equals(CurrentSequence?.StartKeybind, _originalCurrentSequence?.StartKeybind))
            return true;

        if (!Equals(CurrentSequence?.StopKeybind, _originalCurrentSequence?.StopKeybind))
            return true;

        if (CurrentSequence?.Templates.Count != _originalCurrentSequence?.Templates.Count)
            return true;

        for (int i = 0; i < CurrentSequence?.Templates.Count; i++)
        {
            var current = CurrentSequence.Templates[i];
            var original = _originalCurrentSequence?.Templates[i];

            if (current.ImagePath != original?.ImagePath ||
                !current.ClickCoordinates.Equals(original.ClickCoordinates) ||
                current.DelayBeforeClicking != original.DelayBeforeClicking ||
                current.DelayAfterClicking != original.DelayAfterClicking ||
                current.ResetPosition != original.ResetPosition ||
                current.MonitorForChange != original.MonitorForChange ||
                current.MonitorForChangeInterval != original.MonitorForChangeInterval)
            {
                return true;
            }
        }

        return false;*/
    }

    #endregion

    #region ViewEventsOverride

    /// <summary>
    /// Handles when the "MatchThreshold" textbox changes.
    /// </summary>
    /// <param name="value">New value.</param>
    partial void OnMatchThresholdChanged(double value)
    {
        if (CurrentSequence?.Templates.Any() != true)
            return;

        CurrentSequence!.Templates[CurrentTemplateImageIndex].MatchThreshold = value;
    }

    /// <summary>
    /// Handles when the "DelayBeforeClicking" textbox changes.
    /// </summary>
    /// <param name="value">New value.</param>
    partial void OnDelayBeforeClickingChanged(int value)
    {
        if (CurrentSequence?.Templates.Any() != true)
            return;

        CurrentSequence!.Templates[CurrentTemplateImageIndex].DelayBeforeClicking = value;
    }

    /// <summary>
    /// Handles when the "DelayAfterClicking" textbox changes.
    /// </summary>
    /// <param name="value">New value.</param>
    partial void OnDelayAfterClickingChanged(int value)
    {
        if (CurrentSequence?.Templates.Any() != true)
            return;

        CurrentSequence!.Templates[CurrentTemplateImageIndex].DelayAfterClicking = value;
    }

    /// <summary>
    /// Handles when the "MonitorForChange" checkbox changes.
    /// </summary>
    /// <param name="value">New value.</param>
    partial void OnMonitorForChangeChanged(bool value)
    {
        if (CurrentSequence?.Templates.Any() != true)
            return;

        CurrentSequence!.Templates[CurrentTemplateImageIndex].MonitorForChange = value;
    }

    /// <summary>
    /// Handles when the "MonitorForChangeInterval" textbox changes.
    /// </summary>
    /// <param name="value">New value.</param>
    partial void OnMonitorForChangeIntervalChanged(int value)
    {
        if (CurrentSequence?.Templates.Any() != true)
            return;

        CurrentSequence!.Templates[CurrentTemplateImageIndex].MonitorForChangeInterval = value;
    }

    #endregion

    #region CustomNotifyCanExecutedChanged

    /// <summary>
    /// Custom implementation of <see cref="NotifyCanExecuteChangedObservableCollection{T}"/> event to be able to raise <see cref="INotifyPropertyChangedAttribute"/> passing it whatever <see cref="RelayCommand"/> you need to raised the event.
    /// </summary>
    /// <param name="command"><see cref="RelayCommand"/> to have <see cref="NotifyCanExecuteChangedForAttribute"/> raised.</param>
    private static void NotifyCanExecuteChanged(IRelayCommand command) => command.NotifyCanExecuteChanged();

    #endregion
}
