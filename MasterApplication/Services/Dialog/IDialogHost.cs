namespace MasterApplication.Services.Dialog;

/// <summary>
/// Interface for a dialog host to show up to the user.
/// </summary>
public interface IDialogHost
{
    /// <summary>
    /// Shows a pop up dialog host with the content presented and it places inside a xaml element with that identifier.
    /// </summary>
    /// <param name="content">Content to present.</param>
    /// <param name="dialogIdentifier">Where to place the dialog host.</param>
    /// <returns>'True' if the dialog is canceled, 'False' if it wasn't.</returns>
    Task<object?> Show(object content, string dialogIdentifier);
}
