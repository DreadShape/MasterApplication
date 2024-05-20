namespace MasterApplication.Services.Dialog;

/// <summary>
/// Service to cover all the differet dialog options.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Opens a dialog to select a file or multiple ones.
    /// </summary>
    /// <param name="filter">Filter to what extension files to show. Default is every file.</param>
    /// <param name="allowMultipleFileSelection">Allows multiple selection of files. Default is 'True'.</param>
    /// <returns>All the selected files.</returns>
    string[] ShowOpenFileDialog(string filter = "Files...|*.*", bool allowMultipleFileSelection = true);

    /// <summary>
    /// Opens a dialog to select a folder.
    /// </summary>
    /// <returns>The folder selected.</returns>
    string ShowOpenFolderDialog();

    /// <summary>
    /// Opens a dialog to save a file.
    /// </summary>
    /// <returns>The file or path to where to save the file.</returns>
    string ShowSaveFileDialog();
}
