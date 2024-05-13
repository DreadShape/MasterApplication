using System.Collections;
using System.ComponentModel;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MasterApplication.Services.Dialog;

using Microsoft.Extensions.Logging;

namespace MasterApplication.Feature.YoutubeAudioDownloader
{
    public partial class LinkViewModel : ObservableObject
    {
        #region Properties

        private string _link;
        public string Link
        {
            get => _link;
            set
            {
                if (SetProperty(ref _link, value))
                {
                    Task.Run(async () =>
                    {
                        if (await VerifyLink())
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
        private string _iconForeground;

        [ObservableProperty]
        private bool _isIconVisible = false;

        [ObservableProperty]
        private string _status = "Waiting for link...";

        [ObservableProperty]
        private string _videoTitle;

        [ObservableProperty]
        private bool _isProgressBarVisible = false;

        [ObservableProperty]
        private int _progressBarValue;

        [ObservableProperty]
        private bool _isDownloadButtonEnabled;

        #endregion

        #region PrivateFields

        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates and instanceo of an <see cref="LinkViewModel"/>.
        /// </summary>
        /// <param name="logger"><see cref="Logger"/> to be able to log information, warnings and errors.</param>
        /// <param name="dialogService"><see cref="IDialogService"/> open dialogs for the user.</param>
        public LinkViewModel(ILogger<LinkViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _propertyNameToErrors = new();
        }

        #endregion

        #region CommandValidations


        #endregion

        #region ErrorValidators

        private readonly Dictionary<string, IList<string>> _propertyNameToErrors;

        public bool HasErrors => _propertyNameToErrors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Gets all the errors of a property.
        /// </summary>
        /// <param name="propertyName">Name of the property containing errors</param>
        /// <returns>All the errors of that property</returns>
        public IEnumerable GetErrors(string? propertyName)
        {
            return string.IsNullOrEmpty(propertyName)
                ? new List<string>()
                : (IEnumerable)_propertyNameToErrors.GetValueOrDefault(propertyName, new List<string>());
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
        /// <returns>'True' if the audio was downloaded correctly, 'False' if it didn't</returns>
        [RelayCommand]
        private async Task OnDownload()
        {
            if (string.IsNullOrEmpty(Link))
                return;

            if (string.IsNullOrEmpty(SaveLocation) || !Directory.Exists(SaveLocation))
                return;

            /*if (string.IsNullOrEmpty(_youtubeVideo?.Id))
                return;

            string fullPathAndName = string.Empty;
            try
            {
                string? videoName = NormalizeFileName(_youtubeVideo.Title);
                fullPathAndName = Path.Combine(SaveLocation, $"{videoName}.mp3");
                IsProgressBarVisible = true;

                if (File.Exists(fullPathAndName))
                {
                    IsProgressBarVisible = false;
                    return;
                }

                Status = "Downloading...";
                await _youtubeClient.Videos.DownloadAsync(_youtubeVideo.Id, fullPathAndName, _progressBar, _cancellationTokenSource.Token);
                Status = "Waiting for link...";
                VideoTitle = string.Empty;
                Link = string.Empty;
                IsProgressBarVisible = false;
            }
            catch (Exception)
            {
                //TODO: Create logger to log other exceptions

                if (File.Exists(fullPathAndName))
                    File.Delete(fullPathAndName);

                IsProgressBarVisible = false;
            }*/
        }

        /// <summary>
        /// Cancels the current downloading of the audio.
        /// </summary>
        [RelayCommand]
        private void OnCancel()
        {
            /*_cancellationTokenSource?.Cancel();
            Status = "Ready to download";
            IsProgressBarVisible = false;*/
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verifies if the link is valid or not.
        /// </summary>
        /// <returns>'True' if the link is valid, 'False' if it isn't</returns>
        private async Task<bool> VerifyLink()
        {
            return false;
            /*IsDownloadButtonEnabled = false;
            IsIconVisible = false;
            if (string.IsNullOrEmpty(Link))
            {
                VideoTitle = string.Empty;
                Status = "Waiting for link...";
                return;
            }

            try
            {
                Status = "Analyzing link...";
                _youtubeVideo = await _youtubeClient.Videos.GetAsync(Link ?? string.Empty);
                IsDownloadButtonEnabled = true;
                VideoTitle = _youtubeVideo.Title;
                ShowLinkIcon("Check", "#00de00");
                Status = "Ready to download";
            }
            catch (Exception)
            {
                Status = "Invalid link";
                ShowLinkIcon("CloseBoxOutline", "#e60000");
            }*/
        }

        #endregion
    }
}
