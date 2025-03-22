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

    /// <summary>
    /// Select the entire text and place the cursor at the end.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBox? textBox = sender as TextBox;

        if (textBox != null) 
        {
            textBox.SelectAll();
        }
    }

    /// <summary>
    /// Makes it so that when I click on the text above the <see cref="CheckBox"/> it toggles it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ShowCoordinatesToggleButton.IsChecked = !ShowCoordinatesToggleButton.IsChecked;
    }
}
