using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
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

            try
            {
                if (!device.JustDisconnected)
                {
                    if (device.IsADBCompatible && device.AndroidDebugBridgeTransport != null)
                    {
                        (string variableName, string variableValue)[] allVariables = device.AndroidDebugBridgeTransport.GetAllVariables();

                        if (allVariables != null)
                        {
                            deviceIdentityString = string.Join("\n", allVariables.Select(t => $"{t.variableName}: {t.variableValue}"));
                        }
                    }
                    else if (device.IsFastBootCompatible && device.FastBootTransport != null)
                    {
                        if (device.FastBootTransport.GetAllVariables(out (string variableName, string variableValue)[] allVariables))
                        {
                            deviceIdentityString = string.Join("\n", allVariables.Select(t => $"{t.variableName}: {t.variableValue}"));
                        }
                    }
                    else if (device.UFPConnected && device.UnifiedFlashingPlatformTransport != null)
                    {
                        string PlatformID = device.UnifiedFlashingPlatformTransport.ReadDevicePlatformID();
                        string ProcessorManufacturer = device.UnifiedFlashingPlatformTransport.ReadProcessorManufacturer();

                        deviceIdentityString = $"Platform ID: {PlatformID}\nProcessor Manufacturer: {ProcessorManufacturer}";
                    }
                }
            }
            catch { }

            return deviceIdentityString;
        }

        private string DeviceIdentityString => device != null ? GetDeviceIdentityString() : "Unknown";

        private string BatteryLevelFormatted => device != null && device.BatteryLevel != null ? $"Battery level: {device.BatteryLevel}%" : "Battery level: Unknown";
    }
}
