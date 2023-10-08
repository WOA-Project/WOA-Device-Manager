using SAPTeam.AndroCtrl.Adb;
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
        public enum DeviceStateEnum
        {
            ANDROID, ANDROID_ADB_ENABLED, WINDOWS, BOOTLOADER, FASTBOOT, TWRP, DISCONNECTED
        }

        public enum OEMUnlockStateEnum
        {
            UNLOCKED, LOCKED, UNKNOWN
        }

        public Device() { }

        public string ID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public DeviceStateEnum State { get; set; } = DeviceStateEnum.DISCONNECTED;
        public OEMUnlockStateEnum OEMUnlockState { get; set; }
        public DeviceInformation Information { get; set; }
        public DeviceInformationUpdate LastInformationUpdate { get; set; }
        public DeviceData Data { get; set; }

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

        public string BatteryLevel
        {
            get
            {
                if (State == DeviceStateEnum.ANDROID_ADB_ENABLED) return ADBProcedures.GetDeviceBatteryLevel();
                else return null;
            }
        }

        public string DeviceStateLocalized
        {
            get
            {
                switch (State) {
                    case DeviceStateEnum.ANDROID:
                        return "Android";
                    case DeviceStateEnum.ANDROID_ADB_ENABLED:
                        return "Android (ADB Connected)";
                    case DeviceStateEnum.WINDOWS:
                        return "Windows";
                    case DeviceStateEnum.BOOTLOADER:
                        return "Bootloader";
                    case DeviceStateEnum.FASTBOOT:
                        return "Fastboot";
                    case DeviceStateEnum.TWRP:
                        return "TWRP";
                    case DeviceStateEnum.DISCONNECTED:
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
                    case DeviceStateEnum.DISCONNECTED:
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
                return State == DeviceStateEnum.ANDROID_ADB_ENABLED;
            }
        }

        public bool ADBORTWRPConnected
        {
            get
            {
                return State == DeviceStateEnum.ANDROID_ADB_ENABLED || State == DeviceStateEnum.TWRP;
            }
        }

        public bool TWRPConnected
        {
            get
            {
                return State == DeviceStateEnum.TWRP;
            }
        }

        public bool FastbootConnected
        {
            get
            {
                return State == DeviceStateEnum.FASTBOOT;
            }
        }
    }
}
