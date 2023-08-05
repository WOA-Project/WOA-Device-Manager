using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using WOADeviceManager.Helpers;

namespace WOADeviceManager
{
    public class Device
    {
        public enum DeviceState
        {
            ANDROID, ANDROID_ADB_ENABLED, WINDOWS, BOOTLOADER, FASTBOOT, TWRP, DISCONNECTED
        }

        public Device() { }

        public string ID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public DeviceState State { get; set; } = DeviceState.DISCONNECTED;
        public DeviceInformation Information { get; set; }
        public DeviceInformationUpdate LastInformationUpdate { get; set; }

        public string ADBID { get; set; }
        public string FastbootID { get; set; }
        public string TWRPID { get; set; }

        // TODO: These don't work, fix them
        //public string AndroidVersion
        //{
        //    get
        //    {
        //        if (State == DeviceState.ANDROID_ADB_ENABLED)
        //        {
        //            return ADBProcedures.GetDeviceBuildVersionRelease(SerialNumber).GetAwaiter().GetResult();
        //        }
        //        else return null;
        //    }
        //}

        //public string AndroidBuildId
        //{
        //    get
        //    {
        //        if (State != DeviceState.ANDROID_ADB_ENABLED)
        //        {
        //            return ADBProcedures.GetDeviceBuildId(SerialNumber).GetAwaiter().GetResult();
        //        }
        //        else return null;
        //    }
        //}

        public string DeviceStateLocalized
        {
            get
            {
                switch (State) {
                    case DeviceState.ANDROID:
                        return "Android";
                    case DeviceState.ANDROID_ADB_ENABLED:
                        return "Android (ADB Connected)";
                    case DeviceState.WINDOWS:
                        return "Windows";
                    case DeviceState.BOOTLOADER:
                        return "Bootloader";
                    case DeviceState.FASTBOOT:
                        return "Fastboot";
                    case DeviceState.TWRP:
                        return "TWRP";
                    case DeviceState.DISCONNECTED:
                        return "Disconnected";
                    default:
                        return null;
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                switch (State)
                {
                    case DeviceState.DISCONNECTED:
                        return false;
                    default: return true;
                }
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return !IsConnected;
            }
        }

        public bool ADBConnected
        {
            get
            {
                return State == DeviceState.ANDROID_ADB_ENABLED;
            }
        }

        public bool ADBORTWRPConnected
        {
            get
            {
                return State == DeviceState.ANDROID_ADB_ENABLED || State == DeviceState.TWRP;
            }
        }

        public bool TWRPConnected
        {
            get
            {
                return State == DeviceState.TWRP;
            }
        }

        public bool FastbootConnected
        {
            get
            {
                return State == DeviceState.FASTBOOT;
            }
        }
    }
}
