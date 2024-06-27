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

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class MouseClickerViewModel : ObservableObject, IRecipient<BringToFrontWindowMessage>
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    public IList<AutoClickerSequence> _autoClickerSequence = new List<AutoClickerSequence>();

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IMouseService _mouseService;
    private readonly IKeyboardService _keyboardService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private AutoClickerMenuView? _autoClickerMenuView;
    private AutoClickerMenuViewModel? _autoClickerMenuViewModel;
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="mouseService"><see cref="IMouseService"/> to simulate mouse clicks on the screen.</param>
    /// <param name="keyboardService"><see cref="IKeyboardService"/> to intercept keyboard presses.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> to get access to the dependency injector container.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IMessenger messenger, IMouseService mouseService, IKeyboardService keyboardService, 
        IDialogService dialogService, IServiceProvider serviceProvider, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _mouseService = mouseService ?? throw new ArgumentNullException(nameof(mouseService));
        _mouseService.MouseClicked -= MouseServiceOnMouseClicked;
        _mouseService.MouseClicked += MouseServiceOnMouseClicked;

        _keyboardService = keyboardService ?? throw new ArgumentNullException(nameof(keyboardService));
        _keyboardService.KeyPressed -= KeyboardServiceOnKeyPressed;
        _keyboardService.KeyPressed += KeyboardServiceOnKeyPressed;

        _messenger.Register(this);
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _autoClickerMenuView = new();

        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
    }

    #endregion

    #region Commands

    /// <summary>
    /// Opens the auto clicker menu with all it's options.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenAutoClickerMenu))]
    private void OnOpenAutoClickerMenu()
    {
        _autoClickerMenuView = new ();
        _autoClickerMenuViewModel = _serviceProvider.GetRequiredService<AutoClickerMenuViewModel>();
        _autoClickerMenuView.DataContext = _autoClickerMenuViewModel;
        _messenger.Send(new MinimizeWindowMessage());
        _autoClickerMenuView.Show();
        _autoClickerMenuView.Activate();
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    /// <summary>
    /// Creates the sequence of where the mouse clicks will happen.
    /// </summary>
    [RelayCommand]
    private void OnCreateSequence()
    {
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string jsonFilePath = Path.Combine(exeDirectory, "AutoClicker.json");

        if (File.Exists(jsonFilePath))
        {
            

        }

        _messenger.Send(new MinimizeWindowMessage());
        _keyboardService.StartKeyboardHook();
        //_mouseService.StartMouseHook();

        // Minimize the application
        /*_messenger.Send(new MinimizeWindowMessage());
        _keyboardService.StartKeyboardHook();
        _mouseService.StartMouseHook();*/

        /*Thread.Sleep(2000);
        Task.Run(() =>
        {
            MouseCoordinate mouseCoordinate = new MouseCoordinate(1700, 598);
            for (int i = 0; i < 8; i++)
            {
                _mouseService.ClickLeftMouseButton(mouseCoordinate.X, mouseCoordinate.Y);
                Thread.Sleep(100);
            }
            *//*while (_interceptingCursor)
            {
                MouseCoordinate position = _mouseService.GetMousePos();
                Test = $"X:{position.X} - Y:{position.Y}";
                Thread.Sleep(500);
            }*//*
        });*/
        // Minimize the application
        //_messenger.Send(new MinimizeWindowMessage());
        //Thread.Sleep(2000);
        /*try
        {
            MouseSimulator.ClickLeftMouseButton(1291, 745);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            throw;
        }*/
        /*_interceptingCursor = true;
        _interceptingKeyboard = true;
        _proc = HookCallback;
        _hookID = SetHook(_proc);

        // Get the current cursor position
        while (_interceptingCursor)
        {
            try
            {
                if (GetCursorPos(out POINT cursorPosition))
                {
                    Sequence.Add(cursorPosition);
                    MessageBox.Show($"Cursor Position: X = {cursorPosition.X}, Y = {cursorPosition.Y}");
                }
            }
            catch (Exception ex)
            {
                var test = ex;
            }
        }*/
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Open Menu" button on the UI based on if <see cref="AutoClickerMenuView"/> is visible or not.
    /// </summary>
    /// <returns>'True' if <see cref="AutoClickerMenuView"/> isn't visible, 'False' if it is.</returns>
    private bool CanOpenAutoClickerMenu() => _autoClickerMenuView != null && !_autoClickerMenuView.IsVisible;
    #endregion

    #region ErrorValidations
    #endregion

    #region PublicMethods

    /// <summary>
    /// <see cref="IRecipient{TMessage}"/>' implementation to process different messages received from all parts of the application.
    /// </summary>
    /// <param name="message">The message received.</param>
    public void Receive(BringToFrontWindowMessage message)
    {
        NotifyCanExecuteChanged(OpenAutoClickerMenuCommand);
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Mouse clicked intercepted from the mouse.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="mouseCoordinate"></param>
    private void MouseServiceOnMouseClicked(object? sender, MouseCoordinate mouseCoordinate)
    {
        //Todo: If we right click we stop intercepting the keys
    }

    /// <summary>
    /// Key pressed intercepted from the keyboard.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="vkCode">Code of the key pressed.</param>
    private void KeyboardServiceOnKeyPressed(object? sender, int vkCode)
    {
        switch (vkCode)
        {
            //Comma
            case 188:
                //ToggleBiddingClicker();
                break;

            //Hyphen/Underline
            case 189:
                ToggleAutoClicker(220);
                break;

            //Period
            case 190:
                //ToggleBiddingClicker();
                //ToggleSellingClicker();
                break;

            //Escape
            case 27:
                _mouseService.StopMouseHook();
                //_keyboardService.StopKeyboardHook();
                break;
        }
    }

    /// <summary>
    /// Toggles the selling autocliker.
    /// </summary>
    /// <param name="speed">Speed of each clicks.</param>
    public void ToggleAutoClicker(int speed)
    {
        //Start
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            //Start auto-clicker
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    MouseCoordinate mousePosition = _mouseService.GetMousePos();
                    _mouseService.MoveCursorTo(mousePosition.X, mousePosition.Y);
                    _mouseService.ClickLeftMouseButton();
                    Thread.Sleep(speed);
                }
            }, token);

            return;
        }

        //Stop auto-clicker
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
    }

    /// <summary>
    /// Toggles the selling autocliker.
    /// </summary>
    public void ToggleSellingClicker()
    {
        //StartAutoClicker
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (AutoClickerSequence sequence in _autoClickerSequence)
                    {
                        _mouseService.MoveCursorTo(sequence.X, sequence.Y);
                        Thread.Sleep(sequence.Sleep);
                        _mouseService.ClickLeftMouseButton();

                        if (token.IsCancellationRequested)
                            return;
                    }
                }
            }, token);

            return;
        }

        //StopAutoClicker
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
    }

    /// <summary>
    /// Toggles the bidding autocliker.
    /// </summary>
    public void ToggleAutoClicker(AutoClickerType type)
    {
        switch (type)
        {
            case AutoClickerType.Buying:
                break;

            case AutoClickerType.Selling:
                break;

            case AutoClickerType.Bidding:
                break;

            default:
                break;
        }
        //StartAutoClicker
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (AutoClickerSequence sequence in _autoClickerSequence)
                    {
                        _mouseService.MoveCursorTo(sequence.X, sequence.Y);
                        Thread.Sleep(sequence.Sleep);
                        _mouseService.ClickLeftMouseButton();

                        if (token.IsCancellationRequested)
                            return;
                    }
                }
            }, token);

            return;
        }

        //StopAutoClicker
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
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
