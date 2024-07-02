using System.Drawing;
using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using MasterApplication.Helpers;
using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;
using MasterApplication.Models.Structs;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.MouseClicker;
using MasterApplication.UserControls;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

using Point = System.Drawing.Point;

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
    private readonly string _templateFolderPath = @"E:\Applications\Desktop\Images";
    private readonly string _outputFolderPath = @"E:\Applications\Desktop\Images\Output";
    private const double TEMPLATE_THRESHOLD = 0.65;

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

        //MouseCoordinate test = GetScreenCoordinates("SellingAndBuyingTemplate.png");
        //_mouseService.MoveCursorTo(test.X, test.Y);
        //_mouseService.ClickLeftMouseButton();
        _keyboardService.StartKeyboardHook();
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
                /*AutoClickerSequence sequence = new AutoClickerSequence()
                {
                    Sleep = Convert.ToInt32(elapsedTime),
                    X = e.Coordinate.X,
                    Y = e.Coordinate.Y
                };
                _newSequence.Add(sequence);*/
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
            //If the cancellation token is not null it means there's an AutoClicker active.
            if (_cancellationTokenSource != null)
                return;

            AutoClickerStatus = AutoClickerStatus.RUNNING;
            AutoClickerStatusForecolor = HexColors.Error;
            _cancellationTokenSource = new();
            StartAutoClicker(_cancellationTokenSource.Token, _currentSequence.Steps, _currentSequence.Loops);
        }

        if (vkCode == _currentSequence.StopKeybind.KeyCode)
            StopAutoClicker();
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
    /// <param name="token">Token to cancel the loop.</param>
    /// <param name="sequence">Sequence of steps to loop.</param>
    /// <param name="loops">Number of times to do the sequence.</param>
    public void StartAutoClicker(CancellationToken token, IList<AutoClickerSequence> steps, int loops = 0)
    {
        Task.Run(async () =>
        {
            int currentLoop = loops;
            MouseCoordinate mouseCoordinate;

            while (!token.IsCancellationRequested && (currentLoop > 0 || loops == 0))
            {
                if (token.IsCancellationRequested)
                    return;

                foreach (AutoClickerSequence step in steps)
                {
                    await Task.Delay(step.Sleep, token);
                    if (step.MonitorForChange)
                        await MonitorAroundMouseForColorChange(token);

                    mouseCoordinate = GetScreenCoordinates(step.Template);
                    _mouseService.MoveCursorTo(mouseCoordinate.X, mouseCoordinate.Y);
                    _mouseService.ClickLeftMouseButton();

                    if (token.IsCancellationRequested)
                        return;
                }

                if (loops > 0)
                    currentLoop--;
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

        AutoClickerStatus = AutoClickerStatus.READY;
        AutoClickerStatusForecolor = HexColors.Success;
    }





    

    /// <summary>
    /// Captures the screen and performs template matching to find a template image.
    /// </summary>
    /// <param name="templateToSearch">Name of the template image file to search for.</param>
    /// <returns>Coordinates of the lower right corner of the matched area or 0,0 if there was an error finding the template.</returns>
    private MouseCoordinate GetScreenCoordinates(string templateToSearch)
    {
        try
        {
            Image<Gray, byte> sourceImage = Utils.CaptureScreen();
            Image<Gray, byte> templateImage = new Image<Gray, byte>(templateToSearch);

            // Perform template matching
            using (Image<Gray, float> resultImage = sourceImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                resultImage.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                double maxValue = maxValues[0];
                Point maxLocation = maxLocations[0];

                if (maxValue >= TEMPLATE_THRESHOLD)
                {
                    // Define a rectangle around the matched area
                    Rectangle matchRect = new Rectangle(maxLocation, templateImage.Size);

                    // Draw a rectangle around the matched area on the source image
                    sourceImage.Draw(matchRect, new Gray(255), 2);

                    // Save the source image with the drawn rectangle (for debugging purposes)
                    //string fileName = $"{maxValue.ToString("F2")}_{Path.GetFileName(templateToSearch)}";
                    //sourceImage.Save(Path.Combine(_outputFolderPath, fileName));

                    // Calculate the coordinates of the lower right corner of the matched area -8 as a buffer to not be directly in the corner
                    int matchLowerRightX = matchRect.X + matchRect.Width - 8;
                    int matchLowerRightY = matchRect.Y + matchRect.Height - 8;

                    // Return the coordinates as a MouseCoordinate struct
                    return new MouseCoordinate(matchLowerRightX, matchLowerRightY);
                }

                _logger?.LogWarning("Template searching result below threshold, could not find the right coordinates. Result: '{value}/{threshold}'", maxValue.ToString("F2"), TEMPLATE_THRESHOLD);
                string fileName = $"{maxValue.ToString("F2")}_template_not_found_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
                sourceImage.Save(Path.Combine(_outputFolderPath, fileName));
                return new MouseCoordinate(0,0);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError("Could not analyze images. Exception: {ex}", ex);
            return new MouseCoordinate(0, 0);
        }
    }

    /// <summary>
    /// Monitors the screen around the current mouse position for changes in color intensity.
    /// </summary>
    /// <param name="token">Cancellation token to stop monitoring.</param>
    /// <returns></returns>
    public async Task MonitorAroundMouseForColorChange(CancellationToken token)
    {
        Image<Gray, byte>? previousScreenshot = null;

        while (!token.IsCancellationRequested)
        {
            // Get the current mouse position
            var mousePos = _mouseService.GetMousePos();
            int captureRadius = 50;
            int captureX = mousePos.X - captureRadius;
            int captureY = mousePos.Y - captureRadius;
            int captureWidth = captureRadius * 2;
            int captureHeight = captureRadius * 2;

            // Capture the screen around the mouse position in grayscale
            Image<Gray, byte> screenshot = Utils.CaptureScreen(captureX, captureY, captureWidth, captureHeight);
            //Save image for debugging purposes
            //string fileName = $"mouse_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
            //screenshot.Save(Path.Combine(_outputFolderPath, fileName));

            if (previousScreenshot != null)
            {
                // Check for changes in intensity
                bool hasChanged = Utils.DetectChange(previousScreenshot, screenshot);

                if (hasChanged)
                    return;
            }

            previousScreenshot = screenshot;

            // Wait for 500ms before taking the next screenshot
            await Task.Delay(500, token);
        }
    }

    #endregion
}
