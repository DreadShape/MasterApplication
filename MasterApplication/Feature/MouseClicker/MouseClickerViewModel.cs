using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Services.Dialog;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.MouseClicker;

public partial class MouseClickerViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    public NotifyCanExecuteChangedObservableCollection<int[][]> Sequence;

    [ObservableProperty]
    private string _startClickerKeybinding;

    [ObservableProperty]
    private string _stopClickerKeybinding;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IDialogService _dialogService;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of a <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to los information, warnings and errors.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public MouseClickerViewModel(ILogger<MouseClickerViewModel> logger, IDialogService dialogService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Action canClearSequenceAction = () => NotifyCanExecuteChanged(ClearSequenceCommand);
        Action canStartSequenceAction = () => NotifyCanExecuteChanged(StartAutoClickerCommand);
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        Sequence = new NotifyCanExecuteChangedObservableCollection<int[][]>(new List<Action> { canClearSequenceAction, canStartSequenceAction });
    }

    #endregion

    #region Commands

    /// <summary>
    /// Starts the mouse clicker when pressing the specified key binding.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearOrStartSequence))]
    private void OnStartAutoClicker()
    {

    }

    /// <summary>
    /// Creates the sequence of where the mouse clicks will happen.
    /// </summary>
    [RelayCommand]
    private void OnCreateSequence()
    {

    }

    /// <summary>
    /// Clears the <see cref="Sequence"/> collection.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearOrStartSequence))]
    private void OnClearSequence()
    {
        Sequence?.Clear();
    }

    /// <summary>
    /// Sets a start keybinding to start the auto clicker.
    /// </summary>
    [RelayCommand]
    private void OnSetStartAutoClickerKeybinding()
    {

    }

    /// <summary>
    /// Sets a stop keybinding to finish the auto clicker.
    /// </summary>
    [RelayCommand]
    private void OnSetStopAutoClickerKeybinding()
    {

    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Ready to Click" and "Clear Sequence" buttons on the UI based on if <see cref="Sequence"/> collection is empty or not.
    /// </summary>
    /// <returns>'True' if <see cref="Sequence"/> has any items inside, 'False' if it doesn't.</returns>
    private bool CanClearOrStartSequence() => Sequence.Any();

    #endregion

    #region ErrorValidations



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
