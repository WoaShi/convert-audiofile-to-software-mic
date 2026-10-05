# convert-audiofile-to-software-mic

[English](README_EN.md) | [简体中文](README.md)

Convert audio files into microphone input for desktop chat applications (supports QQNT, WeChat, and any application that sends voice messages via hold-to-talk buttons).

---

## Key Highlights & Requirements

- **Runtime Requirement**: Requires [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0). Please download the installer matching your system architecture (usually **x64** for 64-bit systems).
- **Driver Requirement**: Depends on a virtual audio driver (such as [VB-CABLE](https://vb-audio.com/Cable/index.htm) or [VoiceMeeter](https://vb-audio.com/Voicemeeter/)). The software automatically detects and binds available virtual audio devices in the system.
- **Compatibility**: Supports Windows 10 / Windows 11, and any desktop chat application that records voice messages while holding down a button.
- If the newest release causes confusion, please download the 2025-4-26 release.
- **Modern Fluent UI**: Built with native .NET 10 WPF Fluent UI, Windows 11 Mica backdrop effect, automatic system dark mode detection, real-time dark/light theme switching, and English / Simplified Chinese localization.
- **Zero Heavy Native Dependencies**: Uses a custom pure C# template matching algorithm based on Normalized Cross Correlation (NCC) and 2D integral images, avoiding bulky OpenCV binaries.

---

## Step-by-Step Guide

1. **Open the chat window** where you wish to send a voice message and expand the voice input panel. In the process selector, group chat windows typically appear under the main application name (e.g., "QQ").

   ![Guide 1](https://github.com/user-attachments/assets/6bf3b043-83d6-46e1-8680-268ee03fd013)

2. **Launch the application**. If no virtual audio driver is detected, click **"Install Driver"** to visit the official website and download it (VB-CABLE is recommended; after installation, simply click "Refresh"). Click **"Audio Pipeline"** to view the current routing between playback output and recording input.

3. **Click "Select Audio"** to choose the audio file you want to play (supports `.mp3`, `.wav`, `.flac`, `.ogg`, `.m4a`, `.aac`, `.wma`, etc.). Once selected, the file path will appear in the text box.

   ![Guide 2](https://github.com/user-attachments/assets/7abb1533-430b-471a-99d1-c1778f589456)

4. **Click "Select Window"**, choose the target chat window from the process list (e.g., "QQ"), and click confirm.

   ![Guide 3](https://github.com/user-attachments/assets/d0dc655d-e2bc-4635-9e82-8126c5164f7d)

   - *Note*: If the target window does not appear in the list, ensure the chat window is visible on your screen, click "Refresh", and search again.

5. **Configure the chat application's microphone input** to match the virtual audio card channel:
   - When using **VB-CABLE**: This software outputs audio to `CABLE Input`, and the chat application's microphone should be set to `CABLE Output`.
   - When using **VoiceMeeter**: Match the corresponding input and output channels accordingly.

   ![Guide 4](https://github.com/user-attachments/assets/649c6b71-0061-4873-9808-bb4d91201af4)

   - *Note*: Set the microphone in Windows sound settings or within the chat software's internal settings to the Output endpoint of the virtual sound card.

   ![Guide 5](https://github.com/user-attachments/assets/ec894435-003d-4b62-bcec-ec295b348f14)

6. **Click "Capture Voice Button"**, then drag or click on the screen to crop and select the voice button in your chat window.

   ![Guide 6](https://github.com/user-attachments/assets/c8c1fbb5-4053-41c9-a418-ec995f0f40bf)

   - You can define the selection area by dragging or with two clicks. Right-click or press "Reselect" to start over, or press <kbd>ESC</kbd> to exit.
   - Click "Save Screenshot" to confirm. The template image will be saved to `Images/MicButton.png` in the application directory.

7. **Click "Play to Software Voice"** to start the automated playback. The software will automatically activate the target window, locate the voice button, hold down the mouse button to record, play the audio, and release the button to send when finished.

---

## Important Notes & Troubleshooting

- **No Sound Recorded**: If the chat application records silence, ensure in the Windows Sound Control Panel (`mmsys.cpl`) that both the input and output endpoints of the virtual sound card are set to the same sample rate (e.g., `44100Hz` or `48000Hz`).
- **Button Misalignment**: If the mouse does not accurately land on the voice button during playback, adjust the **"Recognition Confidence"** slider (default is 70%). If you change the window theme, display scaling, or resolution, taking a fresh screenshot is recommended.

---

## Acknowledgments & Dependencies

- **UI Framework**: .NET 10 WPF Native Fluent UI & Windows 11 Mica Backdrop
- **Audio Processing**: [NAudio](https://github.com/naudio/NAudio) / [BunLabs.NAudio.Flac](https://github.com/BunLabs/NAudio.Flac) / [NVorbis](https://github.com/NVorbis/NVorbis)
- Special thanks to **ETO-QSH** for valuable advice and corrections to this project.

---

## License

This project is licensed under the [MIT License](LICENSE).
