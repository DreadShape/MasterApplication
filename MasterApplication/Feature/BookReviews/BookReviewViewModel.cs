using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Models;
using MasterApplication.Models.Enums;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.BookReviews;

public partial class BookReviewViewModel : ObservableObject
{
    #region Properties

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private BookReviewType _bookReviewType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string _summary = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string _mainStory = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private bool _isLikedMainStory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string? _sideStories;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private bool _isLikedSideStories;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string _characters = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private bool _isLikedCharacters;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string _settingsAndAmbiance = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private bool _isLikedSettingsAndAmbiance;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string _ending = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private bool _isLikedEnding;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearFormCommand))]
    private string? _extensiveReview;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="HomeViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="snackbarMessageQueue"><see cref="ISnackbarMessageQueue"/> send a pop up message to the user interface.</param>
    public BookReviewViewModel(ILogger<BookReviewViewModel> logger, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SnackbarMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
    }

    #endregion

    #region CommandValidations

    #endregion

    #region Commands

    /// <summary>
    /// Copies to the clipboard the review created.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAcceptReview))]
    private void OnAcceptReview()
    {
        BookReview bookReview = new()
        {
            ReviewType = BookReviewType,
            Summary = Summary,
            ScoringSystem = "I have five things I look for in a book, if the book checks all five it's a 5/5 stars book, if it checks none it's a 1/5 stars and everything else is a combination:",

            MainStory = MainStory,
            IsLikedMainStory = IsLikedMainStory,

            SideStories = SideStories,
            IsLikedSideStories = IsLikedSideStories,

            Characters = Characters,
            IsLikedCharacters = IsLikedCharacters,

            SettingsAndAmbiance = SettingsAndAmbiance,
            IsLikedSettingsAndAmbiance = IsLikedSettingsAndAmbiance,

            Ending = Ending,
            IsLikedEnding = IsLikedEnding,

            ExtensiveReview = ExtensiveReview
        };

        // Goodreads review
        if (BookReviewType == BookReviewType.GoodReads)
        {
            bookReview.Summary = $"<b>TL;DR</b>{Environment.NewLine}{Summary}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";

            bookReview.ScoringSystem = $"<b>My Scoring System</b>{Environment.NewLine}{bookReview.ScoringSystem}{Environment.NewLine}{Environment.NewLine}";
            bookReview.MainStory = $"<b>{(IsLikedMainStory ? "&#x2713;" : "X")} - Main Story:</b> {bookReview.MainStory}{Environment.NewLine}{Environment.NewLine}";
            bookReview.SideStories = $"<b>{(IsLikedSideStories ? "&#x2713;" : "X")} - Side Stories (if it applies):</b> {bookReview.SideStories}{Environment.NewLine}{Environment.NewLine}";
            bookReview.Characters = $"<b>{(IsLikedCharacters ? "&#x2713;" : "X")} - Characters:</b> {bookReview.Characters}{Environment.NewLine}{Environment.NewLine}";
            bookReview.SettingsAndAmbiance = $"<b>{(IsLikedSettingsAndAmbiance ? "&#x2713;" : "X")} - Setting/Ambiance:</b> {bookReview.SettingsAndAmbiance}{Environment.NewLine}{Environment.NewLine}";
            bookReview.Ending = $"<b>{(IsLikedEnding ? "&#x2713;" : "X")} - Ending:</b> {bookReview.Ending}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";

            bookReview.ExtensiveReview = $"<b>Extensive Review</b>{Environment.NewLine}{bookReview.ExtensiveReview}";
        }

        if (BookReviewType == BookReviewType.Hardcover)
        {
            bookReview.Summary = $"TL;DR{Environment.NewLine}{Summary}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";

            bookReview.ScoringSystem = $"My Scoring System{Environment.NewLine}{bookReview.ScoringSystem}{Environment.NewLine}{Environment.NewLine}";
            bookReview.MainStory = $"{(IsLikedMainStory ? "&#x2713;" : "X")} - Main Story: {bookReview.MainStory}{Environment.NewLine}{Environment.NewLine}";
            bookReview.SideStories = $"{(IsLikedSideStories ? "&#x2713;" : "X")} - Side Stories (if it applies): {bookReview.SideStories}{Environment.NewLine}{Environment.NewLine}";
            bookReview.Characters = $"{(IsLikedCharacters ? "&#x2713;" : "X")} - Characters: {bookReview.Characters}{Environment.NewLine}{Environment.NewLine}";
            bookReview.SettingsAndAmbiance = $"{(IsLikedSettingsAndAmbiance ? "&#x2713;" : "X")} - Setting/Ambiance: {bookReview.SettingsAndAmbiance}{Environment.NewLine}{Environment.NewLine}";
            bookReview.Ending = $"{(IsLikedEnding ? "&#x2713;" : "X")} - Ending: {bookReview.Ending}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";

            bookReview.ExtensiveReview = $"Extensive Review{Environment.NewLine}{bookReview.ExtensiveReview}";
        }

        Clipboard.Clear();
        Clipboard.SetText(bookReview.ToClipboardString());

        //SendMessageToUI
        SnackbarMessageQueue.Enqueue("Book review copied and ready to be used!");
    }

    /// <summary>
    /// Clears all the form's inputs.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearForm))]
    private void OnClearForm()
    {
        BookReviewType = BookReviewType.GoodReads;
        Summary = string.Empty;

        MainStory = string.Empty;
        IsLikedMainStory = false;

        SideStories = string.Empty;
        IsLikedSideStories = false;

        Characters = string.Empty;
        IsLikedCharacters = false;

        SettingsAndAmbiance = string.Empty;
        IsLikedSettingsAndAmbiance = false;

        Ending = string.Empty;
        IsLikedEnding = false;

        ExtensiveReview = string.Empty;
    }

    #endregion

    #region CommandValidations

    /// <summary>
    /// Enables or disables the "AcceptReview" button on the UI based on if some of the inputs are not empty.
    /// </summary>
    /// <returns>'True' if some of the inputs are not empty, 'False' if they are.</returns>
    private bool CanAcceptReview() => !string.IsNullOrEmpty(Summary) &&
                                      !string.IsNullOrEmpty(MainStory) &&
                                      !string.IsNullOrEmpty(Characters) &&
                                      !string.IsNullOrEmpty(SettingsAndAmbiance) &&
                                      !string.IsNullOrEmpty(Ending);

    /// <summary>
    /// Enables or disables the "ClearForm" button on the UI based on if at least one input is not empty.
    /// </summary>
    /// <returns>'True' if at least one input isn't empty, 'False' if all of them are.</returns>
    private bool CanClearForm() => BookReviewType != BookReviewType.GoodReads ||
                                   !string.IsNullOrEmpty(Summary) ||
                                   !string.IsNullOrEmpty(MainStory) ||
                                   IsLikedMainStory ||
                                   !string.IsNullOrEmpty(SideStories) ||
                                   IsLikedSideStories ||
                                   !string.IsNullOrEmpty(Characters) ||
                                   IsLikedCharacters ||
                                   !string.IsNullOrEmpty(SettingsAndAmbiance) ||
                                   IsLikedSettingsAndAmbiance ||
                                   !string.IsNullOrEmpty(Ending) ||
                                   IsLikedEnding ||
                                   !string.IsNullOrEmpty(ExtensiveReview);

    #endregion

    #region PublicMethods

    #endregion

    #region PrivateMethods

    #endregion
}
