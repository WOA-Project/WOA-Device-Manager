using AndroidDebugBridge;
using FastBoot;
using System.Collections.Generic;
using System.Linq;
using UnifiedFlashingPlatform;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Pages
{
    public class DeviceInfo
    {
        public static string GetDeviceIdentityString()
        {
            string deviceIdentityString = "N/A";

            try
            {
                Device device = DeviceManager.Device;

                if (!device.JustDisconnected)
                {
                    if (device.IsADBEnabled && device.AndroidDebugBridgeTransport != null)
                    {
                        AndroidDebugBridgeTransport adb = device.AndroidDebugBridgeTransport;

                        (string variableName, string variableValue)[] allVariables = adb.GetAllVariables();

                        if (allVariables != null)
                        {
                            deviceIdentityString = string.Join("\n", allVariables.Select(t => $"{t.variableName}: {t.variableValue}"));
                        }
                    }
                    else if (device.IsFastBootEnabled && device.FastBootTransport != null)
                    {
                        if (device.FastBootTransport.GetAllVariables(out (string variableName, string variableValue)[] allVariables))
                        {
                            deviceIdentityString = string.Join("\n", allVariables.Select(t => $"{t.variableName}: {t.variableValue}"));
                        }
                    }
                    else if (device.IsInUFP && device.UnifiedFlashingPlatformTransport != null)
                    {
                        UnifiedFlashingPlatformTransport ufp = device.UnifiedFlashingPlatformTransport;

                        string PlatformID = ufp.ReadDevicePlatformID();
                        string ProcessorManufacturer = ufp.ReadProcessorManufacturer();

                        deviceIdentityString = $"Platform ID: {PlatformID}\nProcessor Manufacturer: {ProcessorManufacturer}";
                    }
                }
            }
            catch { }

            return deviceIdentityString;
        }

        public static string GetDeviceLogString()
        {
            string deviceLogString = "N/A";

            try
            {
                Device device = DeviceManager.Device;

                if (!device.JustDisconnected)
                {
                    if (device.IsInUFP && device.UnifiedFlashingPlatformTransport != null)
                    {
                        UnifiedFlashingPlatformTransport ufp = device.UnifiedFlashingPlatformTransport;

                        deviceLogString = ufp.ReadLog();
                    }
                }
            }
            catch { }

            return deviceLogString;
        }

        public static string GetFlashInfoString()
        {
            string flashInfoString = "N/A";

            try
            {
                Device device = DeviceManager.Device;

                if (!device.JustDisconnected)
                {
                    if (device.IsInUFP && device.UnifiedFlashingPlatformTransport != null)
                    {
                        UnifiedFlashingPlatformTransport ufp = device.UnifiedFlashingPlatformTransport;

                        UnifiedFlashingPlatformTransport.PhoneInfo phoneInfo = ufp.ReadPhoneInfo();

                        flashInfoString = $"App: {phoneInfo.App}\n";
                        flashInfoString += $"App Type: {phoneInfo.AppType}\n";
                        flashInfoString += $"Async Support: {phoneInfo.AsyncSupport}\n";
                        flashInfoString += $"Authenticated: {phoneInfo.Authenticated}\n";
                        flashInfoString += $"Baseboard Manufacturer: {phoneInfo.BaseboardManufacturer}\n";
                        flashInfoString += $"Baseboard Product: {phoneInfo.BaseboardProduct}\n";
                        flashInfoString += $"Boot Devices: {phoneInfo.BootDevices.Count} boot device(s):\n";
                        for (int i = 0; i < phoneInfo.BootDevices.Count; i++)
                        {
                            (uint SectorCount, uint SectorSize, ushort FlashType, ushort FlashIndex, uint Unknown, string DevicePath) = phoneInfo.BootDevices[i];

                            BootDevice bootDevice = new(SectorCount, SectorSize, FlashType, FlashIndex, Unknown, DevicePath);

                            flashInfoString += $"  {i}: Sector Count: {SectorCount}\n";
                            flashInfoString += $"      Sector Size: {SectorSize}\n";
                            flashInfoString += $"      Flash Type: {FlashType}\n";
                            flashInfoString += $"      Flash Index: {FlashIndex}\n";
                            flashInfoString += $"      Unknown: {Unknown}\n";
                            flashInfoString += $"      Device Path: {DevicePath}: {BootDevice.FormatDevicePath(DevicePath)}\n";
                        }
                        flashInfoString += $"Emmc Size In Sectors: {phoneInfo.EmmcSizeInSectors}\n";
                        flashInfoString += $"Family: {phoneInfo.Family}\n";
                        flashInfoString += $"Flash App Protocol Version Major: {phoneInfo.FlashAppProtocolVersionMajor}\n";
                        flashInfoString += $"Flash App Protocol Version Minor: {phoneInfo.FlashAppProtocolVersionMinor}\n";
                        flashInfoString += $"Flash App Version Major: {phoneInfo.FlashAppVersionMajor}\n";
                        flashInfoString += $"Flash App Version Minor: {phoneInfo.FlashAppVersionMinor}\n";
                        flashInfoString += $"Is Bootloader Secure: {phoneInfo.IsBootloaderSecure}\n";
                        flashInfoString += $"Jtag Disabled: {phoneInfo.JtagDisabled}\n";
                        flashInfoString += $"Largest Memory Region: {phoneInfo.LargestMemoryRegion}\n";
                        flashInfoString += $"Manufacturer: {phoneInfo.Manufacturer}\n";
                        flashInfoString += $"Mmos Over Usb Supported: {phoneInfo.MmosOverUsbSupported}\n";
                        flashInfoString += $"Platform ID: {phoneInfo.PlatformID}\n";
                        flashInfoString += $"Platform Secure Boot Enabled: {phoneInfo.PlatformSecureBootEnabled}\n";
                        flashInfoString += $"Product Name: {phoneInfo.ProductName}\n";
                        flashInfoString += $"Product Version: {phoneInfo.ProductVersion}\n";
                        flashInfoString += $"Rdc Present: {phoneInfo.RdcPresent}\n";
                        flashInfoString += $"SKU Number: {phoneInfo.SKUNumber}\n";
                        flashInfoString += $"Sd Card Size In Sectors: {phoneInfo.SdCardSizeInSectors}\n";
                        flashInfoString += $"Secondary Hardware Key Present: {phoneInfo.SecondaryHardwareKeyPresent}\n";
                        flashInfoString += $"Secure Ffu Enabled: {phoneInfo.SecureFfuEnabled}\n";
                        flashInfoString += $"Secure Ffu Supported Protocol Mask: {phoneInfo.SecureFfuSupportedProtocolMask}\n";
                        flashInfoString += $"State: {phoneInfo.State}\n";
                        flashInfoString += $"Transfer Size: {phoneInfo.TransferSize}\n";
                        flashInfoString += $"Uefi Secure Boot Enabled: {phoneInfo.UefiSecureBootEnabled}\n";
                        flashInfoString += $"Write Buffer Size: {phoneInfo.WriteBufferSize}\n";

                        string ProcessorManufacturer = ufp.ReadProcessorManufacturer();

                        flashInfoString += $"Processor Manufacturer: {ProcessorManufacturer}";
                    }
                }
            }
            catch { }

            return flashInfoString;
        }

        public static BootDevice[] GetBootDevices()
        {
            List<BootDevice> bootDevices = [];

            try
            {
                Device device = DeviceManager.Device;

                if (!device.JustDisconnected)
                {
                    if (device.IsInUFP && device.UnifiedFlashingPlatformTransport != null)
                    {
                        UnifiedFlashingPlatformTransport ufp = device.UnifiedFlashingPlatformTransport;

                        UnifiedFlashingPlatformTransport.PhoneInfo phoneInfo = ufp.ReadPhoneInfo();

                        for (int i = 0; i < phoneInfo.BootDevices.Count; i++)
                        {
                            (uint SectorCount, uint SectorSize, ushort FlashType, ushort FlashIndex, uint Unknown, string DevicePath) = phoneInfo.BootDevices[i];

                            BootDevice bootDevice = new(SectorCount, SectorSize, FlashType, FlashIndex, Unknown, DevicePath);
                            bootDevices.Add(bootDevice);
                        }
                    }
                }
            }
            catch { }

            return [.. bootDevices];
        }
    }
}
