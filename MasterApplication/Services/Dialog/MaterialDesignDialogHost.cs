using MaterialDesignThemes.Wpf;

namespace MasterApplication.Services.Dialog;

/// <summary>
/// <see cref="IDialogHost"/>'s implementation that uses the MaterialDesignInXaml's dialog host to show up to the user.
/// </summary>
public class MaterialDesignDialogHost : IDialogHost
{
    /// <summary>
    /// Shows a pop up dialog host with the content presented and it places inside a xaml element with that identifier.
    /// </summary>
    /// <param name="content">Content to present.</param>
    /// <param name="dialogIdentifier">Where to place the dialog host.</param>
    /// <returns>'True' if the dialog is canceled, 'False' if it wasn't.</returns>
    public async Task<object?> Show(object content, string dialogIdentifier)
    {
        return await DialogHost.Show(content, dialogIdentifier);
    }
}
