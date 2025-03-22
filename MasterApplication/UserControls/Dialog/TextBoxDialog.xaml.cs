using System.Windows;
using System.Windows.Controls;

using MaterialDesignThemes.Wpf;

namespace MasterApplication.UserControls.Dialog;

/// <summary>
/// Interaction logic for TextBoxDialog.xaml
/// </summary>
public partial class TextBoxDialog : UserControl
{
    public string SequenceName { get; set; } = string.Empty;

    private readonly IList<string> _invalidNames = new List<string>();

    /// <summary>
    /// Creates and instance of a <see cref="TextBoxDialog"/>.
    /// </summary>
    /// <param name="title">Title of the <see cref="TextBoxDialog"/>.</param>
    /// <param name="content">Content of the <see cref="TextBoxDialog"/>.</param>
    /// <param name="invalidNames">List of name that are not valid.</param>
    public TextBoxDialog(string title, string content, IList<string> invalidNames)
    {
        InitializeComponent();
        AcceptButton.IsEnabled = false;
        DialogTitle.Text = string.IsNullOrEmpty(title) ? "TextBox Title" : title;
        ContentTextblock.Text = string.IsNullOrEmpty(content) ? "TextBox Content" : content;
        _invalidNames = invalidNames;
    }

    /// <summary>
    /// Handle the "AcceptButton" click.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        SequenceName = TextBoxContent.Text;
        DialogHost.CloseDialogCommand.Execute(false, (Button)sender);
    }

    /// <summary>
    /// Enable the "AcceptButton" only if there's text in the textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxContent_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsTextInInvalidNames() || string.IsNullOrEmpty(TextBoxContent.Text))
        {
            AcceptButton.IsEnabled = false;
            return;
        }

        AcceptButton.IsEnabled = true;
    }

    /// <summary>
    /// Checks the current text to see if it's in the invalid name list.
    /// </summary>
    /// <returns></returns>
    private bool IsTextInInvalidNames()
    {
        foreach (string invalidName in _invalidNames)
        {
            if (invalidName.Equals(TextBoxContent.Text, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
