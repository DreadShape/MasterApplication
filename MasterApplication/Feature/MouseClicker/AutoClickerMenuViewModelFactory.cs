using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.MouseClicker;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public class AutoClickerMenuViewModelFactory : IAutoClickerMenuViewModelFactory
{
    private readonly ILogger<AutoClickerMenuViewModel> _logger;
    private readonly IMessenger _messenger;
    private readonly IDialogHost _dialogHost;
    private readonly IMouseService _mouseService;
    private readonly IKeyboardService _keyboardService;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;

    public AutoClickerMenuViewModelFactory(ILogger<AutoClickerMenuViewModel> logger, IMessenger messenger, IDialogHost dialogHost, IMouseService mouseService, IKeyboardService keyboardService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger;
        _messenger = messenger;
        _dialogHost = dialogHost;
        _mouseService = mouseService;
        _keyboardService = keyboardService;
        _snackbarMessageQueue = snackbarMessageQueue;
    }

    /// <summary>
    /// Creates an <see cref="AutoClickerMenuViewModel"/>.
    /// </summary>
    /// <param name="sequence"><see cref="AutoClickerSequence"/> to know what sequences to execute.</param>
    /// <returns>The <see cref="AutoClickerMenuViewModel"/> created.</returns>
    public AutoClickerMenuViewModel Create(AutoClickerSequence sequence)
    {
        return new(_logger, _messenger, _dialogHost, _mouseService, _keyboardService, _snackbarMessageQueue, sequence);
    }
}
