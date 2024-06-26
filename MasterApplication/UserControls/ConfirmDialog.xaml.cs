using System.Windows.Controls;

namespace MasterApplication.UserControls;

/// <summary>
/// Interaction logic for ConfirmDialog.xaml
/// </summary>
public partial class ConfirmDialog : UserControl
{
    public ConfirmDialog(string? contentText = null)
    {
        InitializeComponent();
        ContentTextblock.Text = contentText ?? "Are you sure you want to confirm the action?";
    }
}
