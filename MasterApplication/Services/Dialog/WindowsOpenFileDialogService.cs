using Microsoft.Win32;

namespace MasterApplication.Services.Dialog
{
    public class WindowsOpenFileDialogService : IOpenFileDialogService
    {
        public string[] ShowDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select file/s...",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }

            return [];
        }
    }
}
