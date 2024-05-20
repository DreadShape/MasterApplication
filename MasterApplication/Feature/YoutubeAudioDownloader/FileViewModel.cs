using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Helpers;
using MasterApplication.Models;
using MasterApplication.Services.Dialog;

using Microsoft.Extensions.Logging;

using YoutubeExplode;
using YoutubeExplode.Converter;
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
    private string _statusTextForeground = HexColors.Default;

    [ObservableProperty]
    private string _audioTitle = null!;

    [ObservableProperty]
    private string _errorText = null!;

    [ObservableProperty]
    private bool _isErrorTextVisible = false;

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

    #endregion

    #region Constructor

    /// <summary>
    /// Creates and instance of an <see cref="FileViewModel"/>.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
    /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
    public FileViewModel(ILogger<FileViewModel> logger, IDialogService dialogService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _youtubeClient = new();
        _progressBar = new Progress<double>(x => IndividualProgressBarValue = (int)(x * 100));
        _cancellationTokenSource = new();
        _fileLinks = new List<string>();
        _errorLinks = new List<string>();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects a file from where to get all the links to download.
    /// </summary>
    [RelayCommand]
    private async Task OnFileLocation()
    {
        IsErrorTextVisible = false;
        string[] selectedFilePath = _dialogService.ShowOpenFileDialog("Text files...|*.txt", false);
        if (selectedFilePath == null || selectedFilePath.Length <= 0)
        {
            AudioTitle = string.Empty;
            StatusTextForeground = HexColors.Default;
            Status = "Waiting for file...";
            return;
        }

        FileLocation = selectedFilePath.First();
        if (!await AreLinksInFileValid())
        {
            StatusTextForeground = HexColors.Error;
            Status = "All links inside the file are invalid.";
            IsDownloadButtonEnabled = false;
            return;
        }

        // We only save a file with the invalid links if there are some valid ones as well, if all the links are invalid we don't need to create a file.
        if (_errorLinks.Any() && _fileLinks.Any())
        {
            ErrorText = $"'{_errorLinks.Count}' invalid {(_errorLinks.Count > 1 ? "links" : "link")} in the selected file. Check 'invalidLinks.txt' file created in the save location.";
            IsErrorTextVisible = true;
            string invalidLinksFilePath = Path.Combine(SaveLocation, "invalidLinks.txt");
            _logger.LogInformation("All the invalid links have been saved in the file {file}.", invalidLinksFilePath);
            File.WriteAllLines(invalidLinksFilePath, _errorLinks);
        }

        StatusTextForeground = HexColors.Success;
        Status = "Ready to download...";
        IsDownloadButtonEnabled = true;
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

    /// <summary>
    /// Downloads the valid audio links found in the selected file.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown when the '<see cref="FileLocation"/>' or '<see cref="SaveLocation"/>' are empty.</exception>
    [RelayCommand]
    private async Task OnDownload()
    {
        _cancellationTokenSource = new();
        IsProgressBarVisible = true;
        IsCancelButtonEnabled = true;
        OverallProgressBarValue = 0;
        

        if (string.IsNullOrEmpty(FileLocation))
            throw new ArgumentNullException(nameof(FileLocation));

        if (string.IsNullOrEmpty(SaveLocation))
            throw new ArgumentNullException(nameof(SaveLocation));

        for (int i = 0; i < _fileLinks.Count; i++)
        {
            IndividualProgressBarValue = 0;
            Status = $"Downloading... {i}/{_fileLinks.Count}";
            string fullPathAndName = string.Empty;
            try
            {
                Video youtubeVideo = await _youtubeClient.Videos.GetAsync(_fileLinks[i]);
                AudioTitle = youtubeVideo.Title;
                string audioName = Utils.NormalizeFileName(youtubeVideo.Title);
                fullPathAndName = Path.Combine(SaveLocation, $"{audioName}.mp3");

                if (File.Exists(fullPathAndName))
                    continue;

                await _youtubeClient.Videos.DownloadAsync(youtubeVideo.Id, fullPathAndName, _progressBar, _cancellationTokenSource.Token);
                OverallProgressBarValue++;
            }
            catch (Exception ex)
            {
                if (File.Exists(fullPathAndName))
                    File.Delete(fullPathAndName);

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _logger.LogInformation("Audio {audioTitle} download canceled.", AudioTitle);
                    return;
                }
                else
                    _logger.LogError("Error trying to download audio {audioTitle}. Error: {exception}.", AudioTitle, ex);
            }
        }

        ResetProgressBar();
        AudioTitle = string.Empty;
        StatusTextForeground = HexColors.Default;
        Status = "Waiting for file...";
        IsDownloadButtonEnabled = false;
    }

    /// <summary>
    /// Cancels the current downloading of the audio.
    /// </summary>
    [RelayCommand]
    private void OnCancel()
    {
        ResetProgressBar();
        AudioTitle = string.Empty;
        StatusTextForeground = HexColors.Success;
        Status = "Ready to download...";
        IsDownloadButtonEnabled = true;
        IsCancelButtonEnabled = false;
        _cancellationTokenSource?.Cancel();
    }

    #endregion

    #region CommandValidations


    #endregion

    #region PublicMethods



    #endregion

    #region PrivateMethods

    /// <summary>
    /// Analyzes the file to see if it contains valid links.
    /// </summary>
    private async Task<bool> AreLinksInFileValid()
    {
        try
        {
            _fileLinks.Clear();
            _errorLinks.Clear();
            YoutubeClient youtubeClient = new();

            string[] fileLines = File.ReadAllLines(FileLocation);
            
            for (int i = 0; i < fileLines.Length; i++)
            {
                StatusTextForeground = HexColors.Default;
                Status = $"Analyzing links, please wait... {i}/{fileLines.Length}";
                _logger.LogInformation("Analyzing audio link {link}.", fileLines[i]);

                string link = new(fileLines[i].Split(',').FirstOrDefault());
                try
                {
                    Video? youtubeVideo = await youtubeClient.Videos.GetAsync(link, _cancellationTokenSource.Token);
                    _fileLinks.Add(link);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error trying to get the audio track. Error: {exception}.", ex);
                    _errorLinks.Add(link);
                }
            }

            if (!_fileLinks.Any())
                return false;

            TotalFileLinks = _fileLinks.Count;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error trying to analyze the file {fileLocation}. Error: {exception}.", FileLocation, ex);
            return false;
        }
    }

    /// <summary>
    /// Resets the overall <see cref="IProgress{T}"/> bar.
    /// </summary>
    private void ResetProgressBar()
    {
        IsProgressBarVisible = false;
        OverallProgressBarValue = 0;
        IndividualProgressBarValue = 0;
    }

    #endregion
}
