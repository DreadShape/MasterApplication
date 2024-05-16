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
        ContentTextbox.Text = contentText ?? string.Empty;
    }
}
