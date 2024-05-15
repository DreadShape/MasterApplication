using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Services.Dialog;
using MasterApplication.UserControls;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

using YoutubeExplode;
using YoutubeExplode.Videos;

namespace MasterApplication.Feature.YoutubeAudioDownloader;

public partial class FileViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private string _fileLocation = null!;

    [ObservableProperty]
    private string _saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    [ObservableProperty]
    private string _status = "Waiting for file...";

    [ObservableProperty]
    private string _statusTextForeground = "#ffffff";

    [ObservableProperty]
    private string _audioTitle = null!;

    [ObservableProperty]
    private bool _isDownloadButtonEnabled = false;

    [ObservableProperty]
    private bool _isCancelButtonEnabled = false;

    [ObservableProperty]
    private bool _isProgressBarVisible = false;

    [ObservableProperty]
    private int _individualProgressBarValue;

    [ObservableProperty]
    private int _overallProgressBarValue;

    [ObservableProperty]
    private int _totalFileLinks;

    #endregion

    #region PrivateFields

    private readonly ILogger _logger;
    private readonly IDialogService _dialogService;
    private readonly YoutubeClient _youtubeClient;
    private readonly IProgress<double> _progressBar;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IList<string> _fileLinks;
    private readonly IList<string> _errorLinks;
    private readonly string _dialogIdentifier;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="FileViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    public FileViewModel(ILogger<LinkViewModel> logger, IDialogService dialogService)
    {
        _logger = logger;
        _dialogService = dialogService;
        _youtubeClient = new();
        _progressBar = new Progress<double>(x => IndividualProgressBarValue = (int)(x * 100));
        _cancellationTokenSource = new();
        _fileLinks = new List<string>();
        _errorLinks = new List<string>();
        _dialogIdentifier = "AudioDialog";
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects a file from where to get all the links to download.
    /// </summary>
    [RelayCommand]
    private async Task OnFileLocation()
    {
        string[] selectedFilePath = _dialogService.ShowOpenFileDialog("Text files...|*.txt", false);
        if (selectedFilePath != null && selectedFilePath.Length > 0)
            FileLocation = selectedFilePath.First();

        /*if (fileLocationDialog.ShowDialog() == DialogResult.OK)
        {
            FileLocation = fileLocationDialog.FileName;
            LoadingDialog loadingDialog = new("Analyzing links, please wait...");
            await DialogHost.Show(loadingDialog, _dialogIdentifier, OnCheckFileOpenAsync, OnCheckClosingOpenAsync);
        }*/
    }

    /// <summary>
    /// Selects a folder for where to download the audio.
    /// </summary>
    [RelayCommand]
    private void OnSaveLocation()
    {
        string selectedFolderPath = _dialogService.ShowOpenFolderDialog();
        if (!string.IsNullOrEmpty(selectedFolderPath))
            SaveLocation = selectedFolderPath;
    }

    [RelayCommand]
    private async Task OnDownload()
    {
        IsProgressBarVisible = true;
        ErrorDialog errorDialog;
        OverallProgressBarValue = 0;

        if (string.IsNullOrEmpty(FileLocation))
            return;

        if (string.IsNullOrEmpty(SaveLocation))
            return;

        /*foreach (string link in _fileLinks)
        {
            Status = "Downloading...";
            string fullPathAndName = string.Empty;
            try
            {
                Video? youtubeVideo = await _youtubeClient.Videos.GetAsync(link);
                VideoTitle = youtubeVideo.Title;
                string? videoName = NormalizeFileName(youtubeVideo.Title);
                fullPathAndName = Path.Combine(SaveLocation, $"{videoName}.mp3");

                if (File.Exists(fullPathAndName))
                {
                    continue;
                }

                await _youtubeClient.Videos.DownloadAsync(youtubeVideo.Id, fullPathAndName, _progressBar, _cancellationTokenSource.Token);
                OverallProgressBarValue++;
            }
            catch (Exception)
            {
                //TODO: Create logger to log other exceptions

                if (File.Exists(fullPathAndName))
                    File.Delete(fullPathAndName);

                IsProgressBarVisible = false;
            }
        }

        IsProgressBarVisible = false;
        VideoTitle = string.Empty;
        Status = "Waiting to file link...";

        if (!_errorLinks.Any())
            return;

        File.WriteAllLines(Path.Combine(SaveLocation, "invalidLinks.txt"), _errorLinks);
        errorDialog = new($"There were '{_errorLinks.Count}' invalid links that couldn't be downloaded found in the selected file.\nCheck the 'invalidLinks.txt' file created where the rest were downloaded for more information.");
        await DialogHost.Show(errorDialog, _dialogIdentifier);*/
    }

    /// <summary>
    /// Cancels the current downloading of the audio.
    /// </summary>
    [RelayCommand]
    private void OnCancel()
    {
        _cancellationTokenSource?.Cancel();
        ResetProgressBar();
        Status = "Ready to download";
        _logger.LogInformation("Canceled downloading of '{audioTitle}'", AudioTitle);
    }

    #endregion

    #region CommandValidations


    #endregion

    #region PublicMethods



    #endregion

    #region PrivateMethods

    /// <summary>
    /// Checks to see if the file has valid youtube links or not
    /// </summary>
    /// <param name="sender">Dialog that sent the event.</param>
    /// <param name="eventArgs"></param>
    /// <returns></returns>
    private async void OnCheckFileOpenAsync(object sender, DialogOpenedEventArgs eventArgs)
    {
        try
        {
            _cancellationTokenSource = new();
            _fileLinks.Clear();
            _errorLinks.Clear();
            IsDownloadButtonEnabled = false;
            YoutubeClient youtubeClient = new();

            /*if (string.IsNullOrEmpty(FileLocation))
            {
                VideoTitle = string.Empty;
                Status = "Waiting for file...";
                return;
            }

            string[] fileLines = File.ReadAllLines(FileLocation);
            Status = "Analyzing file links...";
            foreach (string line in fileLines)
            {
                string link = new(line.Split(',').FirstOrDefault());
                try
                {
                    Video? youtubeVideo = await youtubeClient.Videos.GetAsync(link, _cancellationTokenSource.Token);
                    _fileLinks.Add(link);
                }
                catch (Exception)
                {
                    _errorLinks.Add(link);
                }
            }

            if (!_fileLinks.Any())
            {
                Status = "All links inside the file are invalid.";
                IsDownloadButtonEnabled = false;
                eventArgs.Session.Close(false);
                return;
            }

            TotalFileLinks = _fileLinks.Count;
            Status = "Ready to download";
            IsDownloadButtonEnabled = true;
            eventArgs.Session.Close(false);*/
        }
        catch (Exception)
        {
            IsDownloadButtonEnabled = false;
        }
    }

    /// <summary>
    /// Checks to see if the file has valid youtube links or not
    /// </summary>
    /// <param name="sender">Dialog that sent the event.</param>
    /// <param name="eventArgs"></param>
    /// <returns></returns>
    private void OnCheckClosingOpenAsync(object sender, DialogClosingEventArgs eventArgs)
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Resets the <see cref="IProgress{T}"/> bar.
    /// </summary>
    private void ResetProgressBar()
    {
        IsProgressBarVisible = false;
        IndividualProgressBarValue = 0;
    }

    #endregion
}
