using FastBoot;
using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Managers;

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

        private string GetDeviceIdentityString()
        {
            string deviceIdentityString = "N/A";
            if (device.IsADBCompatible && device.AndroidDebugBridgeTransport != null)
            {
                deviceIdentityString = device.AndroidDebugBridgeTransport.Shell("getprop");
            }
            else if (device.IsFastBootCompatible && device.FastBootTransport != null)
            {
                if (!device.FastBootTransport.GetAllVariables(out deviceIdentityString))
                {
                    deviceIdentityString = "N/A";
                }
            }

            return deviceIdentityString;
        }

        private string DeviceIdentityString => device != null ? GetDeviceIdentityString() : "Unknown";

        private string BatteryLevelFormatted => device != null && device.BatteryLevel != null ? $"Battery level: {device.BatteryLevel}%" : "Battery level: Unknown";

        private bool IsDeviceConnected => device != null && device.IsConnected;

        private bool IsDeviceDisconnected => !IsDeviceConnected;
    }
}
