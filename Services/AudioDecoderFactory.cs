using System;
using System.IO;
using NAudio.Flac;
using NAudio.Wave;

namespace AudioToMicWPF.Services
{
    public static class AudioDecoderFactory
    {
        public static WaveStream CreateAudioReader(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".mp3" => new Mp3FileReader(filePath),
                ".wav" => new WaveFileReader(filePath),
                ".flac" => new FlacReader(filePath),
                ".ogg" => new VorbisWaveReader(filePath),
                ".m4a" or ".aac" or ".wma" => new MediaFoundationReader(filePath),
                _ => new MediaFoundationReader(filePath) // 尝试使用 Windows Media Foundation 解码器
            };
        }
    }
}
