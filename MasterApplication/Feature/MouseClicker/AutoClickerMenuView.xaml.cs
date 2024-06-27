using System.Windows;

namespace MasterApplication.Feature.MouseClicker;

/// <summary>
/// Interaction logic for AutoClickerMenuView.xaml
/// </summary>
public partial class AutoClickerMenuView : Window
{
    public AutoClickerMenuView()
    {
        InitializeComponent();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (DataContext is AutoClickerMenuViewModel viewModel)
        {
            viewModel.OnWindowClosed(sender, e);
        }
    }
}
