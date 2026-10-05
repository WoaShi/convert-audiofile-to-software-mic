using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioToMicWPF.Services;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;

namespace AudioToMicWPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string? _selectedFilePath;
        private string? _programName;
        private string _driverStatusDescription = string.Empty;
        private string _driverActionButtonText = string.Empty;
        private string _themeIconGlyph = "\uE708";
        private string _themeButtonToolTip = "切换至深色模式";
        private string _languageButtonText = "EN";
        private string _windowTitle = "音频文件转软件语音";

        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (_selectedFilePath != value)
                {
                    _selectedFilePath = value;
                    OnPropertyChanged(nameof(SelectedFilePath));
                }
            }
        }

        public string? ProgramName
        {
            get => _programName;
            set
            {
                if (_programName != value)
                {
                    _programName = value;
                    OnPropertyChanged(nameof(ProgramName));
                }
            }
        }

        public string DriverStatusDescription
        {
            get => _driverStatusDescription;
            set
            {
                if (_driverStatusDescription != value)
                {
                    _driverStatusDescription = value;
                    OnPropertyChanged(nameof(DriverStatusDescription));
                }
            }
        }

        public string DriverActionButtonText
        {
            get => _driverActionButtonText;
            set
            {
                if (_driverActionButtonText != value)
                {
                    _driverActionButtonText = value;
                    OnPropertyChanged(nameof(DriverActionButtonText));
                }
            }
        }

        public bool IsDriverInstalled { get; set; }

        public string ThemeIconGlyph
        {
            get => _themeIconGlyph;
            set
            {
                if (_themeIconGlyph != value)
                {
                    _themeIconGlyph = value;
                    OnPropertyChanged(nameof(ThemeIconGlyph));
                }
            }
        }

        public string ThemeButtonToolTip
        {
            get => _themeButtonToolTip;
            set
            {
                if (_themeButtonToolTip != value)
                {
                    _themeButtonToolTip = value;
                    OnPropertyChanged(nameof(ThemeButtonToolTip));
                }
            }
        }

        public string LanguageButtonText
        {
            get => _languageButtonText;
            set
            {
                if (_languageButtonText != value)
                {
                    _languageButtonText = value;
                    OnPropertyChanged(nameof(LanguageButtonText));
                }
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (_windowTitle != value)
                {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        private double _driverActionButtonFontSize = 12;
        public double DriverActionButtonFontSize
        {
            get => _driverActionButtonFontSize;
            set
            {
                if (_driverActionButtonFontSize != value)
                {
                    _driverActionButtonFontSize = value;
                    OnPropertyChanged(nameof(DriverActionButtonFontSize));
                }
            }
        }

        public bool IsChineseSelected => LocalizationService.Instance.CurrentLanguage == AppLanguage.Chinese;
        public bool IsEnglishSelected => LocalizationService.Instance.CurrentLanguage == AppLanguage.English;

        public void RefreshLocalizationAndTheme()
        {
            bool isDark = ThemeService.Instance.IsDark;
            ThemeIconGlyph = isDark ? "\uE706" : "\uE708";
            ThemeButtonToolTip = LocalizationService.Instance.GetString(isDark ? "Header_Theme_ToLight" : "Header_Theme_ToDark");

            LanguageButtonText = LocalizationService.Instance.GetString("Header_Language_BtnText");
            WindowTitle = LocalizationService.Instance.GetString("App_Title");

            DriverStatusDescription = IsDriverInstalled
                ? LocalizationService.Instance.GetString("Driver_Status_Installed")
                : LocalizationService.Instance.GetString("Driver_Status_NotInstalled");
            DriverActionButtonText = IsDriverInstalled
                ? LocalizationService.Instance.GetString("Driver_Action_Website")
                : LocalizationService.Instance.GetString("Driver_Action_Install");

            // 只有文本长度超出按钮单行容纳限制（溢出）时才缩小字号为 10.5；正常情况下保持未缩小的 12pt 标准字号
            DriverActionButtonFontSize = DriverActionButtonText.Length > 6 ? 10.5 : 12;

            OnPropertyChanged(nameof(IsChineseSelected));
            OnPropertyChanged(nameof(IsEnglishSelected));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        public enum DialogButtonResult
        {
            Primary,
            Close
        }

        private TaskCompletionSource<DialogButtonResult>? _dialogTcs;

        public MainViewModel viewModel = new MainViewModel();
        private readonly FindDevicesServices findDevicesServices;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            findDevicesServices = new FindDevicesServices();

            // 订阅主题与语言切换事件
            ThemeService.Instance.ThemeChanged += OnThemeChanged;
            LocalizationService.Instance.LanguageChanged += OnLanguageChanged;

            RefreshDriverState();
            viewModel.RefreshLocalizationAndTheme();
        }

        private void OnThemeChanged(bool isDark)
        {
            viewModel.RefreshLocalizationAndTheme();
            TryApplyMica();
        }

        private void OnLanguageChanged(AppLanguage lang)
        {
            viewModel.RefreshLocalizationAndTheme();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            TryApplyMica();
        }

        private void TryApplyMica()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                ThemeService.Instance.ApplyWindowDarkMode(this, ThemeService.Instance.IsDark);

                if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000)
                {
                    int backdropType = 2; // DWMSBT_MAINWINDOW (Mica)
                    int hr = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
                    if (hr == 0)
                    {
                        Background = Brushes.Transparent;
                    }
                }
            }
            catch
            {
                // 系统不支持或异常时保持默认背景色
            }
        }

        public Task<DialogButtonResult> ShowDialogAsync(string title, object content, string? primaryText = null, string? closeText = null)
        {
            closeText ??= LocalizationService.Instance.GetString("Dialog_OK");

            DialogTitle.Text = title;
            DialogContent.Content = content;

            if (string.IsNullOrEmpty(primaryText))
            {
                DialogPrimaryBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                DialogPrimaryBtn.Content = primaryText;
                DialogPrimaryBtn.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(closeText))
            {
                DialogCloseBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                DialogCloseBtn.Content = closeText;
                DialogCloseBtn.Visibility = Visibility.Visible;
            }

            DialogHost.Visibility = Visibility.Visible;

            _dialogTcs = new TaskCompletionSource<DialogButtonResult>();
            return _dialogTcs.Task;
        }

        private void OnDialogPrimary_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Visibility = Visibility.Collapsed;
            _dialogTcs?.TrySetResult(DialogButtonResult.Primary);
        }

        private void OnDialogClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Visibility = Visibility.Collapsed;
            _dialogTcs?.TrySetResult(DialogButtonResult.Close);
        }

        private void RefreshDriverState()
        {
            findDevicesServices.RefreshDevices();
            var virtualPipelineInfo = VirtualAudioDriverService.DetectPipeline();
            viewModel.IsDriverInstalled = virtualPipelineInfo.IsInstalled;
            viewModel.RefreshLocalizationAndTheme();
        }

        private void OnPickFile(object sender, RoutedEventArgs e)
        {
            string text = new FilePickerServices().OpenFilePicker();
            string notSelected = LocalizationService.Instance.GetString("FilePicker_NoFile");
            if (text != notSelected && text != "未选择音频文件！")
            {
                viewModel.SelectedFilePath = text;
            }
        }

        private void OnRefreshDevices(object sender, RoutedEventArgs e)
        {
            RefreshDriverState();
        }

        private void OnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.Instance.ToggleTheme();
        }

        private void OnLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void OnSelectChinese_Click(object sender, RoutedEventArgs e)
        {
            LocalizationService.Instance.SetLanguage(AppLanguage.Chinese);
        }

        private void OnSelectEnglish_Click(object sender, RoutedEventArgs e)
        {
            LocalizationService.Instance.SetLanguage(AppLanguage.English);
        }

        private async void OnViewPipeline_Click(object sender, RoutedEventArgs e)
        {
            var virtualPipelineInfo = VirtualAudioDriverService.DetectPipeline();
            string pipelineTitle = LocalizationService.Instance.GetString("Dialog_Pipeline_Title");
            string okText = LocalizationService.Instance.GetString("Dialog_OK");

            if (virtualPipelineInfo.IsInstalled)
            {
                var panel = new StackPanel { Width = 360, Margin = new Thickness(0, 4, 0, 0) };

                // 播放输出端节点
                panel.Children.Add(CreatePipelineDeviceCard(
                    LocalizationService.Instance.GetString("Dialog_Pipeline_PlaybackOutput"),
                    virtualPipelineInfo.OutputDeviceName,
                    "\uE767"));

                // 连接指示
                var arrowStack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 6, 0, 6)
                };
                arrowStack.Children.Add(new TextBlock
                {
                    FontFamily = new System.Windows.Media.FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                    Text = "\uE74B",
                    FontSize = 14,
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"]
                });
                panel.Children.Add(arrowStack);

                // 录音输入端节点
                panel.Children.Add(CreatePipelineDeviceCard(
                    LocalizationService.Instance.GetString("Dialog_Pipeline_RecordInput"),
                    virtualPipelineInfo.InputDeviceName,
                    "\uE720"));

                await ShowDialogAsync(pipelineTitle, panel, null, okText);
            }
            else
            {
                var panel = new StackPanel { Width = 360, Margin = new Thickness(0, 4, 0, 0) };

                panel.Children.Add(new TextBlock
                {
                    Text = LocalizationService.Instance.GetString("Dialog_Pipeline_DriverNotInstalled"),
                    FontSize = 13,
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimaryBrush"],
                    Margin = new Thickness(0, 4, 0, 10)
                });

                string downloadDriverText = LocalizationService.Instance.GetString("Dialog_DownloadDriver");
                string cancelText = LocalizationService.Instance.GetString("Dialog_Cancel");

                var result = await ShowDialogAsync(pipelineTitle, panel, downloadDriverText, cancelText);
                if (result == DialogButtonResult.Primary)
                {
                    VirtualAudioDriverService.OpenDriverDownloadPage();
                }
            }
        }

        private static Border CreatePipelineDeviceCard(string title, string deviceName, string iconGlyph)
        {
            var border = new Border
            {
                Background = (System.Windows.Media.Brush)Application.Current.Resources["DialogCardBackgroundBrush"],
                BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["DialogCardBorderBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10)
            };

            var stack = new StackPanel();

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
            headerStack.Children.Add(new TextBlock
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                Text = iconGlyph,
                FontSize = 13,
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            headerStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 12,
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
                VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(headerStack);

            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrEmpty(deviceName) ? LocalizationService.Instance.GetString("Dialog_Pipeline_NotFound") : deviceName,
                FontSize = 13,
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimaryBrush"],
                Margin = new Thickness(0, 4, 0, 0)
            });

            border.Child = stack;
            return border;
        }

        private void OnDriverActionButton_Click(object sender, RoutedEventArgs e)
        {
            VirtualAudioDriverService.OpenDriverDownloadPage();
        }

        private async void OnPlayAudio(object sender, RoutedEventArgs e)
        {
            string noticeTitle = LocalizationService.Instance.GetString("Dialog_Notice");
            string okText = LocalizationService.Instance.GetString("Dialog_OK");

            if (string.IsNullOrEmpty(viewModel.SelectedFilePath) || !File.Exists(viewModel.SelectedFilePath))
            {
                await ShowDialogAsync(noticeTitle, LocalizationService.Instance.GetString("Dialog_NoAudioSelected"), null, okText);
                return;
            }

            var pipeline = VirtualAudioDriverService.DetectPipeline();
            if (!pipeline.IsInstalled || pipeline.OutputDeviceID < 0)
            {
                string notReadyTitle = LocalizationService.Instance.GetString("Dialog_DriverNotReady_Title");
                string notReadyMsg = LocalizationService.Instance.GetString("Dialog_DriverNotReady_Msg");
                string goInstallText = LocalizationService.Instance.GetString("Dialog_GoInstallDriver");
                string cancelText = LocalizationService.Instance.GetString("Dialog_Cancel");

                var result = await ShowDialogAsync(notReadyTitle, notReadyMsg, goInstallText, cancelText);
                if (result == DialogButtonResult.Primary)
                {
                    VirtualAudioDriverService.OpenDriverDownloadPage();
                }
                return;
            }

            if (ListAllWindows.chatProcess == null || ListAllWindows.chatProcess.HasExited || ListAllWindows.chatProcess.MainWindowHandle == IntPtr.Zero)
            {
                await ShowDialogAsync(noticeTitle, LocalizationService.Instance.GetString("Dialog_NoWindowBound"), null, okText);
                return;
            }

            try
            {
                WindowInterop.SetForegroundWindow(ListAllWindows.chatProcess.MainWindowHandle);
                await Task.Delay(200);

                var audioReader = AudioDecoderFactory.CreateAudioReader(viewModel.SelectedFilePath);
                var nAudioServices = new NAudioServices(audioReader, pipeline.OutputDeviceID, thresholdSlider.Value);
                await nAudioServices.PlayAudioAsync();
            }
            catch (Exception ex)
            {
                string errTitle = LocalizationService.Instance.GetString("Dialog_PlaybackError_Title");
                string errMsg = LocalizationService.Instance.GetString("Dialog_PlaybackError_Msg", ex.Message);
                await ShowDialogAsync(errTitle, errMsg, null, okText);
            }
        }

        private void GetQQWindow(object sender, RoutedEventArgs e)
        {
            var listAllWindows = new ListAllWindows();
            listAllWindows.ShowDialog();
        }

        private async void OnCaptureScreen(object sender, RoutedEventArgs e)
        {
            var captureWindow = new CaptureWindow();
            bool? dialogResult = captureWindow.ShowDialog();

            if (dialogResult == true && captureWindow.CapturedRect.HasValue)
            {
                Rectangle value = captureWindow.CapturedRect.Value;
                if (value.Width > 5 && value.Height > 5)
                {
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesDir);
                    string savePath = Path.Combine(imagesDir, "MicButton.png");

                    using (Bitmap bitmap = new Bitmap(value.Width, value.Height))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.CopyFromScreen(value.Left, value.Top, 0, 0, bitmap.Size);
                        }
                        bitmap.Save(savePath, ImageFormat.Png);
                    }

                    // 弹出 Fluent UI 风格的结果提示框
                    var panel = new StackPanel { Width = 350, Margin = new Thickness(0, 4, 0, 0) };

                    // 截图详情展示卡片
                    var previewBorder = new Border
                    {
                        Background = (System.Windows.Media.Brush)Application.Current.Resources["DialogCardBackgroundBrush"],
                        BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["DialogCardBorderBrush"],
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(12)
                    };
                    var previewGrid = new Grid();
                    previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
                    previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
                    previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    try
                    {
                        var bmpImg = new BitmapImage();
                        bmpImg.BeginInit();
                        bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                        bmpImg.UriSource = new Uri(savePath, UriKind.Absolute);
                        bmpImg.EndInit();
                        bmpImg.Freeze();

                        var imgControl = new Image
                        {
                            Source = bmpImg,
                            Stretch = Stretch.Uniform,
                            Width = 56,
                            Height = 56
                        };
                        previewGrid.Children.Add(imgControl);
                    }
                    catch { }

                    var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    Grid.SetColumn(infoStack, 2);
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = LocalizationService.Instance.GetString("Dialog_Capture_Saved"),
                        FontSize = 13,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimaryBrush"]
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = LocalizationService.Instance.GetString("Dialog_Capture_Dimensions", value.Width, value.Height),
                        FontSize = 12,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    previewGrid.Children.Add(infoStack);
                    previewBorder.Child = previewGrid;
                    panel.Children.Add(previewBorder);

                    string captureTitle = LocalizationService.Instance.GetString("Dialog_Capture_Title");
                    string okText = LocalizationService.Instance.GetString("Dialog_OK");
                    await ShowDialogAsync(captureTitle, panel, null, okText);
                }
            }
        }
    }
}
