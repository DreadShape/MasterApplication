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

namespace MasterApplication.Feature.YoutubeAudioDownloader
{
    public partial class LinkViewModel : ObservableObject
    {
        #region Properties

        private string _link = null!;
        public string Link
        {
            get => _link;
            set
            {
                if (SetProperty(ref _link, value))
                {
                    Task.Run(async () =>
                    {
                        if (await VerifyLinkAsync())
                        {
                            IsDownloadButtonEnabled = true;
                            return;
                        }

                        IsDownloadButtonEnabled = false;
                    });
                }
            }
        }

        [ObservableProperty]
        private string _saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [ObservableProperty]
        private string _icon = "Check";

        [ObservableProperty]
        private string _iconAndTextForeground = HexColors.Default;

        [ObservableProperty]
        private bool _isIconVisible = false;

        [ObservableProperty]
        private string _status = "Waiting for link...";

        [ObservableProperty]
        private string _audioTitle = null!;

        [ObservableProperty]
        private bool _isProgressBarVisible = false;

        [ObservableProperty]
        private int _progressBarValue;

        [ObservableProperty]
        private bool _isDownloadButtonEnabled = false;

        [ObservableProperty]
        private bool _isCancelButtonEnabled = false;

        #endregion

        #region PrivateFields

        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly YoutubeClient _youtubeClient;
        private readonly IProgress<double> _progressBar;
        private CancellationTokenSource _cancellationTokenSource;
        private Video _youtubeAudio = null!;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates and instance of an <see cref="LinkViewModel"/>.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> to be able to log information, warnings and errors.</param>
        /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
        public LinkViewModel(ILogger<LinkViewModel> logger, IDialogService dialogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _youtubeClient = new();
            _progressBar = new Progress<double>(x => ProgressBarValue = (int)(x * 100));
            _cancellationTokenSource = new();
        }

        #endregion

        #region Commands

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
        /// Downloads the audio to the selected folder.
        /// </summary>
        /// <returns>'True' if the audio was downloaded correctly, 'False' if it didn't.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the '<see cref="Link"/>' or '<see cref="SaveLocation"/>' are empty.</exception>
        [RelayCommand]
        private async Task OnDownload()
        {
            _cancellationTokenSource = new();

            if (string.IsNullOrEmpty(Link))
                throw new ArgumentNullException(nameof(Link));

            if (string.IsNullOrEmpty(SaveLocation) || !Directory.Exists(SaveLocation))
                throw new ArgumentNullException(nameof(SaveLocation));

            if (string.IsNullOrEmpty(_youtubeAudio?.Id))
            {
                _logger.LogError("No audio Id found to download.");
                throw new ArgumentNullException("No audio Id found to download.");
            }

            string fullPathAndName = string.Empty;
            try
            {
                string audioName = Utils.NormalizeFileName(_youtubeAudio.Title);
                fullPathAndName = Path.Combine(SaveLocation, $"{audioName}.mp3");
                
                if (File.Exists(fullPathAndName))
                {
                    ResetProgressBar();
                    Status = "Audio already exists, download canceled.";
                    return;
                }

                Status = "Downloading...";
                IsProgressBarVisible = true;
                IsCancelButtonEnabled = true;
                _logger.LogInformation("Started downloading of {audioTitle} at {saveLocation}.", AudioTitle, SaveLocation);
                await _youtubeClient.Videos.DownloadAsync(_youtubeAudio.Id, fullPathAndName, _progressBar, _cancellationTokenSource.Token);

                Status = "Waiting for link...";
                _logger.LogInformation("{audioTitle} downloaded correctly.", AudioTitle);
                AudioTitle = string.Empty;
                Link = string.Empty;
                IsCancelButtonEnabled = false;
                ResetProgressBar();
            }
            catch (Exception ex)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    _logger.LogInformation("Audio {audioTitle} download canceled.", AudioTitle);
                else
                    _logger.LogError("Error trying to download audio {audioTitle}. Error: {exception}.", AudioTitle, ex);

                if (File.Exists(fullPathAndName))
                    File.Delete(fullPathAndName);

                IsCancelButtonEnabled = false;
                ResetProgressBar();
            }
        }

        /// <summary>
        /// Cancels the current downloading of the audio.
        /// </summary>
        [RelayCommand]
        private void OnCancel()
        {
            _cancellationTokenSource?.Cancel();
            ResetProgressBar();
            Status = "Ready to download...";
            _logger.LogInformation("Canceled downloading of {audioTitle}.", AudioTitle);
        }

        #endregion

        #region CommandValidations


        #endregion

        #region PublicMethods

        /// <summary>
        /// Verifies if the link is valid or not.
        /// </summary>
        /// <returns>'True' if the link is valid, 'False' if it isn't.</returns>
        public async Task<bool> VerifyLinkAsync()
        {
            IsDownloadButtonEnabled = false;
            IsIconVisible = false;
            IconAndTextForeground = HexColors.Default;

            if (string.IsNullOrEmpty(Link))
            {
                AudioTitle = string.Empty;
                Status = "Waiting for link...";
                return false;
            }

            try
            {
                Status = "Analyzing link...";
                _logger.LogInformation("Analyzing audio link {link}.", Link);
                _youtubeAudio = await _youtubeClient.Videos.GetAsync(Link ?? string.Empty);
                IsDownloadButtonEnabled = true;
                AudioTitle = _youtubeAudio.Title;
                ShowLinkIcon("Check", "#7fee7f");
                Status = "Ready to download...";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error trying to get the audio track. Error: {exception}.", ex);
                Status = "Invalid link";
                ShowLinkIcon("CloseBoxOutline", "#f27f7f");
                return false;
            }
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Shows the <see cref="LinkView"/> page's icon with a specific kind and color.
        /// </summary>
        /// <param name="kind">Type of icon.</param>
        /// <param name="hexColor">Color of the icon.</param>
        private void ShowLinkIcon(string kind, string hexColor)
        {
            Icon = kind;
            IsIconVisible = true;
            IconAndTextForeground = hexColor;
        }

        /// <summary>
        /// Resets the <see cref="IProgress{T}"/> bar.
        /// </summary>
        private void ResetProgressBar()
        {
            IsProgressBarVisible = false;
            ProgressBarValue = 0;
        }

        #endregion
    }
}
