using System.Windows;
using AudioToMicWPF.Services;

namespace AudioToMicWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化多语言（默认为系统语言，支持中英）
            LocalizationService.Instance.SetLanguage(LocalizationService.Instance.CurrentLanguage);

            // 初始化主题（自动检测系统是否为暗色模式并应用对应画刷）
            ThemeService.Instance.ApplyTheme(ThemeService.Instance.IsDark, false);
        }
    }
}
