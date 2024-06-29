using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models.Messages;
using MasterApplication.Models.Structs;
using MasterApplication.Services.Dialog;

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
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private AutoClickerMenuView? _autoClickerMenuView;
    private AutoClickerMenuViewModel? _autoClickerMenuViewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="messengerService"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> to get access to the dependency injector container.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IMessenger messenger, IDialogService dialogService, 
        IServiceProvider serviceProvider, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
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
    #endregion

    #region CustomNotifyCanExecutedChanged

    /// <summary>
    /// Custom implementation of <see cref="NotifyCanExecuteChangedObservableCollection{T}"/> event to be able to raise <see cref="INotifyPropertyChangedAttribute"/> passing it whatever <see cref="RelayCommand"/> you need to raised the event.
    /// </summary>
    /// <param name="command"><see cref="RelayCommand"/> to have <see cref="NotifyCanExecuteChangedForAttribute"/> raised.</param>
    private static void NotifyCanExecuteChanged(IRelayCommand command) => command.NotifyCanExecuteChanged();

    #endregion
}
