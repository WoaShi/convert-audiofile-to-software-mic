#pragma warning disable WPF0001

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace AudioToMicWPF.Services
{
    public class ThemeService
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        public bool IsDark { get; private set; }
        public bool HasUserOverridden { get; private set; }

        public event Action<bool>? ThemeChanged;

        public ThemeService()
        {
            IsDark = DetectSystemDarkTheme();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
            {
                if (!HasUserOverridden)
                {
                    bool systemIsDark = DetectSystemDarkTheme();
                    if (systemIsDark != IsDark)
                    {
                        ApplyTheme(systemIsDark, false);
                    }
                }
            }
        }

        public static bool DetectSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int intVal)
                {
                    return intVal == 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect system theme: {ex.Message}");
            }
            return false;
        }

        public void ToggleTheme()
        {
            ApplyTheme(!IsDark, true);
        }

        public void ApplyTheme(bool isDark, bool isUserOverride = true)
        {
            IsDark = isDark;
            if (isUserOverride)
            {
                HasUserOverridden = true;
            }

            // 1. 设置 WPF 10 原生 ThemeMode
            try
            {
                Application.Current.ThemeMode = isDark ? ThemeMode.Dark : ThemeMode.Light;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set ThemeMode: {ex.Message}");
            }

            // 2. 注入动态语义画刷到 Application.Current.Resources
            UpdateDynamicResources(isDark);

            // 3. 对当前所有打开的 Window 刷新 DWM 暗色标题栏与材质
            foreach (Window window in Application.Current.Windows)
            {
                ApplyWindowDarkMode(window, isDark);
            }

            ThemeChanged?.Invoke(isDark);
        }

        public void ApplyWindowDarkMode(Window window, bool isDark)
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    int darkMode = isDark ? 1 : 0;
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyWindowDarkMode failed: {ex.Message}");
            }
        }

        private static void UpdateDynamicResources(bool isDark)
        {
            var res = Application.Current.Resources;

            if (isDark)
            {
                // 深色模式画刷
                res["WindowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
                res["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B));
                res["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x3E));
                
                res["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0xA8, 0xA8, 0xA8));
                res["TextTertiaryBrush"] = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75));
                res["TextMediumBrush"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));

                res["ControlBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x24));
                res["ControlBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));

                res["DialogBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
                res["DialogBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                res["DialogOverlayBrush"] = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
                res["DialogCardBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
                res["DialogCardBorderBrush"] = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));

                res["HeaderButtonBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
                res["HeaderButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(0x35, 0xFF, 0xFF, 0xFF));
                res["HeaderButtonBorderBrush"] = new SolidColorBrush(Color.FromArgb(0x25, 0xFF, 0xFF, 0xFF));

                res["CapturePanelBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0xF2, 0x2A, 0x2A, 0x2A));
                res["CapturePanelBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x48, 0x48, 0x48));

                res["MenuBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x2C, 0x2C));
                res["MenuBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                res["MenuItemHoverBrush"] = new SolidColorBrush(Color.FromArgb(0x25, 0xFF, 0xFF, 0xFF));
            }
            else
            {
                // 浅色模式画刷
                res["WindowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3));
                res["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFB, 0xFB, 0xFB));
                res["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));

                res["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x5C, 0x5C, 0x5C));
                res["TextTertiaryBrush"] = new SolidColorBrush(Color.FromRgb(0x8A, 0x8A, 0x8A));
                res["TextMediumBrush"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));

                res["ControlBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                res["ControlBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));

                res["DialogBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA));
                res["DialogBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0));
                res["DialogOverlayBrush"] = new SolidColorBrush(Color.FromArgb(0x55, 0x00, 0x00, 0x00));
                res["DialogCardBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80));
                res["DialogCardBorderBrush"] = new SolidColorBrush(Color.FromArgb(0x28, 0x80, 0x80, 0x80));

                res["HeaderButtonBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0xCC, 0xFA, 0xFA, 0xFA));
                res["HeaderButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(0xE0, 0xEF, 0xEF, 0xEF));
                res["HeaderButtonBorderBrush"] = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0x00));

                res["CapturePanelBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(0xF9, 0xFF, 0xFF, 0xFF));
                res["CapturePanelBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xD6, 0xD6, 0xD6));

                res["MenuBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFC, 0xFC, 0xFC));
                res["MenuBorderBrush"] = new SolidColorBrush(Color.FromRgb(0xE2, 0xE2, 0xE2));
                res["MenuItemHoverBrush"] = new SolidColorBrush(Color.FromArgb(0x12, 0x00, 0x00, 0x00));
            }
        }
    }
}
