using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Models.Messages;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.MouseClicker;
using MasterApplication.UserControls;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class MouseClickerViewModel : ObservableObject, IRecipient<BringToFrontWindowMessage>
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    public ObservableCollection<AutoClickerSequence> AutoClickerSequences { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSequenceCommand))]
    private AutoClickerSequence _currentSequence;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeTemplateImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeClickCoordinateTemplateImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveSequenceToFileCommand))]
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
    private bool _isSequenceDetailsVisible;

    [ObservableProperty]
    private bool _isDelayBeforeClickingTextBoxEnabled;

    #endregion

    #region PropertiesOnChangedMethods

    /// <summary>
    /// Gets called when the property changes value.
    /// </summary>
    /// <param name="oldValue">Old value of the property.</param>
    /// <param name="newValue">New value of the property.</param>
    partial void OnDelayBeforeClickingChanged(int oldValue, int newValue)
    {
        if (!CurrentSequence.Templates.Any())
            return;

        if (_lastShownSequence.Name.Equals(CurrentSequence.Name, StringComparison.OrdinalIgnoreCase))
            _isUnsavedChanges = true;

        CurrentSequence.Templates[CurrentTemplateImageIndex].DelayBeforeClicking = newValue;
    }

    #endregion

    #region PrivateFields

    private const string DIALOG_IDENTIFIER = "AutoClickerMenuDialog";
    private const string SEQUENCE_PATH = @"Feature\MouseClicker\Sequences";
    private readonly string _sequencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SEQUENCE_PATH);
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private readonly IServiceProvider _serviceProvider;

    private readonly AutoClickerMenuView _autoClickerMenuView;
    private readonly AutoClickerMenuViewModel _autoClickerMenuViewModel;

    private AutoClickerSequence _lastShownSequence = new AutoClickerSequence();
    private bool _isChangingTemplate = false;
    private bool _isUnsavedChanges = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogHost"><see cref="IDialogHost"/> implementation to be able to show the material design dialog host.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> to get access to the dependency injector container.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IMessenger messenger, IDialogHost dialogHost, 
        IServiceProvider serviceProvider, ISnackbarMessageQueue snackbarMessageQueue)
    {
        AutoClickerSequences = new ObservableCollection<AutoClickerSequence>();
        CurrentSequence = new AutoClickerSequence();
        CurrentTemplateImageIndex = 0;
        NumberOfTemplateImages = 0;
        DelayBeforeClicking = 0;
        IsSequenceDetailsVisible = false;
        IsDelayBeforeClickingTextBoxEnabled = false;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _messenger.Register(this);
        _messenger.Register<AutoClickerTemplate>(this, HandleAutoClickerTemplateMessage);

        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _autoClickerMenuView = new();
        _autoClickerMenuViewModel = _serviceProvider.GetRequiredService<AutoClickerMenuViewModel>();
        _autoClickerMenuView.DataContext = _autoClickerMenuViewModel;

        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));

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
        if (CurrentSequence == null)
        {
            IsSequenceDetailsVisible = false;
            return;
        }

        //TODO: Figure out how to save changes 
        if (_isUnsavedChanges)
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
        }

        CurrentTemplateImageIndex = 0;
        ShowTemplateImagesForCurrentSequence();
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
        IsDelayBeforeClickingTextBoxEnabled = false;
        IsSequenceDetailsVisible = true;

        AutoClickerSequences.Add(autoClickerSequence);
        ShowSequence(autoClickerSequence);
    }

    /// <summary>
    /// Removes the current selected <see cref="AutoClickerSequence"/> from the list.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteSequence))]
    private async Task OnDeleteSequence()
    {
        ConfirmDialog confirmDialog = new($"Are you sure you want to delete the '{CurrentSequence.Name}' sequence?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteSequenceCanceled && isDeleteSequenceCanceled)
            return;

        await DeleteSequenceFromFile();
        AutoClickerSequences.Remove(CurrentSequence);
        ShowSequence(null);
    }

    /// <summary>
    /// Saves the current selected <see cref="AutoClickerSequence"/> to a file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteOrChangeTemplateImage))]
    private async Task OnSaveSequenceToFile()
    {
        ConfirmDialog confirmDialog = new($"Confirm saving '{CurrentSequence.Name}' sequence to file?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteSequenceCanceled && isDeleteSequenceCanceled)
            return;

        /*if (await SaveSequenceAndImagesToFile())
            return;*/
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteOrChangeTemplateImage))]
    private async Task OnChangeTemplateImage()
    {
        _isChangingTemplate = true;
        // Show the selection overlay
        IKeyboardService keyboardService = _serviceProvider.GetRequiredService<IKeyboardService>();
        ScreenShotWindow screenShotWindow = new(keyboardService, _messenger);
        _messenger.Send(new MinimizeWindowMessage());
        await Task.Delay(170);
        screenShotWindow.ShowDialog();
    }

    /// <summary>
    /// Opens the current template image to change the click coordinates.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteOrChangeTemplateImage))]
    private async Task OnChangeClickCoordinateTemplateImage()
    {
        
    }

    /// <summary>
    /// Opens the <see cref="ScreenShotWindow"/> to let the user select a new region for a template.
    /// </summary>
    [RelayCommand]
    private async Task OnAddTemplateImage()
    {
        _isChangingTemplate = false;
        // Show the selection overlay
        IKeyboardService keyboardService = _serviceProvider.GetRequiredService<IKeyboardService>();
        ScreenShotWindow screenShotWindow = new(keyboardService, _messenger);
        _messenger.Send(new MinimizeWindowMessage());
        await Task.Delay(170);
        screenShotWindow.ShowDialog();
    }

    /// <summary>
    /// Deletes the current template image from the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteOrChangeTemplateImage))]
    private async Task OnDeleteTemplateImage()
    {
        if (CurrentShowingImage == null)
            return;

        ConfirmDialog confirmDialog = new($"Are you sure you want to delete the current template image?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isDeleteTemplateImageCanceled && isDeleteTemplateImageCanceled)
            return;

        NumberOfTemplateImages--;
        CurrentSequence.Templates.RemoveAt(CurrentTemplateImageIndex);
        if (!CurrentSequence.Templates.Any())
        {
            DelayBeforeClicking = 0;
            IsDelayBeforeClickingTextBoxEnabled = false;
        }

        // We only subtract one from the index if we're not on the first image
        if (CurrentTemplateImageIndex > 0)
            CurrentTemplateImageIndex--;

        ShowTemplateImagesForCurrentSequence(CurrentTemplateImageIndex);
        _isUnsavedChanges = true;
    }

    /// <summary>
    /// Shows the next image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowPreviousImageTemplate))]
    private void OnPreviousTemplateImage()
    {
        CurrentTemplateImageIndex--;
        CurrentShowingImage = CurrentSequence.Templates[CurrentTemplateImageIndex].Image;
        DelayBeforeClicking = CurrentSequence.Templates[CurrentTemplateImageIndex].DelayBeforeClicking;
    }

    /// <summary>
    /// Shows the previous image on the sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowNextImageTemplate))]
    private void OnNextTemplateImage()
    {
        CurrentTemplateImageIndex++;
        CurrentShowingImage = CurrentSequence.Templates[CurrentTemplateImageIndex].Image;
        DelayBeforeClicking = CurrentSequence.Templates[CurrentTemplateImageIndex].DelayBeforeClicking;
    }

    /// <summary>
    /// Opens the auto clicker menu with all it's options.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenAutoClickerMenu))]
    private void OnOpenAutoClickerMenu()
    {
        _messenger.Send(new MinimizeWindowMessage());
        _autoClickerMenuView.Show();
        _autoClickerMenuView.Activate();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Open Menu" button on the UI based on if <see cref="AutoClickerMenuView"/> is visible or not.
    /// </summary>
    /// <returns>'True' if <see cref="AutoClickerMenuView"/> isn't visible, 'False' if it is.</returns>
    private bool CanOpenAutoClickerMenu() => _autoClickerMenuView != null && !_autoClickerMenuView.IsVisible;

    /// <summary>
    /// Enables or disables the "Next Image" button on the UI based on if it's not on the last image.
    /// </summary>
    /// <returns>'True' if the index is not on the last image, 'False' if it is.</returns>
    private bool CanShowNextImageTemplate() => CurrentTemplateImageIndex < CurrentSequence.Templates.Count-1;

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
    /// Enables or disables the "Delete Template Image" and "Change Template Image" buttons on the UI based on if there's a showing image.
    /// </summary>
    /// <returns>'True' if there's a showing image, 'False' if there isn't.</returns>
    private bool CanDeleteOrChangeTemplateImage() => CurrentShowingImage != null;

    #endregion

    #region ErrorValidations
    #endregion

    #region PublicMethods

    /// <summary>
    /// <see cref="IRecipient{TMessage}"/>' implementation to process different messages received from all parts of the application.
    /// </summary>
    /// <param name="message"><see cref="BringToFrontWindowMessage"/> message to bring the window into focus.</param>
    public void Receive(BringToFrontWindowMessage message)
    {
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Clears the images of the current showing sequence.
    /// </summary>
    private void ClearSequenceTemplateImages()
    {
        CurrentShowingImage = null;
    }

    /// <summary>
    /// Loads all the available <see cref="AutoClickerSequence"/>.
    /// </summary>
    private void LoadAutoClickerSequences()
    {
        try
        {
            IList<string> files = Directory.GetDirectories(_sequencePath);
            foreach (string file in files)
            {
                string jsonString = File.ReadAllText(Path.Combine(file, "Sequence.json"));
                AutoClickerSequence? autoClickerSequence = JsonSerializer.Deserialize<AutoClickerSequence>(jsonString);

                if (autoClickerSequence == null)
                    continue;

                AutoClickerSequences.Add(autoClickerSequence);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to load auto clicker sequence. {ex}", ex);
        }
    }

    /// <summary>
    /// Loads all the template images of the current sequence from the .json file.
    /// </summary>
    private void LoadAllSequencesTemplateImagesFromFile()
    {
        try
        {
            foreach (AutoClickerSequence sequence in AutoClickerSequences)
            {
                // Create a copy of the collection to iterate over
                IList<AutoClickerTemplate> templatesCopy = sequence.Templates.ToList();
                foreach (AutoClickerTemplate autoClickerTemplate in templatesCopy)
                {
                    if (!File.Exists(autoClickerTemplate.ImagePath) && !Path.GetExtension(autoClickerTemplate.ImagePath).Equals("png", StringComparison.OrdinalIgnoreCase))
                    {
                        sequence.Templates.Remove(autoClickerTemplate);
                        continue;
                    }

                    byte[] image = File.ReadAllBytes(autoClickerTemplate.ImagePath);
                    autoClickerTemplate.Image = image;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to load sequence template images. {ex}", ex);
        }
    }

    /// <summary>
    /// Loads all the template images of the current sequence.
    /// </summary>
    /// <param name="showImageIndex">Index of the image to show.</param>
    private void ShowTemplateImagesForCurrentSequence(int showImageIndex = 0)
    {
        try
        {
            ClearSequenceTemplateImages();
            IsSequenceDetailsVisible = true;
            if (!CurrentSequence.Templates.Any())
                return;

            NumberOfTemplateImages = CurrentSequence.Templates.Count;
            CurrentShowingImage = CurrentSequence?.Templates?.FirstOrDefault()?.Image;
            DelayBeforeClicking = CurrentSequence!.Templates[showImageIndex].DelayBeforeClicking;

            if (showImageIndex > 0 &&  showImageIndex < CurrentSequence.Templates.Count)
            {
                CurrentShowingImage = CurrentSequence.Templates[showImageIndex].Image;
                DelayBeforeClicking = CurrentSequence.Templates[showImageIndex].DelayBeforeClicking;
            }

            _lastShownSequence = CurrentSequence;
            IsDelayBeforeClickingTextBoxEnabled = CurrentSequence.Templates.Any();
            NotifyCanExecuteChanged(NextTemplateImageCommand);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to load sequence template images. {ex}", ex);
        }
    }

    /// <summary>
    /// Saves the current sequence to a file.
    /// </summary>
    private bool SaveSequenceAndImagesToFile()
    {
        string directoryPath = Path.Combine(_sequencePath, CurrentSequence.Name);

        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
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
            _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            _logger.LogError("Error trying to save sequence to file. {ex}", ex);
            return false;
        }
    }

    /// <summary>
    /// Deletes the current sequence from file.
    /// </summary>
    private async Task<bool> DeleteSequenceFromFile()
    {
        try
        {
            string directoryPath = Path.Combine(_sequencePath, CurrentSequence.Name);
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            return true;
        }
        catch (Exception ex)
        {
            ErrorDialog errorDialog = new($"Error trying to delete the sequence from file. Exception: '{ex.Message}'.");
            await _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            _logger.LogError("Error trying to save sequence to file. {ex}", ex);
            return false;
        }
    }

    /// <summary>
    /// Shows the passed <see cref="AutoClickerSequence"/>.
    /// </summary>
    /// <param name="sequence">The <see cref="AutoClickerSequence"/> to show.</param>
    private void ShowSequence(AutoClickerSequence? sequence)
    {
        CurrentSequence = sequence ?? AutoClickerSequences.FirstOrDefault()!;
        _lastShownSequence = CurrentSequence;
        OnSequenceSelectedItemChanged();
    }

    /// <summary>
    /// Handles the message received from the <see cref="ScreenShotSelection"/> view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="autoClickerTemplate"><see cref="AutoClickerTemplate"/> with the selection made by the user and where to click on the image.</param>
    private void HandleAutoClickerTemplateMessage(object sender, AutoClickerTemplate autoClickerTemplate)
    {
        if (autoClickerTemplate.Image == null)
        {
            ErrorDialog errorDialog = new($"Template image is null cannot save it to a file.");
            _dialogHost.Show(errorDialog, DIALOG_IDENTIFIER);
            return;
        }

        // We're changing an existing template image.
        if (_isChangingTemplate)
        {
            CurrentSequence.Templates[CurrentTemplateImageIndex] = autoClickerTemplate;
            ShowTemplateImagesForCurrentSequence(CurrentTemplateImageIndex);
            IsDelayBeforeClickingTextBoxEnabled = CurrentSequence.Templates.Any();
            _isUnsavedChanges = true;
            return;
        }

        // We're adding a new template image after the specified index. We add one to the index to show the correct number on the UI.
        CurrentSequence.Templates.Insert(CurrentTemplateImageIndex, autoClickerTemplate);
        ShowTemplateImagesForCurrentSequence(CurrentTemplateImageIndex);
        IsDelayBeforeClickingTextBoxEnabled = CurrentSequence.Templates.Any();
        _isUnsavedChanges = true;
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
}
