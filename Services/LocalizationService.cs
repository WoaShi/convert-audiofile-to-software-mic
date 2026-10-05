using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace AudioToMicWPF.Services
{
    public enum AppLanguage
    {
        Chinese,
        English
    }

    public class LocalizationService
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        public AppLanguage CurrentLanguage { get; private set; }

        public event Action<AppLanguage>? LanguageChanged;

        private readonly Dictionary<string, string> _zh = new()
        {
            // 窗口与通用
            ["App_Title"] = "音频文件转软件语音",
            ["Top_SelectAudio"] = "选择音频文件",
            ["Top_SelectWindow"] = "选择软件窗口",

            // 卡片
            ["Card_AudioSelection_Title"] = "音频文件选择",
            ["Card_AudioSelection_Desc"] = "点击选择将要使用的音频文件",
            ["Card_AudioSelection_Header"] = "音频路径",
            ["Card_WindowSelection_Title"] = "程序窗口选择",
            ["Card_WindowSelection_Desc"] = "点击选择目标软件聊天窗口进行绑定",
            ["Card_WindowSelection_Header"] = "程序窗口",
            ["Card_Driver_Title"] = "虚拟声卡设置",
            ["Driver_Status_Installed"] = "驱动状态: 已安装",
            ["Driver_Status_NotInstalled"] = "驱动状态: 未安装",
            ["Driver_Action_Install"] = "安装驱动",
            ["Driver_Action_Website"] = "官网页面",
            ["Driver_View_Pipeline"] = "查看音频链路",
            ["Driver_Tooltip_Pipeline"] = "查看链路",
            ["Driver_Tooltip_Refresh"] = "刷新状态",

            // 滑块与底部按钮
            ["Slider_Confidence"] = "图像识别置信度",
            ["Btn_Capture"] = "截图语音按钮",
            ["Btn_Play"] = "播放到软件语音",

            // 顶部工具按钮
            ["Header_Theme_ToDark"] = "切换至深色模式",
            ["Header_Theme_ToLight"] = "切换至浅色模式",
            ["Header_Language"] = "切换语言 (Switch Language)",
            ["Header_Language_BtnText"] = "EN",

            // 对话框与提示
            ["Dialog_Notice"] = "提示",
            ["Dialog_Error"] = "错误",
            ["Dialog_OK"] = "确定",
            ["Dialog_Cancel"] = "取消",
            ["Dialog_DownloadDriver"] = "下载驱动",
            ["Dialog_NoAudioSelected"] = "尚未选择音频文件，请先点击【选择音频文件】进行选择！",
            ["Dialog_DriverNotReady_Title"] = "声卡未就绪",
            ["Dialog_DriverNotReady_Msg"] = "未检测到虚拟声卡设备，请先点击【安装驱动】完成声卡驱动配置！",
            ["Dialog_GoInstallDriver"] = "前往安装驱动",
            ["Dialog_NoWindowBound"] = "请先点击【选择软件窗口】绑定目标聊天软件窗口！",
            ["Dialog_PlaybackError_Title"] = "播放出错",
            ["Dialog_PlaybackError_Msg"] = "播放音频时发生错误：\n{0}",
            ["Dialog_Pipeline_Title"] = "链路",
            ["Dialog_Pipeline_PlaybackOutput"] = "播放输出端",
            ["Dialog_Pipeline_RecordInput"] = "录音输入端",
            ["Dialog_Pipeline_NotFound"] = "（未找到设备）",
            ["Dialog_Pipeline_DriverNotInstalled"] = "未检测到虚拟音频驱动。",
            ["Dialog_Capture_Title"] = "截图",
            ["Dialog_Capture_Saved"] = "截图已保存",
            ["Dialog_Capture_Dimensions"] = "尺寸: {0} × {1} 像素",
            ["FilePicker_FilterTitle"] = "选择目标音频",
            ["FilePicker_NoFile"] = "未选择音频文件！",

            // 列表窗口
            ["ListWindow_Title"] = "选择聊天窗口",
            ["ListWindow_Confirm"] = "确定该窗口为软件聊天窗口",
            ["ListWindow_Refresh"] = "刷新",

            // 截图窗口
            ["Capture_Instruction"] = "拖拽或点击选定语音按钮区域 (右键重选 · ESC 退出)",
            ["Capture_Save"] = "保存截图",
            ["Capture_Reselect"] = "重选",
            ["Capture_Cancel"] = "取消",

            // 定位按钮
            ["Locate_MissingImage"] = "未找到语音按钮特征图片：{0}\n请先点击“截图语音按钮”进行裁截！",
            ["Locate_LoadError"] = "加载语音按钮图片失败：{0}",
            ["Locate_NoMatch"] = "未在屏幕上匹配到语音按钮！(当前最高相似度: {0:P0}，设定阈值: {1:P0})\n请确认聊天窗口已处于前台且语音面板处于展开状态，或适当降低置信度阈值。",
            ["Locate_NoticeTitle"] = "匹配提示"
        };

        private readonly Dictionary<string, string> _en = new()
        {
            // 窗口与通用
            ["App_Title"] = "Audio File to Software Mic",
            ["Top_SelectAudio"] = "Select Audio",
            ["Top_SelectWindow"] = "Select Window",

            // 卡片
            ["Card_AudioSelection_Title"] = "Audio File Selection",
            ["Card_AudioSelection_Desc"] = "Click to select the audio file to use",
            ["Card_AudioSelection_Header"] = "Audio Path",
            ["Card_WindowSelection_Title"] = "Target Window Selection",
            ["Card_WindowSelection_Desc"] = "Click to select and bind target chat window",
            ["Card_WindowSelection_Header"] = "Target Window",
            ["Card_Driver_Title"] = "Virtual Audio Driver",
            ["Driver_Status_Installed"] = "Driver Status: Installed",
            ["Driver_Status_NotInstalled"] = "Driver Status: Not Installed",
            ["Driver_Action_Install"] = "Install Driver",
            ["Driver_Action_Website"] = "Official Site",
            ["Driver_View_Pipeline"] = "Audio Pipeline",
            ["Driver_Tooltip_Pipeline"] = "View Pipeline",
            ["Driver_Tooltip_Refresh"] = "Refresh Status",

            // 滑块与底部按钮
            ["Slider_Confidence"] = "Recognition Confidence",
            ["Btn_Capture"] = "Capture Voice Button",
            ["Btn_Play"] = "Play to Software Voice",

            // 顶部工具按钮
            ["Header_Theme_ToDark"] = "Switch to Dark Mode",
            ["Header_Theme_ToLight"] = "Switch to Light Mode",
            ["Header_Language"] = "Switch Language (切换语言)",
            ["Header_Language_BtnText"] = "中",

            // 对话框与提示
            ["Dialog_Notice"] = "Notice",
            ["Dialog_Error"] = "Error",
            ["Dialog_OK"] = "OK",
            ["Dialog_Cancel"] = "Cancel",
            ["Dialog_DownloadDriver"] = "Download Driver",
            ["Dialog_NoAudioSelected"] = "No audio file selected. Please click [Select Audio] first!",
            ["Dialog_DriverNotReady_Title"] = "Audio Driver Not Ready",
            ["Dialog_DriverNotReady_Msg"] = "No virtual sound card detected. Please install driver first!",
            ["Dialog_GoInstallDriver"] = "Go to Install Driver",
            ["Dialog_NoWindowBound"] = "Please click [Select Window] to bind the target window first!",
            ["Dialog_PlaybackError_Title"] = "Playback Error",
            ["Dialog_PlaybackError_Msg"] = "An error occurred while playing audio:\n{0}",
            ["Dialog_Pipeline_Title"] = "Audio Pipeline",
            ["Dialog_Pipeline_PlaybackOutput"] = "Playback Output",
            ["Dialog_Pipeline_RecordInput"] = "Recording Input",
            ["Dialog_Pipeline_NotFound"] = "(Device not found)",
            ["Dialog_Pipeline_DriverNotInstalled"] = "No virtual audio driver detected.",
            ["Dialog_Capture_Title"] = "Screenshot",
            ["Dialog_Capture_Saved"] = "Screenshot Saved",
            ["Dialog_Capture_Dimensions"] = "Size: {0} × {1} pixels",
            ["FilePicker_FilterTitle"] = "Select Target Audio",
            ["FilePicker_NoFile"] = "No audio file selected!",

            // 列表窗口
            ["ListWindow_Title"] = "Select Chat Window",
            ["ListWindow_Confirm"] = "Confirm as Chat Target Window",
            ["ListWindow_Refresh"] = "Refresh",

            // 截图窗口
            ["Capture_Instruction"] = "Drag or click to select voice button area (Right-click to reselect · ESC to cancel)",
            ["Capture_Save"] = "Save Screenshot",
            ["Capture_Reselect"] = "Reselect",
            ["Capture_Cancel"] = "Cancel",

            // 定位按钮
            ["Locate_MissingImage"] = "Voice button template not found: {0}\nPlease click 'Capture Voice Button' first!",
            ["Locate_LoadError"] = "Failed to load voice button image: {0}",
            ["Locate_NoMatch"] = "Voice button not matched on screen! (Top confidence: {0:P0}, Threshold: {1:P0})\nPlease ensure the chat window is active and voice panel is open, or lower the confidence threshold.",
            ["Locate_NoticeTitle"] = "Match Notice"
        };

        public LocalizationService()
        {
            // 自动检测系统语言，默认中文
            string culture = CultureInfo.CurrentUICulture.Name;
            CurrentLanguage = culture.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? AppLanguage.Chinese : AppLanguage.English;
        }

        public void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == AppLanguage.Chinese ? AppLanguage.English : AppLanguage.Chinese);
        }

        public void SetLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            var dict = language == AppLanguage.Chinese ? _zh : _en;
            var res = Application.Current.Resources;

            foreach (var kv in dict)
            {
                res[kv.Key] = kv.Value;
            }

            LanguageChanged?.Invoke(language);
        }

        public string GetString(string key, params object[] args)
        {
            var dict = CurrentLanguage == AppLanguage.Chinese ? _zh : _en;
            if (dict.TryGetValue(key, out var val))
            {
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(val, args);
                    }
                    catch
                    {
                        return val;
                    }
                }
                return val;
            }
            return key;
        }
    }
}
