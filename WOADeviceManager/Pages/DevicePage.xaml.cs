using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;
using System.Collections.ObjectModel;

namespace WOADeviceManager.Pages
{
    public sealed partial class DevicePage : Page
    {
        private Device device = DeviceManager.Device;

        public DevicePage()
        {
            InitializeComponent();

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                this.device = device;
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                this.device = device;
                Bindings.Update();
            });
        }

        public ObservableCollection<BootDevice> BootDevices => device != null && !device.JustDisconnected ? new(DeviceInfo.GetBootDevices()) : [];

        private string DeviceIdentityString => device != null && !device.JustDisconnected ? DeviceInfo.GetDeviceIdentityString() : "Unknown";
        private string DeviceLogString => device != null && !device.JustDisconnected ? DeviceInfo.GetDeviceLogString() : "Unknown";
        private string FlashInfoString => device != null && !device.JustDisconnected ? DeviceInfo.GetFlashInfoString() : "Unknown";

        private string BatteryLevelFormatted => device != null && !device.JustDisconnected && device.BatteryLevel != null ? $"Battery level: {device.BatteryLevel}%" : "Battery level: Unknown";
    }
}
