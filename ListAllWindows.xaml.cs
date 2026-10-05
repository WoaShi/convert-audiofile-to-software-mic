using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using AudioToMicWPF.Services;

namespace AudioToMicWPF
{
    /// <summary>
    /// ListAllWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ListAllWindows : Window
    {
        public static Process? chatProcess;

        public ListAllWindows()
        {
            InitializeComponent();
            Title = LocalizationService.Instance.GetString("ListWindow_Title");
            LoadRunningProcesses();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ThemeService.Instance.ApplyWindowDarkMode(this, ThemeService.Instance.IsDark);
        }

        private void LoadRunningProcesses()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    processListBox.Items.Add(new ProcessDisplay(process));
                }
            }
        }

        private void SetFocusButton_Click(object sender, RoutedEventArgs e)
        {
            if (processListBox.SelectedItem is ProcessDisplay selectedProcessDisplay)
            {
                chatProcess = selectedProcessDisplay.Process;

                // 获取 MainWindow 的实例  
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.viewModel.ProgramName = selectedProcessDisplay.Name;
                }

                Close();
            }
        }

        private void processListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            processListBox.Items.Clear();
            LoadRunningProcesses();
        }
    }

    public class WindowInterop
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    public class ProcessDisplay
    {
        public string Name { get; set; }
        public Process Process { get; set; }

        public ProcessDisplay(Process process)
        {
            Process = process;
            Name = process.MainWindowTitle;
        }

        public override string ToString()
        {
            return Name; // 当 ListBox 显示 ProcessDisplay 对象时调用
        }
    }
}
