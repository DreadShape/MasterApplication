using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Models.Structs;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.MouseClicker;
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
    private readonly IMouseService _mouseService;
    private readonly IKeyboardService _keyboardService;
    private const string DIALOG_IDENTIFIER = "AutoClickerDialog";
    private readonly string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    private AutoClicker _autoClicker;
    private readonly IList<AutoClickerSequence> _newSequence;
    private DateTime _dateTime;
    private Sequence _currentSequence;
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="AutoClickerMenuViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogHost"><see cref="IDialogHost"/> implementation to be able to show the material design dialog host.</param>
    /// <param name="mouseService"><see cref="IMouseService"/> to simulate mouse clicks on the screen.</param>
    /// <param name="keyboardService"><see cref="IKeyboardService"/> to intercept keyboard presses.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public AutoClickerMenuViewModel(ILogger<AutoClickerMenuViewModel> logger, IMessenger messenger, IDialogHost dialogHost, IMouseService mouseService, IKeyboardService keyboardService,
        ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        _mouseService = mouseService ?? throw new ArgumentNullException(nameof(mouseService));
        _mouseService.MouseButtonClicked -= MouseServiceOnMouseButtonCliked;
        _mouseService.MouseButtonClicked += MouseServiceOnMouseButtonCliked;
        _keyboardService = keyboardService ?? throw new ArgumentNullException(nameof(keyboardService));
        _keyboardService.KeyPressed -= KeyboardServiceOnKeyPressed;
        _keyboardService.KeyPressed += KeyboardServiceOnKeyPressed;
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));

        _autoClicker = new();
        _currentSequence = new();
        _newSequence = new List<AutoClickerSequence>();
        LoadAutoClickerConfigurationFile();
        SetData();
    }

    #endregion

    #region CommandValidations
    #endregion

    #region Commands

    /// <summary>
    /// Starts the AutoClicker.
    /// </summary>
    [RelayCommand]
    private void OnStartAutoClicker()
    {
        AutoClickerStatus = AutoClickerStatus.READY;
        AutoClickerStatusForecolor = HexColors.Success;



















        //_keyboardService.StartKeyboardHook();
    }

    /// <summary>
    /// Handle when the user changes the sequence from the combo box.
    /// </summary>
    [RelayCommand]
    private void OnSequenceSelectedItemChanged()
    {
        SetData();
    }

    /// <summary>
    /// Starts a recording of the position of each mouse click.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnCreateSequence()
    {
        ConfirmDialog confirmDialog = new("Creating a new sequence will delete the existing one, continue?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isCreatingSequenceCanceled && isCreatingSequenceCanceled)
            return;

        AutoClickerStatus = AutoClickerStatus.RECORDING;
        AutoClickerStatusForecolor = HexColors.Primary;
        AutoClickerCurrentSequenceSteps = 0;
        AutoClickerInstructions = "Left Click: Record the current position of the cursor.\nWheel Button: Delete previous position.\nRight Click: End the recording.";
        _mouseService.StartMouseHook();
        _dateTime = DateTime.UtcNow;
        _newSequence.Clear();
    }

    /// <summary>
    /// Sets the start/stop AutoClicker keybinds.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnSetKeybind(string commanButton)
    {
        KeybindDialog keybindDialog = new(_keyboardService);
        if (await _dialogHost.Show(keybindDialog, DIALOG_IDENTIFIER) is bool isSettingStartKeybindCanceled && isSettingStartKeybindCanceled)
            return;

        string keyName = keybindDialog.KeybindKey.KeyName;
        int keyCode = keybindDialog.KeybindKey.KeyCode;
        Keybind newKeybind = new(keyName, keyCode);

        if (commanButton.Equals("Start", StringComparison.OrdinalIgnoreCase))
        {
            StartKeybind = keyName;
            _currentSequence.StartKeybind = newKeybind;
            SaveAutoClickerToFile();
            return;
        }

        StopKeybind = keyName;
        _currentSequence.StopKeybind = newKeybind;
        SaveAutoClickerToFile();
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
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        _mouseService.StopMouseHook();
        _keyboardService.StopKeyboardHook();
        _messenger.Send(new BringToFrontWindowMessage());
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Intercepts the mouse clicking events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MouseServiceOnMouseButtonCliked(object? sender, MouseButtonEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButton.Left:
                AutoClickerCurrentSequenceSteps++;
                DateTime currentDateTime = DateTime.UtcNow;
                double elapsedTime = (currentDateTime - _dateTime).TotalMilliseconds;
                _dateTime = currentDateTime;
                AutoClickerSequence sequence = new AutoClickerSequence()
                {
                    Sleep = Convert.ToInt32(elapsedTime),
                    X = e.Coordinate.X,
                    Y = e.Coordinate.Y
                };
                _newSequence.Add(sequence);
                return;

            case MouseButton.Right:
                _ = FinishCreatingSequence();
                return;

            case MouseButton.Middle:
                if (_newSequence.Any())
                {
                    AutoClickerCurrentSequenceSteps--;
                    _dateTime = DateTime.UtcNow;
                    _newSequence.Remove(_newSequence.Last());
                }
                return;

            default:
                return;
        }
    }

    /// <summary>
    /// Intercepts the keyboard presses events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyboardServiceOnKeyPressed(object? sender, int vkCode)
    {
        //Escape key
        if (vkCode == 27)
        {
            StopAutoClicker();
            _mouseService.StopMouseHook();
        }

        if (vkCode == _currentSequence.StartKeybind.KeyCode)
        {
            //It means there's an AutoClicker active if the cancellation token is not null.
            if (_cancellationTokenSource != null)
                return;

            AutoClickerStatus = AutoClickerStatus.RUNNING;
            AutoClickerStatusForecolor = HexColors.Error;
            _cancellationTokenSource = new();
            StartAutoClicker(_currentSequence.Steps, _cancellationTokenSource.Token);
        }

        if (vkCode == _currentSequence.StopKeybind.KeyCode)
        {
            AutoClickerStatus = AutoClickerStatus.READY;
            AutoClickerStatusForecolor = HexColors.Success;
            StopAutoClicker();
        }
    }

    /// <summary>
    /// Sets the current <see cref="AutoClicker"/> options to the UI.
    /// </summary>
    private void SetData()
    {
        switch (AutoClickerType)
        {
            case AutoClickerType.Buying:
                _currentSequence = _autoClicker.Buying;
                break;

            case AutoClickerType.Selling:
                _currentSequence = _autoClicker.Selling;
                break;

            case AutoClickerType.Bidding:
                _currentSequence = _autoClicker.Bidding;
                break;

            case AutoClickerType.AutoClicker:
                _currentSequence = _autoClicker.AutoClickerLoop;
                break;

            default:
                _currentSequence = _autoClicker.Buying;
                break;
        }

        AutoClickerCurrentSequenceSteps = _currentSequence.Steps.Count;
        StartKeybind = _currentSequence.StartKeybind.KeyName;
        StopKeybind = _currentSequence.StopKeybind.KeyName;
    }

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
                _currentSequence = _autoClicker.Buying;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Could not load auto clicker configuration file. Exception: {ex}", ex);
        }
    }

    /// <summary>
    /// Updates the current settings of the AutoClicker and saves them to a configuration file.
    /// </summary>
    private async Task FinishCreatingSequence()
    {
        _mouseService.StopMouseHook();

        ConfirmDialog confirmDialog = new("Save current sequence?");
        if (await _dialogHost.Show(confirmDialog, DIALOG_IDENTIFIER) is bool isSavingSequenceCanceled && isSavingSequenceCanceled)
            return;

        AutoClickerStatus = AutoClickerStatus.IDLE;
        AutoClickerStatusForecolor = HexColors.Success;
        AutoClickerInstructions = string.Empty;

        _currentSequence.Steps = _newSequence;

        if (AutoClickerType == AutoClickerType.Buying)
        {
            _autoClicker.Buying = _currentSequence;
        }

        if (AutoClickerType == AutoClickerType.Selling)
        {
            _autoClicker.Buying = _currentSequence;
        }

        if (AutoClickerType == AutoClickerType.Bidding)
        {
            _autoClicker.Buying = _currentSequence;
        }

        SaveAutoClickerToFile();
    }

    /// <summary>-
    /// Saves the current AutoClicker to a configuration file
    private void SaveAutoClickerToFile()
    {
        try
        {
            string autoClickerConfigurationFilePath = Path.Combine(executingDirectory, "AutoClicker.json");
            if (File.Exists(autoClickerConfigurationFilePath))
            {
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                };

                string jsonString = JsonSerializer.Serialize(_autoClicker, jsonSerializerOptions);
                File.WriteAllText(autoClickerConfigurationFilePath, jsonString);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Could not load auto clicker configuration file. Exception: {ex}", ex);
        }
    }
 
    /// <summary>
    /// Starts the AutoCliker.
    /// </summary>
    /// <param name="sequence">Sequence of steps to loop.</param>
    public void StartAutoClicker(IList<AutoClickerSequence> sequence, CancellationToken token)
    {
        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                foreach (AutoClickerSequence step in sequence)
                {
                    _mouseService.MoveCursorTo(step.X, step.Y);
                    Thread.Sleep(step.Sleep);
                    _mouseService.ClickLeftMouseButton();

                    if (token.IsCancellationRequested)
                        return;
                }
            }
        }, token);
    }

    /// <summary>
    /// Stops the AutoCliker.
    /// </summary>
    public void StopAutoClicker()
    {
        //StopAutoClicker
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
    }

    #endregion
}
