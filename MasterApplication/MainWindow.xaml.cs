﻿using System.Windows.Input;

using MasterApplication.Features.BookReviews;
using MasterApplication.Features.Md5HashFileGenerator;
using MasterApplication.Features.Productivity;
using MasterApplication.Features.YoutubeAudioDownloader;
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
    /// <param name="productivityViewModel"><see cref="ProductivityView"/>'s view model.</param>
    /// <param name="dailyProductivityChunkViewModel"><see cref="DailyProductivityChunksView"/>'s view model.</param>
    public MainWindow(MainWindowViewModel viewModel, 
        OtherViewModel otherViewModel, 
        Md5HashFileGeneratorViewModel md5HashFileGeneratorViewModel, 
        YoutubeAudioDownloaderViewModel youtubeAudioDownloaderViewModel,
        LinkViewModel linkViewModel,
        FileViewModel fileViewModel,
        BookReviewViewModel bookReviewViewModel,
        ProductivityViewModel productivityViewModel,
        DailyProductivityChunksViewModel dailyProductivityChunkViewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        Menu_OtherView.DataContext = otherViewModel;
        Menu_OtherView.Feature_Md5HashFileGenerator.DataContext = md5HashFileGeneratorViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.DataContext = youtubeAudioDownloaderViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.LinkView.DataContext = linkViewModel;
        Menu_OtherView.Feature_YoutubeAudioDownloader.FileView.DataContext = fileViewModel;
        Menu_OtherView.Feature_BookReview.DataContext = bookReviewViewModel;

        Menu_ProductivityView.DataContext = productivityViewModel;
        Menu_ProductivityView.DaileProductivityChunkView.DataContext = dailyProductivityChunkViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
