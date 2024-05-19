using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.BookReview;

public partial class BookReviewViewModel : ObservableObject
{
    #region Properties

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
    public BookReviewViewModel(ILogger<BookReviewViewModel> logger)
    {
        _logger = logger;
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

    }

    /// <summary>
    /// Clears all the form's inputs.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearForm))]
    private void OnClearForm()
    {
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
    private bool CanClearForm() => !string.IsNullOrEmpty(Summary) ||
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
