using System.Windows;
using System.Windows.Controls;

using MasterApplication.Models;

namespace MasterApplication.Feature.Md5HashFileGenerator;

/// <summary>
/// Interaction logic for Md5HashFileGeneratorView.xaml
/// </summary>
public partial class Md5HashFileGeneratorView : UserControl
{
    public Md5HashFileGeneratorView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Get the current hash of the selected row in the datagrid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
    {
        DataGrid? dataGrid = sender as DataGrid;
        if (dataGrid?.CurrentCell == null)
            return;

        Md5HashFile? selectedItem = dataGrid.CurrentItem as Md5HashFile;
        // Access the Hash value of the selected item
        string hashValue = selectedItem?.Hash ?? string.Empty;
        if (string.IsNullOrEmpty(hashValue))
            return;

        Clipboard.SetText(hashValue);
        if (DataContext is Md5HashFileGeneratorViewModel viewModel && viewModel.SnackbarMessageQueue is { } snackbarService)
            snackbarService.Enqueue($"'{hashValue}' copied");
    }
}
