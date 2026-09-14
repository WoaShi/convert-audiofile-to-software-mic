using System.Collections.ObjectModel;
using NAudio.Wave;

namespace AudioToMicWPF.Services
{
    public class FindDevicesServices
    {
        public ObservableCollection<Devices> outputDevices { get; set; } = new();

        public FindDevicesServices()
        {
            RefreshDevices();
        }

        public void RefreshDevices()
        {
            outputDevices.Clear();
            var pipeline = VirtualAudioDriverService.DetectPipeline();
            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                outputDevices.Add(new Devices
                {
                    DeviceID = n,
                    DeviceName = caps.ProductName,
                    IsVirtual = pipeline.IsInstalled && pipeline.OutputDeviceID == n
                });
            }
        }
    }

    public class Devices
    {
        public int DeviceID { get; set; }
        public string? DeviceName { get; set; }
        public bool IsVirtual { get; set; }

        public override string ToString() => DeviceName ?? string.Empty;
    }
}
