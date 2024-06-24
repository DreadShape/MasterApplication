using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Models.Enums;
using MasterApplication.Models.Messages;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Features.Productivity;

public partial class ProductivityViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartProductivityChunkCommand))]
    private ProductivityType _productivityType;

    [ObservableProperty]
    private string _startProductivityChunkButtonText = "Start Productivity Chunk";

    [ObservableProperty]
    private string _productivityProgressbarText = string.Empty;

    [ObservableProperty]
    private int _productivityChunkProgressbarValue;

    [ObservableProperty]
    private int _productivityChunkTime = 2;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;

    private readonly IMessenger _messenger;

    private bool _isProductivityChunkActive = false;

    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="ProductivityView"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    /// <param name="messenger"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    public ProductivityViewModel(ILogger<ProductivityViewModel> logger, ISnackbarMessageQueue snackbarMessageQueue, IMessenger messenger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));

        ProductivityChunkProgressbarValue = _productivityChunkTime;
    }

    #endregion

    #region CommandValidations

    #endregion

    #region Commands

    /// <summary>
    /// Starts a productivity chunk.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartCountdown))]
    private void OnStartProductivityChunk()
    {
        if (_isProductivityChunkActive)
        {
            _cancellationTokenSource?.Cancel();
            StartProductivityChunkButtonText = "Start Productivity Chunk";

            _isProductivityChunkActive = !_isProductivityChunkActive;
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        StartProductivityChunkButtonText = "Cancel";
        Task.Run(() => StartCountdown(_cancellationTokenSource.Token));

        _isProductivityChunkActive = !_isProductivityChunkActive;
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "Start Productivity" button on the UI based on if a productivity type was selected.
    /// </summary>
    /// <returns>'True' if a productivity type was selected, 'False' if it wasn't.</returns>
    private bool CanStartCountdown() => ProductivityType != ProductivityType.None;

    #endregion

    #region PublicMethods

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Starts the productivity countdown.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the countdown.</param>
    /// <returns></returns>
    public async Task StartCountdown(CancellationToken cancellationToken)
    {
        TimeSpan remainingTime = TimeSpan.FromSeconds(ProductivityChunkTime);

        try
        {
            while (remainingTime.TotalSeconds > 0 && !cancellationToken.IsCancellationRequested)
            {
                ProductivityProgressbarText = remainingTime.ToString(@"mm\:ss");
                ProductivityChunkProgressbarValue--;
                await Task.Delay(1000, cancellationToken);
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
        }
        catch (TaskCanceledException)
        {
            // Countdown cancelled
            ProductivityChunkProgressbarValue = ProductivityChunkTime;
            ProductivityProgressbarText = string.Empty;
            _logger.LogInformation("Productivity chunk task canceled successfully.");
        }

        ProductivityChunkProgressbarValue = ProductivityChunkTime;
        ProductivityProgressbarText = string.Empty;
        StartProductivityChunkButtonText = "Start Productivity Chunk";
        _isProductivityChunkActive = false;
        if (!cancellationToken.IsCancellationRequested)
            _messenger.Send(new DailyProductivityMessage("Increment"));
    }

    #endregion
}
