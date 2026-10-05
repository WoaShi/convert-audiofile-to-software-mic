using Microsoft.Win32;

namespace AudioToMicWPF.Services
{
    internal class FilePickerServices
    {
        public string OpenFilePicker()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = LocalizationService.Instance.GetString("FilePicker_FilterTitle"),
                Filter = "音频文件 (*.mp3;*.aac;*.flac;*.ogg;*.wav;*.m4a)|*.mp3;*.aac;*.flac;*.ogg;*.wav;*.m4a;*.wma",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                selectedFilePath = selectedFilePath.Replace("\\", @"\\");
                return selectedFilePath;
            }
            else
            {
                return LocalizationService.Instance.GetString("FilePicker_NoFile");
            }
        }
    }
}
