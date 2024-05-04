using System.Windows.Input;

using MasterApplication.Feature.Md5HashFileGenerator;
using MasterApplication.Menus.Other;

namespace MasterApplication;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Main container that it's job is to link all the view models to their respective views
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="otherViewModel"></param>
    /// <param name="md5HashFileGeneratorViewModel"></param>
    public MainWindow(MainWindowViewModel viewModel, OtherViewModel otherViewModel, Md5HashFileGeneratorViewModel md5HashFileGeneratorViewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        Menu_OtherView.DataContext = otherViewModel;
        Menu_OtherView.Feature_Md5HashFileGenerator.DataContext = md5HashFileGeneratorViewModel;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
