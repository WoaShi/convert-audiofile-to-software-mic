using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioToMicWPF.Services;
using iNKORE.UI.WPF.Modern.Controls;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace AudioToMicWPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string? _selectedFilePath;
        private string? _programName;
        private string _driverStatusDescription = "驱动状态: 未安装";
        private string _driverActionButtonText = "安装驱动";

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class MainWindow : Window
    {
        public MainViewModel viewModel = new MainViewModel();
        private readonly FindDevicesServices findDevicesServices;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            findDevicesServices = new FindDevicesServices();
            RefreshDriverState();
        }

        private void RefreshDriverState()
        {
            findDevicesServices.RefreshDevices();
            var virtualPipelineInfo = VirtualAudioDriverService.DetectPipeline();
            viewModel.IsDriverInstalled = virtualPipelineInfo.IsInstalled;
            viewModel.DriverStatusDescription = virtualPipelineInfo.IsInstalled ? "驱动状态: 已安装" : "驱动状态: 未安装";
            viewModel.DriverActionButtonText = virtualPipelineInfo.IsInstalled ? "官网页面" : "安装驱动";
        }

        private void OnPickFile(object sender, RoutedEventArgs e)
        {
            string text = new FilePickerServices().OpenFilePicker();
            if (text != "未选择音频文件！")
            {
                viewModel.SelectedFilePath = text;
            }
        }

        private void OnRefreshDevices(object sender, RoutedEventArgs e)
        {
            RefreshDriverState();
        }

        private async void OnViewPipeline_Click(object sender, RoutedEventArgs e)
        {
            var virtualPipelineInfo = VirtualAudioDriverService.DetectPipeline();
            if (virtualPipelineInfo.IsInstalled)
            {
                var panel = new StackPanel { Width = 360, Margin = new Thickness(0, 4, 0, 0) };

                // 播放输出端节点
                panel.Children.Add(CreatePipelineDeviceCard(
                    "播放输出端",
                    virtualPipelineInfo.OutputDeviceName,
                    "\uE767"));

                // 连接指示
                var arrowStack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 6, 0, 6)
                };
                arrowStack.Children.Add(new FontIcon
                {
                    Glyph = "\uE74B",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                panel.Children.Add(arrowStack);

                // 录音输入端节点
                panel.Children.Add(CreatePipelineDeviceCard(
                    "录音输入端",
                    virtualPipelineInfo.InputDeviceName,
                    "\uE720"));

                var dialog = new ContentDialog
                {
                    Title = "链路",
                    Content = panel,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                };

                await dialog.ShowAsync();
            }
            else
            {
                var panel = new StackPanel { Width = 360, Margin = new Thickness(0, 4, 0, 0) };

                panel.Children.Add(new TextBlock
                {
                    Text = "未检测到虚拟音频驱动。",
                    FontSize = 13,
                    Margin = new Thickness(0, 4, 0, 10)
                });

                var dialog = new ContentDialog
                {
                    Title = "链路",
                    Content = panel,
                    PrimaryButtonText = "下载驱动",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    VirtualAudioDriverService.OpenDriverDownloadPage();
                }
            }
        }

        private static Border CreatePipelineDeviceCard(string title, string deviceName, string iconGlyph)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(16, 128, 128, 128)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10)
            };

            var stack = new StackPanel();

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
            headerStack.Children.Add(new FontIcon
            {
                Glyph = iconGlyph,
                FontSize = 13,
                Foreground = Brushes.DodgerBlue,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            headerStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(headerStack);

            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrEmpty(deviceName) ? "（未找到设备）" : deviceName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
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
            if (string.IsNullOrEmpty(viewModel.SelectedFilePath) || !File.Exists(viewModel.SelectedFilePath))
            {
                var dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "尚未选择音频文件，请先点击【选择音频文件】进行选择！",
                    CloseButtonText = "确定",
                    DefaultButton = ContentDialogButton.Close
                };
                await dialog.ShowAsync();
                return;
            }

            var pipeline = VirtualAudioDriverService.DetectPipeline();
            if (!pipeline.IsInstalled || pipeline.OutputDeviceID < 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "声卡未就绪",
                    Content = "未检测到虚拟声卡设备，请先点击【安装驱动】完成声卡驱动配置！",
                    PrimaryButtonText = "前往安装驱动",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    VirtualAudioDriverService.OpenDriverDownloadPage();
                }
                return;
            }

            if (ListAllWindows.chatProcess == null || ListAllWindows.chatProcess.HasExited || ListAllWindows.chatProcess.MainWindowHandle == IntPtr.Zero)
            {
                var dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "请先点击【选择软件窗口】绑定目标聊天软件窗口！",
                    CloseButtonText = "确定",
                    DefaultButton = ContentDialogButton.Close
                };
                await dialog.ShowAsync();
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
                var dialog = new ContentDialog
                {
                    Title = "播放出错",
                    Content = $"播放音频时发生错误：\n{ex.Message}",
                    CloseButtonText = "确定",
                    DefaultButton = ContentDialogButton.Close
                };
                await dialog.ShowAsync();
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
                        Background = new SolidColorBrush(Color.FromArgb(16, 128, 128, 128)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)),
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
                        Text = "截图已保存",
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 13
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = $"尺寸: {value.Width} × {value.Height} 像素",
                        FontSize = 12,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    previewGrid.Children.Add(infoStack);
                    previewBorder.Child = previewGrid;
                    panel.Children.Add(previewBorder);

                    var dialog = new ContentDialog
                    {
                        Title = "截图",
                        Content = panel,
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    await dialog.ShowAsync();
                }
            }
        }
    }
}
