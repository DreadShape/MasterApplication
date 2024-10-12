using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasterApplication.Feature.MouseClicker;

/// <summary>
/// Interaction logic for MouseClickerView.xaml
/// </summary>
public partial class MouseClickerView : UserControl
{
    public MouseClickerView()
    {
        InitializeComponent();
    }

    private void DelayBeforeClickingTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow only numeric input using regex
        e.Handled = !IsTextAllowed(e.Text);
    }

    private static bool IsTextAllowed(string text)
    {
        // Regex that matches only digits
        return Regex.IsMatch(text, "^[0-9]+$");
    }

    // Optional: Handle the Backspace, Delete, and Tab keys to allow them
    private void DelayBeforeClickingTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab)
        {
            e.Handled = false; // Allow these keys
        }
    }
}
