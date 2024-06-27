using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Services.Dialog;
using MasterApplication.UserControls;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class AutoClickerMenuViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    [ObservableProperty]
    private AutoClickerStatus _autoClickerStatus = AutoClickerStatus.IDLE;

    [ObservableProperty]
    private AutoClickerType _autoClickerType;

    [ObservableProperty]
    private string _autoClickerStatusForecolor = HexColors.Success;

    [ObservableProperty]
    private int _autoClickerCurrentSequenceSteps = 0;

    [ObservableProperty]
    private string _startKeybind = string.Empty;

    [ObservableProperty]
    private string _stopKeybind= string.Empty;

    [ObservableProperty]
    private string _autoClickerInstructions = string.Empty;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private const string DIALOG_IDENTIFIER = "AutoClickerDialog";
    private readonly string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    private AutoClicker _autoClicker;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="AutoClickerMenuViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogHost"><see cref="IDialogHost"/> implementation to be able to show the material design dialog host.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public AutoClickerMenuViewModel(ILogger<AutoClickerMenuViewModel> logger, IMessenger messenger, IDialogHost dialogHost, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));

        _autoClicker = new();
        LoadAutoClickerConfigurationFile();
        AutoClickerCurrentSequenceSteps = _autoClicker.Buying.Steps.Count;
        StartKeybind = _autoClicker.Buying.StartKeyBind;
        StopKeybind = _autoClicker.Buying.StopKeyBind;
    }

    #endregion

    #region CommandValidations
    #endregion

    #region Commands

    /// <summary>
    /// Handle when the user changes the sequence from the combo box.
    /// </summary>
    [RelayCommand]
    private void OnSequenceSelectedItemChanged()
    {
        switch (AutoClickerType)
        {
            case AutoClickerType.Buying:
                AutoClickerCurrentSequenceSteps = _autoClicker.Buying.Steps.Count;
                AutoClickerStatusForecolor = HexColors.Success;
                break;

            case AutoClickerType.Selling:
                AutoClickerCurrentSequenceSteps = _autoClicker.Selling.Steps.Count;
                AutoClickerStatusForecolor = HexColors.Error;
                break;

            case AutoClickerType.Bidding:
                AutoClickerCurrentSequenceSteps = _autoClicker.Bidding.Steps.Count;
                AutoClickerStatusForecolor = HexColors.Primary;
                break;

            default:
                AutoClickerCurrentSequenceSteps = 0;
                break;
        }
    }

    [RelayCommand]
    private async Task OnCreateSequence()
    {
        ConfirmDialog confirmDialog = new("Creating a new sequence will delete the existing one, continue?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isCreatingSequenceCanceled && isCreatingSequenceCanceled)
            return;

        AutoClickerStatus = AutoClickerStatus.RECORDING;
        AutoClickerStatusForecolor = HexColors.Primary;

        AutoClickerInstructions = "Left Click: Record the current position of the cursor.\nWheel Button: Delete previous position.\nRight Click: End the recording.";
    }

    #endregion

    #region CommandValidations
    #endregion

    #region PublicMethods

    /// <summary>
    /// Received the event of the window closing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnWindowClosed(object sender, EventArgs e)
    {
        _messenger.Send(new BringToFrontWindowMessage());
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Loads the different sequences we have stored in a cofiguration file.
    /// </summary>
    private void LoadAutoClickerConfigurationFile()
    {
        try
        {
            string autoClickerConfigurationFilePath = Path.Combine(executingDirectory, "AutoClicker.json");
            if (File.Exists(autoClickerConfigurationFilePath))
            {
                string jsonString = File.ReadAllText(autoClickerConfigurationFilePath);
                _autoClicker = JsonSerializer.Deserialize<AutoClicker>(jsonString) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Could not load auto clicker configuration file. Exception: {ex}", ex);
        }
    }

    #endregion
}
