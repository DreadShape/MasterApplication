using Microsoft.Win32;

namespace MasterApplication.Services.Dialog
{
    public class WindowsDialogService : IDialogService
    {
        /// <summary>
        /// Opens a dialog to select a file or multiple ones.
        /// </summary>
        /// <returns>All the files selected.</returns>
        public string[] ShowOpenFileDialog(string filter = "Files...|*.*", bool allowMultipleFileSelection = true)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select file/s...",
                Filter = filter,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = allowMultipleFileSelection
            };

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileNames;

            return [];
        }

        /// <summary>
        /// Opens a dialog to select a folder.
        /// </summary>
        /// <returns>The folder selected.</returns>
        public string ShowOpenFolderDialog()
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Title = "Select folder...",
                Multiselect = false
            };

            if (openFolderDialog.ShowDialog() == true)
                return openFolderDialog.FolderName;

            return string.Empty;
        }

        /// <summary>
        /// Opens a dialog to save a file.
        /// </summary>
        /// <returns>The file or path to where to save the file.</returns>
        public string ShowSaveFileDialog()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "Select folder...",
                Filter = "Text Files (*.txt)|*.txt",
            };

            if (saveFileDialog.ShowDialog() == true)
                return saveFileDialog.FileName;

            return string.Empty;
        }
    }
}
