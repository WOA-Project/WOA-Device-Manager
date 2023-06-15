using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace WOADeviceManager
{
    public class Device
    {
        public enum DeviceStateType
        {
            ANDROID, ANDROID_ADB_ENABLED, WINDOWS, BOOTLOADER, FASTBOOT, DISCONNECTED
        }

        public Device() { }

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceStateType DeviceState { get; set; }
        public DeviceInformation DeviceInformation { get; set; }
        public DeviceInformationUpdate LastDeviceInformationUpdate { get; set; }

        public string DeviceStateLocalized
        {
            get
            {
                switch (DeviceState) {
                    case DeviceStateType.ANDROID:
                        return "Android";
                    case DeviceStateType.ANDROID_ADB_ENABLED:
                        return "Android (ADB Connected)";
                    case DeviceStateType.WINDOWS:
                        return "Windows";
                    case DeviceStateType.BOOTLOADER:
                        return "Bootloader";
                    case DeviceStateType.FASTBOOT:
                        return "Fastboot";
                    case DeviceStateType.DISCONNECTED:
                        return "Disconnected";
                    default:
                        return null;
                }
            }
        }
    }
}
