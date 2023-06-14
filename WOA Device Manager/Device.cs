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
            ANDROID, ANDROID_ADB_ENABLED, WINDOWS, BOOTLOADER, FASTBOOT
        }

        public Device() { }

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceStateType DeviceState { get; set; }
        public DeviceInformation DeviceInformation { get; set; }
    }
}
