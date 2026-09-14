using System;
using System.IO;
using NAudio.Wave;
using NVorbis;

namespace AudioToMicWPF.Services
{
    public class VorbisWaveReader : WaveStream
    {
        private readonly VorbisReader vorbisReader;
        private readonly WaveFormat waveFormat;
        private readonly Stream sourceStream;
        private readonly long totalSamples;

        public VorbisWaveReader(string filePath)
        {
            sourceStream = File.OpenRead(filePath);
            vorbisReader = new VorbisReader(sourceStream, false);

            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(vorbisReader.SampleRate, vorbisReader.Channels);
            totalSamples = vorbisReader.TotalSamples;
        }

        public override WaveFormat WaveFormat => waveFormat;

        public override long Length => totalSamples * waveFormat.BlockAlign;

        public override long Position
        {
            get => vorbisReader.SamplePosition * waveFormat.BlockAlign;
            set => vorbisReader.SamplePosition = value / waveFormat.BlockAlign;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int samplesRequested = count / sizeof(float);
            float[] sampleBuffer = new float[samplesRequested];

            int samplesRead = vorbisReader.ReadSamples(sampleBuffer, 0, samplesRequested);

            // Convert float samples to 32-bit IEEE float bytes
            int bytesWritten = 0;
            for (int i = 0; i < samplesRead; i++)
            {
                byte[] bytes = BitConverter.GetBytes(sampleBuffer[i]);
                Array.Copy(bytes, 0, buffer, offset + bytesWritten, 4);
                bytesWritten += 4;
            }

            return bytesWritten;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vorbisReader.Dispose();
                sourceStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
