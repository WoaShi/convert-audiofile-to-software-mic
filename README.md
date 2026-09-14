# convert-audiofile-to-software-mic

将音频文件转为聊天软件的麦克风语音输入（支持 QQNT、微信等通过按住按钮发送语音的聊天软件）。

## 重要说明

- **软件需求**：依赖虚拟音频驱动（如 VB-CABLE 或 VoiceMeeter）。软件会自动检测并绑定系统中的虚拟音频设备。
- **兼容性**：支持 Windows 10 / Windows 11，支持各类通过按住按钮发送语音的电脑端聊天软件。

## 使用步骤

1. 打开准备发送语音的聊天窗口，并展开语音输入界面。关于窗口名称，例如群聊窗口在软件中一般显示为主程序名称（如“QQ”）。

   ![说明](https://github.com/user-attachments/assets/6bf3b043-83d6-46e1-8680-268ee03fd013)

2. 打开软件本体。若系统未安装虚拟音频驱动，可点击“安装驱动”前往官网下载（推荐 VB-CABLE，安装后点击刷新即可）。点击“查看音频链路”可查看当前输出与输入设备的对应关系。

3. 点击“选择音频文件”，选择需要播放的音频文件（支持 mp3、wav、flac、ogg、m4a、aac 等格式）。选择成功后，输入框会显示对应文件路径。

   ![a18c4333-3249-4e5d-97b1-f4e755e946ae](https://github.com/user-attachments/assets/7abb1533-430b-471a-99d1-c1778f589456)

4. 点击“选择软件窗口”，在窗口列表中选中目标聊天窗口（如“QQ”），点击确认。

   ![bf7ff5ec-5659-495a-8572-04c2191f2ea9](https://github.com/user-attachments/assets/d0dc655d-e2bc-4635-9e82-8126c5164f7d)

   - **注**：若目标窗口未出现在列表中，请先让聊天窗口显示在屏幕上，点击“刷新”后再次寻找。

5. 确保聊天软件的麦克风输入已设置为对应的虚拟声卡通道。例如：
   - 使用 VB-CABLE 时：本软件输出至 `CABLE Input`，聊天软件麦克风应设置为 `CABLE Output`。
   - 使用 VoiceMeeter 时：输入端与输出端通道需相匹配。

   ![未标题-2](https://github.com/user-attachments/assets/649c6b71-0061-4873-9808-bb4d91201af4)

   - **注**：请在 Windows 系统设置或聊天软件内部设置中，将麦克风指定为虚拟声卡的 Output 端。
   ![说明](https://github.com/user-attachments/assets/ec894435-003d-4b62-bcec-ec295b348f14)

6. 点击“截图语音按钮”，框选聊天窗口中的语音按钮进行裁截。

   ![5c644045-93eb-4f73-a9b4-addb09fdbb22](https://github.com/user-attachments/assets/c8c1fbb5-4053-41c9-a418-ec995f0f40bf)

   - 可通过拖拽或两次点击确定选区。右键或点击“重选”可重新框选，按 ESC 键可退出。
   - 确认框选后点击“保存截图”，图片将保存到软件同目录的 `Images/MicButton.png`。

7. 完成上述设置后，点击“播放到软件语音”即可开始转录。软件会自动激活目标窗口、识别语音按钮并模拟按住发送。

## 注意事项

- 若点击播放后发现聊天软件录入的声音无声，建议在 Windows 系统声音控制面板中，将虚拟声卡输入和输出的采样率统一设置为 `44100Hz` 或 `48000Hz`。
- 若播放时鼠标未准确停留在语音按钮上，可适当调节“图像识别置信度”滑块（默认 70%）；如更换了窗口主题或系统缩放比例，建议重新截图一次。

## 鸣谢与依赖

- 界面组件：[iNKORE.UI.WPF.Modern](https://github.com/iNKORE-NET/UI.WPF.Modern)
- 音频处理：[NAudio](https://github.com/naudio/NAudio) / [BunLabs.NAudio.Flac](https://github.com/BunLabs/NAudio.Flac) / [NVorbis](https://github.com/NVorbis/NVorbis)
- 特别感谢 ETO-QSH 对该软件的建议与修正。

## 开源协议

本项目采用 [MIT License](LICENSE) 协议。
