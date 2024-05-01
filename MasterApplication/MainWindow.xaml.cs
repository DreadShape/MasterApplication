using System.Windows.Input;

using MasterApplication.Feature.Md5HashFileGenerator;
using MasterApplication.Menus.Other;

namespace MasterApplication;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowViewModel viewModel, Md5HashFileGeneratorViewModel md5ViewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        Md5View.DataContext = md5ViewModel;
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
