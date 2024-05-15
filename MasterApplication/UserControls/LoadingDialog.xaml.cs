using System.Windows.Controls;

namespace MasterApplication.UserControls;

/// <summary>
/// Interaction logic for LoadingDialog.xaml
/// </summary>
public partial class LoadingDialog : UserControl
{
    public LoadingDialog(string contentText)
    {
        InitializeComponent();
        ContentTextblock.Text = contentText ?? string.Empty;
    }
}
