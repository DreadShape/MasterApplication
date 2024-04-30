namespace MasterApplication.Services.Dialog;

/// <summary>
/// Service to cover all the differet dialog options.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Opens a dialog to select a file or multiple ones.
    /// </summary>
    /// <returns>All the files selected</returns>
    string[] ShowOpenFileDialog();

    /// <summary>
    /// Opens a dialog to select a folder.
    /// </summary>
    /// <returns>The folder selected</returns>
    string ShowOpenFolderDialog();

    /// <summary>
    /// Opens a dialog to save a file.
    /// </summary>
    /// <returns>The file or path to where to save the file</returns>
    string ShowSaveFileDialog();
}
