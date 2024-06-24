using System.Collections.ObjectModel;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Extensions;
using MasterApplication.Models.Messages;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Features.Productivity;

public partial class DailyProductivityChunksViewModel : ObservableObject, IRecipient<DailyProductivityMessage>
{
    #region Properties

    public ObservableCollection<bool> AreBlocksVisible { get; private set; }

    [ObservableProperty]
    private string _productivityStatusText = string.Empty;

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly IList<string> _status = new List<string>() { "Day Wasted", "Not Great", "Did Something",
        "Almost Productive", "Productive", "Very Productive" };
    private int _currentChunkIndex = 0;

    private object _lockObject;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="DailyProductivityChunksViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    /// <param name="messenger"><see cref="IMessenger"/> to send/receive messenger from different parts of the application.</param>
    public DailyProductivityChunksViewModel(ILogger<DailyProductivityChunksViewModel> logger, ISnackbarMessageQueue snackbarMessageQueue, 
        IMessenger messenger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _messenger.Register(this);
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        _lockObject = new object();

        AreBlocksVisible = new ObservableCollection<bool> 
        { 
            false, false, false, false, false, false, 
            false, false, false, false, false, false,
        };

        ProductivityStatusText = CreateText();
    }



    #endregion

    #region CommandValidations

    #endregion

    #region Commands

    #endregion

    #region CommandValidations

    #endregion

    #region PublicMethods

    /// <summary>
    /// <see cref="IRecipient{TMessage}"/>' implementation to process the message received.
    /// </summary>
    /// <param name="message">The message received.</param>
    public void Receive(DailyProductivityMessage message)
    {
        if (message.Content.Equals("Increment", StringComparison.OrdinalIgnoreCase))
        {

            if (_currentChunkIndex >= 0 && _currentChunkIndex <= AreBlocksVisible.Count)
            {
                Task.Run(async () =>
                {
                    //We have to wait for the collection to update before we increment the index.
                    await AreBlocksVisible.SafeUpdateAsync(() => AreBlocksVisible[_currentChunkIndex] = true);

                    _currentChunkIndex++;
                });
            }

            ProductivityStatusText = CreateText();
            return;
        }

        if (message.Content.Equals("Reset", StringComparison.OrdinalIgnoreCase))
        {
            lock (_lockObject)
                _currentChunkIndex = 0;
            ProductivityStatusText = CreateText();
            return;
        }
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Creates the productivity text to show on the user control.
    /// </summary>
    /// <returns>The formatted text.</returns>
    private string CreateText()
    {
        int index = (_currentChunkIndex - 1) / 2;

        //We control the limits of below 0 to force the first text in the list to appear three times.
        if (index < 0)
            index = 0;

        if (index >= _status.Count)
            index = _status.Count -1;

        return $"{_currentChunkIndex}/{AreBlocksVisible.Count} - {_status.ElementAtOrDefault(index)}";
    }

    #endregion
}
