using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace AudioToMicWPF.Services
{
    internal class LocateMicButton
    {
        private readonly double _threshold;

        public LocateMicButton(double threshold)
        {
            _threshold = threshold;
        }

        public void MatchAndMoveMouse(string targetImagePath)
        {
            // 处理相对路径与绝对路径
            string resolvedPath = targetImagePath;
            if (!Path.IsPathRooted(resolvedPath))
            {
                resolvedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetImagePath);
            }

            if (!File.Exists(resolvedPath))
            {
                System.Windows.MessageBox.Show($"未找到语音按钮特征图片：{resolvedPath}\n请先点击“截图语音按钮”进行裁截！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Bitmap? targetBmp = null;
            try
            {
                // 使用内存流加载，避免占用文件锁
                byte[] imgBytes = File.ReadAllBytes(resolvedPath);
                using MemoryStream ms = new MemoryStream(imgBytes);
                targetBmp = new Bitmap(ms);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"加载语音按钮图片失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (targetBmp)
            {
                // 获取主屏幕尺寸 (防 null 保护)
                Rectangle screenBounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;

                using (Bitmap screenShot = new Bitmap(screenBounds.Width, screenBounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(screenShot))
                    {
                        g.CopyFromScreen(screenBounds.Left, screenBounds.Top, 0, 0, screenShot.Size);
                    }

                    // 使用轻量高性能纯 C# 模板匹配
                    var matchResult = TemplateMatcher.Match(screenShot, targetBmp, _threshold);

                    if (matchResult.Success)
                    {
                        // 移动鼠标至匹配位置中心 (考虑多屏幕偏移)
                        int targetX = screenBounds.Left + matchResult.Center.X;
                        int targetY = screenBounds.Top + matchResult.Center.Y;
                        Cursor.Position = new Point(targetX, targetY);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            $"未在屏幕上匹配到语音按钮！(当前最高相似度: {matchResult.Score:P0}，设定阈值: {_threshold:P0})\n" +
                            "请确认聊天窗口已处于前台且语音面板处于展开状态，或适当降低置信度阈值。",
                            "匹配提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
