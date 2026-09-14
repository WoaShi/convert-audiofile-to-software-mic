using System;
using System.Diagnostics;
using NAudio.Wave;

namespace AudioToMicWPF.Services
{
    public class VirtualPipelineInfo
    {
        public bool IsInstalled { get; set; }
        public int OutputDeviceID { get; set; } = -1;
        public string OutputDeviceName { get; set; } = string.Empty;
        public string InputDeviceName { get; set; } = string.Empty;
        public string PipelineText { get; set; } = string.Empty;
    }

    public class VirtualAudioDriverService
    {

        /// <summary>
        /// 精准检测系统中已安装的虚拟音频驱动管线
        /// </summary>
        public static VirtualPipelineInfo DetectPipeline()
        {
            // 1. 优先匹配具有微软 WHQL 官方认证的 VB-CABLE
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                string name = caps.ProductName;

                if (IsExcludedDevice(name)) continue;

                if (name.IndexOf("CABLE Input", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("VB-Audio Point", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new VirtualPipelineInfo
                    {
                        IsInstalled = true,
                        OutputDeviceID = i,
                        OutputDeviceName = name,
                        InputDeviceName = "CABLE Output",
                        PipelineText = "CABLE Input -> CABLE Output"
                    };
                }
            }

            // 2. 兼容匹配 VoiceMeeter
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                string name = caps.ProductName;

                if (IsExcludedDevice(name)) continue;

                if (name.IndexOf("VoiceMeeter", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string inputName = name.Contains("VAIO3", StringComparison.OrdinalIgnoreCase) ? "VoiceMeeter VAIO3 Output" :
                                      name.Contains("Aux", StringComparison.OrdinalIgnoreCase) ? "VoiceMeeter Aux Output" : "VoiceMeeter Output";
                    return new VirtualPipelineInfo
                    {
                        IsInstalled = true,
                        OutputDeviceID = i,
                        OutputDeviceName = name,
                        InputDeviceName = inputName,
                        PipelineText = $"{name} -> {inputName}"
                    };
                }
            }

            return new VirtualPipelineInfo
            {
                IsInstalled = false,
                OutputDeviceID = -1,
                OutputDeviceName = string.Empty,
                InputDeviceName = string.Empty,
                PipelineText = string.Empty
            };
        }

        private static bool IsExcludedDevice(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName)) return true;
            string lower = deviceName.ToLowerInvariant();
            return lower.Contains("nvidia") ||
                   lower.Contains("steam") ||
                   lower.Contains("realtek") ||
                   lower.Contains("intel") ||
                   lower.Contains("jbl");
        }

        public static bool IsDriverInstalled()
        {
            return DetectPipeline().IsInstalled;
        }

        public const string VbCableDownloadUrl = "https://vb-audio.com/Cable/index.htm";

        public static void OpenDriverDownloadPage()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = VbCableDownloadUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开驱动下载页面失败: {ex.Message}");
            }
        }

        public static void OpenAppVolumePreferences()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:apps-volume",
                    UseShellExecute = true
                });
            }
            catch
            {
                OpenClassicSoundControlPanel();
            }
        }

        public static void OpenClassicSoundControlPanel()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "control.exe",
                    Arguments = "mmsys.cpl,,1",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开声音控制面板失败: {ex.Message}");
            }
        }
    }
}
