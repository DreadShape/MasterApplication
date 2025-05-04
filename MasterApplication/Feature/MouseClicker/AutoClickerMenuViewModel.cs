using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

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
using MasterApplication.UserControls.Dialog;
using MasterApplication.UserControls.ScreenShot;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

using Point = System.Drawing.Point;

namespace MasterApplication.Feature.MouseClicker;

public partial class AutoClickerMenuViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartAutoClickerSequenceCommand))]
    private AutoClickerStatus _autoClickerStatus = AutoClickerStatus.IDLE;

    [ObservableProperty]
    private string _autoClickerStatusForecolor = HexColors.Success;

    [ObservableProperty]
    private byte[]? _templateImageToSearch;

    [ObservableProperty]
    private int _autoClickerCurrentSequenceLoops = 0;

    [ObservableProperty]
    private string _autoClickerCurrentSequenceTime = "00:00";

    [ObservableProperty]
    private int _templateMatchRetries = 0;

    [ObservableProperty]
    private string _templateMatchScore = "0.0";

    [ObservableProperty]
    private string _templateMatchThreshold = "0.0";

    [ObservableProperty]
    private string _templateMatchScoreForeColor = HexColors.Default;

    [ObservableProperty]
    private bool _isDetailsVisible = false;

    [ObservableProperty]
    private int _mouseClickerLoopInterval;

    [ObservableProperty]
    private bool _isMouseClickerLoopIntervalEnabled;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private readonly IMouseService _mouseService;
    private readonly IKeyboardService _keyboardService;
    private const string DIALOG_IDENTIFIER = "AutoClickerDialog";
    private readonly string _errorTemplatesPath;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly DispatcherTimer _timer;
    private TimeSpan _time;
    private readonly AutoClickerSequence _autoClickerSequence;
    private readonly System.Windows.Size _screenSize = new System.Windows.Size((int)System.Windows.SystemParameters.PrimaryScreenWidth, (int)System.Windows.SystemParameters.PrimaryScreenHeight);

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
    /// <param name="autoClickerSequence"><see cref="AutoClickerSequence>"/> to know which sequence to loop through.</param>
    public AutoClickerMenuViewModel(ILogger<AutoClickerMenuViewModel> logger, IMessenger messenger, IDialogHost dialogHost, IMouseService mouseService, IKeyboardService keyboardService,
        ISnackbarMessageQueue snackbarMessageQueue, AutoClickerSequence autoClickerSequence, IScreenShotWindowFactory screenShotWindowFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        _mouseService = mouseService ?? throw new ArgumentNullException(nameof(mouseService));
        _keyboardService = keyboardService ?? throw new ArgumentNullException(nameof(keyboardService));
        _keyboardService.KeyPressed -= KeyboardServiceOnKeyPressed;
        _keyboardService.KeyPressed += KeyboardServiceOnKeyPressed;
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        _autoClickerSequence = autoClickerSequence;

        MouseClickerLoopInterval = 0;
        IsMouseClickerLoopIntervalEnabled = true;

        _time = TimeSpan.Zero;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _errorTemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"Feature\MouseClicker\Sequences\{autoClickerSequence.Name}\Errors");
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables the "StartAutoClicker" button on the UI based on if the <see cref="AutoClickerStatus"/> is IDLE or READY.
    /// </summary>
    /// <returns><see cref="true"/> if the status is IDLE or READY, <see cref="false"/> if it's any other status.</returns>
    private bool CanStartAutoClicker() => AutoClickerStatus == AutoClickerStatus.IDLE;

    #endregion

    #region Commands

    /// <summary>
    /// Starts the AutoClicker.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartAutoClicker))]
    private void OnStartAutoClickerSequence()
    {
        AutoClickerStatus = AutoClickerStatus.READY;
        AutoClickerStatusForecolor = HexColors.Success;

        if (_keyboardService.IsKeyboardHookAttached() && _mouseService.IsMouseHookAttached())
            return;

        _keyboardService.StartKeyboardHook();
        _mouseService.StartMouseHook();
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// Received the event of the window closing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnWindowClosed(object sender, EventArgs e)
    {
        StopAutoClicker();

        if (_mouseService.IsMouseHookAttached())
            _mouseService.StopMouseHook();

        if (_keyboardService.IsKeyboardHookAttached())
            _keyboardService.StopKeyboardHook();

        _keyboardService.KeyPressed -= KeyboardServiceOnKeyPressed;
        _messenger.Send(new WindowActionMessage(WindowAction.Normal));
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// DispatcherTimer tick method.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        _time = _time.Add(TimeSpan.FromSeconds(1));
        AutoClickerCurrentSequenceTime = _time.ToString(@"mm\:ss");
    }

    /// <summary>
    /// Intercepts the keyboard presses events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyboardServiceOnKeyPressed(object? sender, int vkCode)
    {
        if (vkCode == _autoClickerSequence.StartKeybind.KeyCode)
        {
            AutoClickerStatus = AutoClickerStatus.RUNNING;
            AutoClickerStatusForecolor = HexColors.Success;
            TemplateImageToSearch = null;
            TemplateMatchRetries = 0;
            TemplateMatchScore = "0.0";
            TemplateMatchScoreForeColor = HexColors.Default;
            IsDetailsVisible = true;
            IsMouseClickerLoopIntervalEnabled = false;

            //If the cancellation token is not null it means there's an AutoClicker active.
            if (_cancellationTokenSource != null)
                return;

            _cancellationTokenSource = new();
            //We start the mouse clicker loop at the current mouse position.
            if (MouseClickerLoopInterval > 0)
            {
                IsDetailsVisible = false;
                
                StartAutoClickerLoop(_cancellationTokenSource.Token);
                return;
            }

            StartAutoClicker(_cancellationTokenSource.Token);
            return;
        }

        //KeyCode 27 == Escape key
        if (vkCode == _autoClickerSequence.StopKeybind.KeyCode || vkCode == 27)
        {
            IsMouseClickerLoopIntervalEnabled = true;
            StopAutoClicker();
        }
    }

    /// <summary>
    /// Starts the AutoCliker.
    /// </summary>
    /// <param name="token">Token to cancel the loop.</param>
    public void StartAutoClicker(CancellationToken token)
    {
        _timer.Start();

        Task.Run(async () =>
        {
            try
            {
                MouseCoordinate mouseCoordinate;

                while (!token.IsCancellationRequested)
                {
                    foreach (AutoClickerTemplate step in _autoClickerSequence.Templates)
                    {
                        token.ThrowIfCancellationRequested();

                        TemplateImageToSearch = step.Image;
                        TemplateMatchThreshold = step.MatchThreshold.ToString("F2");

                        mouseCoordinate = GetScreenCoordinates(step.ImagePath, step.MatchThreshold, step.ClickCoordinates, token);
                        token.ThrowIfCancellationRequested();

                        if (mouseCoordinate.X == 0 || mouseCoordinate.Y == 0)
                        {
                            StopAutoClicker();
                            return;
                        }

                        _mouseService.MoveCursorTo(mouseCoordinate.X, mouseCoordinate.Y);
                        await Task.Delay(step.DelayBeforeClicking, token);
                        var monitorForChangeReferenceImage = CaptureAroundMouse();

                        _mouseService.ClickLeftMouseButton();
                        await Task.Delay(step.DelayAfterClicking, token);

                        token.ThrowIfCancellationRequested();

                        if (step.MonitorForChange)
                            await MonitorForChange(monitorForChangeReferenceImage, token);

                        if (step.ResetPosition)
                        {
                            _mouseService.MoveCursorTo(_autoClickerSequence.TemplateSearchBounds.X, _autoClickerSequence.TemplateSearchBounds.Y);
                            await Task.Delay(200, token);
                        }
                    }

                    AutoClickerCurrentSequenceLoops++;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Token was cancelled successfully. Ending sequence loop.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during sequence loop: {ex}", ex);
            }
        }, token);
    }

    /// <summary>
    /// Starts the AutoCliker loop on the current mouse position.
    /// </summary>
    /// <param name="token">Token to cancel the loop.</param>
    public void StartAutoClickerLoop(CancellationToken token)
    {
        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    _mouseService.ClickLeftMouseButton();
                    await Task.Delay(MouseClickerLoopInterval);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Token was cancelled successfully. Ending auto cliker loop.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during auto clicker loop: {ex}", ex);
            }
        }, token);
    }

    /// <summary>
    /// Stops the AutoCliker.
    /// </summary>
    public void StopAutoClicker()
    {
        _timer.Stop();

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;

        Application.Current.Dispatcher.Invoke(() =>
        {
            AutoClickerStatus = AutoClickerStatus.STOPPED;
            AutoClickerStatusForecolor = HexColors.Error;
        });
    }

    /// <summary>
    /// Captures the screen and performs template matching to find a template image.
    /// </summary>
    /// <param name="templateToSearch">Name of the template image file to search for.</param>
    /// <param name="templateThreshold">Threshold of the template to find.</param>
    /// <param name="clickOffset">Where to click based on each template.</param>
    /// <param name="token">Token to cancel the search.</param>
    /// <returns>Coordinates of the lower right corner of the matched area or 0,0 if there was an error finding the template.</returns>
    private MouseCoordinate GetScreenCoordinates(string templateToSearch, double templateThreshold, System.Windows.Point clickOffset, CancellationToken token)
    {
        for (int attempt = 0; attempt <= 10; attempt++)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                using Image<Gray, byte> sourceImage = Utils.CaptureScreen(_autoClickerSequence.TemplateSearchBounds.X, _autoClickerSequence.TemplateSearchBounds.Y, _autoClickerSequence.TemplateSearchBounds.Width, _autoClickerSequence.TemplateSearchBounds.Height);
                using Image<Gray, byte> templateImage = new Image<Gray, byte>(templateToSearch);
                //using Image<Gray, byte> sourceImage = new Image<Gray, byte>("C:\\Users\\grati\\OneDrive\\Desktop\\test.png");
                //using Image<Gray, byte> templateImage = new Image<Gray, byte>("E:\\Applications\\Desktop\\MasterApplication\\MasterApplication\\bin\\Debug\\net8.0-windows\\Feature\\MouseClicker\\Sequences\\Bidding\\Images\\1.jpg");

                using Image<Gray, float> resultImage = sourceImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed);
                resultImage.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);

                double maxValue = maxValues[0];
                Point maxLocation = maxLocations[0];
                Rectangle matchRect = new Rectangle(maxLocation, templateImage.Size);

                TemplateMatchScore = maxValue.ToString("F2");
                _logger?.LogDebug("Template score: {value}/{threshold}", maxValue.ToString("F2"), templateThreshold);
                if (maxValue >= templateThreshold)
                {
                    TemplateMatchScoreForeColor = HexColors.Success;

                    int clickX = (int)(matchRect.X + clickOffset.X);
                    int clickY = (int)(matchRect.Y + clickOffset.Y);

                    // Offset by the top-left of the search bounds
                    clickX += _autoClickerSequence.TemplateSearchBounds.X;
                    clickY += _autoClickerSequence.TemplateSearchBounds.Y;

                    return new MouseCoordinate(clickX, clickY);

                    /*CvInvoke.PutText(sourceImage, $"{maxValue:F2}", new Point(matchRect.X, matchRect.Y - 10), FontFace.HersheySimplex, 0.5, new MCvScalar(255), 1);
                    sourceImage.Draw(matchRect, new Gray(255), 2);  // white border, thickness 2
                    sourceImage.Save("C:\\Users\\grati\\OneDrive\\Desktop\\matched.png");*/

                    // === DEBUG: Save the matched screenshot with rectangle ===
                    /*
                    try
                    {
                        string debugFolder = Path.Combine(_errorTemplatesPath, "Matched");
                        if (!Directory.Exists(debugFolder))
                            Directory.CreateDirectory(debugFolder);

                        using var colorDebugImage = Utils.CaptureScreenColor(_tradingPostLocation.X, _tradingPostLocation.Y, _tradingPostSize.Width, _tradingPostSize.Height);
                        colorDebugImage.Draw(matchRect, new Bgr(Color.Red), 2);

                        string templateName = Path.GetFileName(templateToSearch).Split('.').First();
                        string debugFileName = $"{templateName}_match_{DateTime.Now:yyyyMMdd_HHmmss}_{maxValue:F2}.png";
                        colorDebugImage.Save(Path.Combine(debugFolder, debugFileName));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to save debug match image: {ex}", ex);
                    }
                    */
                    // === END DEBUG ===

                    // Draw rectangle on the grayscale image
                    //sourceImage.Draw(matchRect, new Gray(255), 2);  // white border, thickness 2
                    //sourceImage.Save(@$"E:\\Applications\\Desktop\\MasterApplication\\MasterApplication\\bin\\Debug\\net8.0-windows\\Feature\\MouseClicker\\Sequences\\Bidding\\Matched\{Path.GetFileName(templateToSearch).Split('.').First()}.jpg");
                }

                TemplateMatchScoreForeColor = HexColors.Error;
                _logger?.LogWarning("Attempt {attempt}: Match below threshold ({value}/{threshold})", attempt, maxValue.ToString("F2"), templateThreshold);
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Attempt {attempt}: Exception during image matching: {ex}", attempt, ex);
                break;
            }

            TemplateMatchRetries = attempt;
        }

        // === DEBUG: Save screenshot when match fails after all attempts ===
        /*
        string templateBaseName = Path.GetFileName(templateToSearch).Split('.').First();
        string fileName = $"{templateBaseName}_not_found_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";

        try
        {
            using var finalScreenshot = Utils.CaptureScreen(_tradingPostLocation.X, _tradingPostLocation.Y, _tradingPostSize.Width, _tradingPostSize.Height);

            if (!Directory.Exists(_errorTemplatesPath))
                Directory.CreateDirectory(_errorTemplatesPath);

            finalScreenshot.Save(Path.Combine(_errorTemplatesPath, fileName));
        }
        catch (Exception logEx)
        {
            _logger?.LogError("Failed to save failed match screenshot: {ex}", logEx);
        }
        */
        // === END DEBUG ===

        return new MouseCoordinate(0, 0);
    }

    /// <summary>
    /// Monitors around the mouse for a change.
    /// </summary>
    /// <param name="referenceImage">Reference image to see if it changed.</param>
    /// <param name="token">Cancellation token to stop monitoring.</param>
    /// <returns></returns>
    public async Task MonitorForChange(Image<Gray, byte> referenceImage, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                using var currentScreenshot = CaptureAroundMouse();

                bool hasChanged = Utils.DetectChange(referenceImage, currentScreenshot);
                if (hasChanged)
                    return;

                await Task.Delay(300, token);
            }
        }
        finally
        {
            referenceImage.Dispose();
        }
    }

    /// <summary>
    /// Captures a screenshot around the current position of the cursor.
    /// </summary>
    /// <param name="radius">Size of the screenshot.</param>
    /// <returns></returns>
    private Image<Gray, byte> CaptureAroundMouse(int radius = 50)
    {
        var mousePos = _mouseService.GetMousePos();
        int x = mousePos.X - radius;
        int y = mousePos.Y - radius;
        int size = radius * 2;

        return Utils.CaptureScreen(x, y, size, size);
    }

    #endregion
}
