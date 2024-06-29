using System.Windows.Input;

using MasterApplication.Feature.BookReviews;
using MasterApplication.Feature.Md5HashFileGenerator;
using MasterApplication.Feature.MouseClicker;
using MasterApplication.Feature.YoutubeAudioDownloader;
using MasterApplication.Helpers;
using MasterApplication.Menus.Other;

namespace MasterApplication;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Main container that it's job is to link all the view models to their respective views.
    /// </summary>
    /// <param name="viewModel">This windows view model.</param>
    /// <param name="otherViewModel"><see cref="OtherView"/>'s menu view model.</param>
    /// <param name="md5HashFileGeneratorViewModel"><see cref="Md5HashFileGeneratorView"/>'s view model.</param>
    /// <param name="youtubeAudioDownloaderViewModel"><see cref="YoutubeAudioDownloaderView"/>'s view model.</param>
    /// <param name="linkViewModel"><see cref="LinkView"/>'s view model.</param>
    /// <param name="bookReviewViewModel"><see cref="BookReviewView"/>'s view model.</param>
    /// <param name="mouseClickerViewModel"><see cref="MouseClickerView"/>'s view model.</param>
    public MainWindow(MainWindowViewModel viewModel, 
        OtherViewModel otherViewModel, 
        Md5HashFileGeneratorViewModel md5HashFileGeneratorViewModel, 
        YoutubeAudioDownloaderViewModel youtubeAudioDownloaderViewModel,
        LinkViewModel linkViewModel,
        FileViewModel fileViewModel,
        BookReviewViewModel bookReviewViewModel,
        MouseClickerViewModel mouseClickerViewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        Menu_OtherView.DataContext = otherViewModel;
        Menu_OtherView.Feature_Md5HashFileGenerator.DataContext = md5HashFileGeneratorViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.DataContext = youtubeAudioDownloaderViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.LinkView.DataContext = linkViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.FileView.DataContext = fileViewModel;
        Menu_OtherView.Feature_BookReview.DataContext = bookReviewViewModel;
        Menu_OtherView.Feature_MouseClicker.DataContext = mouseClickerViewModel;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Utils.CloseAllProcessesWithSameName();
    }
}
