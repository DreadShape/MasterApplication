using System.Windows.Controls;

namespace MasterApplication.UserControls;

/// <summary>
/// Interaction logic for ErrorDialog.xaml
/// </summary>
public partial class ErrorDialog : UserControl
{
    public ErrorDialog(string contentText)
    {
        InitializeComponent();
        ContentTextblock.Text = contentText ?? "Generic error, please check the logs for more information.";
    }
}
