# convert-audiofile-to-software-mic (音频文件转软件语音)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D4?style=flat-square&logo=windows)
![UI](https://img.shields.io/badge/UI-Fluent%20(Mica)-005FB8?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

一个轻量、现代化的 Windows 桌面工具，用于将本地各类音频文件转为聊天软件（如 QQ、微信等）的麦克风语音输入。软件通过虚拟音频链路进行音频直通推流，并自动在屏幕上识别匹配语音按钮并模拟按住发送。

---

## ✨ 核心特性

- 🎨 **原生 Fluent 现代设计**：采用 Windows 11 Fluent 设计规范与 Mica（云母）半透明材质背景，交互与视觉现代精致。
- 🔗 **虚拟声卡直通链路**：自动探测与管理系统虚拟声卡通道（支持 VB-Audio CABLE、VoiceMeeter 等），支持一键检测输出端与输入端的连接状态。
- 🎵 **丰富音频格式支持**：内置多解码管线，全面支持 `MP3`、`WAV`、`FLAC`、`OGG (Vorbis)`、`M4A`、`AAC` 等常见格式。
- ⚡ **高性能纯 C# 模板匹配**：采用基于 2D 积分图的高性能归一化互相关（NCC）图像匹配算法，**0 原生 C++/OpenCV 依赖**，运行轻快低内存。
- ✂️ **交互式 Fluent 截图工具**：内置框选截图工具，支持拖拽框选、即时重选、ESC 退出与特征预览，轻松录入语音按钮样本。

---

## 🛠️ 准备工作

软件依赖虚拟音频驱动（将电脑播放的声音重定向为麦克风输入信号）：

1. **安装虚拟音频驱动（推荐 VB-CABLE）**：
   - 首次启动软件时，若未检测到驱动，可在主界面点击 **【安装驱动】** 前往官方网站免费下载安装（通过微软 WHQL 官方认证，无需重启系统）。
2. **配置聊天软件麦克风**：
   - 打开聊天软件（如 QQ、微信）的“设置 -> 音视频通话 / 声音”。
   - 将聊天软件的 **麦克风输入设备** 修改为虚拟声卡对应的录音端（例如 `CABLE Output` 或 `VoiceMeeter Output`）。

---

## 🚀 使用步骤

1. **绑定聊天窗口**：
   - 打开准备发送语音的聊天对话框，展开“按住说话 / 发送语音”界面。
   - 在本软件中点击 **【选择软件窗口】**，在列表中选中目标聊天窗口并确认。
2. **采样语音按钮**：
   - 点击 **【截图语音按钮】**，在屏幕上拖拽框选聊天窗口中的语音按钮并点击保存。
   - 样本将自动保存并即时生效，后续若按钮外观未发生变化无需重复截图。
3. **选择待发送音频**：
   - 点击 **【选择音频文件】**，选中要发送的音频文件。
4. **一键播放与发送**：
   - 点击 **【播放到软件语音】**。
   - 软件将自动将目标聊天窗口激活至前台、精准定位语音按钮并在播放期间持续模拟按住，音频播放完毕后自动释放完成发送。

---

## 💡 常见问题与提示

* **聊天软件接收到的语音无声或有电流杂音？**
  * 请在 Windows 系统声音设置中，将虚拟声卡输入端与输出端的格式统一配置为 `48000Hz (DVD 音质)` 或 `44100Hz (CD 音质)` 16-bit / 24-bit。
* **屏幕未找到语音按钮？**
  * 请确保目标聊天窗口已展开语音输入面板，且未被其他不透明窗口遮挡；
  * 可适当降低界面中的“图像识别置信度”滑块（推荐 60% ~ 75% 之间）；
  * 如聊天软件更新了界面主题或切换了高分屏缩放比例，建议点击【截图语音按钮】重新截取一次。

---

## 📦 技术栈与依赖

- **运行时环境**：.NET 10.0 (`net10.0-windows`)
- **界面与主题**：WPF / [iNKORE.UI.WPF.Modern](https://github.com/iNKORE-NET/UI.WPF.Modern)
- **音频流处理**：[NAudio](https://github.com/naudio/NAudio) / [BunLabs.NAudio.Flac](https://github.com/BunLabs/NAudio.Flac) / [NVorbis](https://github.com/NVorbis/NVorbis)

---

## 📄 开源许可证

本项目基于 [MIT License](LICENSE) 协议开源。
特别感谢 ETO-QSH 及开源社区各位贡献者的建议与支持。
