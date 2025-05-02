using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MasterApplication.Feature.MouseClicker;

/// <summary>
/// Interaction logic for MouseClickerView.xaml
/// </summary>
public partial class MouseClickerView : UserControl
{
    private MouseClickerViewModel _viewModel = null!;

    public MouseClickerView()
    {
        InitializeComponent();

        DataContextChanged += MouseClickerView_DataContextChanged;
        
    }

    private void TemplateImage_Loaded(object sender, RoutedEventArgs e)
    {
        // Ensure the canvas is the same size as the image
        OverlayCanvas.Width = TemplateImage.ActualWidth;
        OverlayCanvas.Height = TemplateImage.ActualHeight;
    }

    /// <summary>
    /// Show an overlay over the image based on what coordinates there are.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShowCoordinatesToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle && toggle.IsChecked is bool isChecked)
        {
            _viewModel.ShowCoordinatesOnImageCommand.Execute(isChecked);

            if (!isChecked || (_viewModel.ClickCoordinates.X == 0 && _viewModel.ClickCoordinates.Y == 0))
            {
                HideCross();
                return;
            }

            DrawCross(_viewModel.ClickCoordinates);
        }
    }

    /// <summary>
    /// Draws an overlay at the specified coordinates.
    /// </summary>
    /// <param name="point">Where to draw the cross.</param>
    private void DrawCross(Point point)
    {
        const double crossSize = 6;

        CrossLine1.X1 = point.X - crossSize;
        CrossLine1.Y1 = point.Y;
        CrossLine1.X2 = point.X + crossSize;
        CrossLine1.Y2 = point.Y;

        CrossLine2.X1 = point.X;
        CrossLine2.Y1 = point.Y - crossSize;
        CrossLine2.X2 = point.X;
        CrossLine2.Y2 = point.Y + crossSize;

        CrossLine1.Visibility = Visibility.Visible;
        CrossLine2.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the overlay cross.
    /// </summary>
    private void HideCross()
    {
        CrossLine1.Visibility = Visibility.Collapsed;
        CrossLine2.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Detect when the DataContext changes so we can get the <see cref="MouseClickerViewModel"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MouseClickerView_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is MouseClickerViewModel viewModel)
            _viewModel = viewModel;
    }

    /// <summary>
    /// Handles text input to ensure only valid decimal numbers are allowed in the TextBox.
    /// Simulates the resulting text after the new input and blocks it if it does not match the allowed pattern.
    /// </summary>
    /// <param name="sender">The source TextBox control.</param>
    /// <param name="e">The input event arguments.</param>
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Predict what the text would be after input
            string fullText = GetPreviewText(textBox, e.Text);
            e.Handled = !IsTextAllowed(fullText);
        }
    }

    /// <summary>
    /// Allows specific control keys (e.g., Backspace, Delete, Tab) during key down events in the TextBox.
    /// Prevents the PreviewTextInput logic from interfering with essential editing keys.
    /// </summary>
    /// <param name="sender">The source object.</param>
    /// <param name="e">The key event arguments.</param>
    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Allow control keys like Backspace, Delete, Tab, etc.
        e.Handled = false;
    }

    /// <summary>
    /// Validates whether the given text matches the format of a decimal number.
    /// Allows optional digits before and after a single decimal point.
    /// </summary>
    /// <param name="text">The full string to validate.</param>
    /// <returns><c>true</c> if the text is a valid decimal number; otherwise, <c>false</c>.</returns>
    private static bool IsTextAllowed(string text)
    {
        return Regex.IsMatch(text, @"^\d*\.?\d*$");
    }

    /// <summary>
    /// Simulates the resulting TextBox content if the current input were to be accepted.
    /// Useful for validating input against the final text, including selections being overwritten.
    /// </summary>
    /// <param name="textBox">The TextBox receiving input.</param>
    /// <param name="input">The new input character(s).</param>
    /// <returns>The predicted text result after applying the input.</returns>
    private static string GetPreviewText(TextBox textBox, string input)
    {
        var currentText = textBox.Text;
        int selectionStart = textBox.SelectionStart;
        int selectionLength = textBox.SelectionLength;

        // Simulate what the text would be after input
        string newText = currentText.Remove(selectionStart, selectionLength);
        return newText.Insert(selectionStart, input);
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
