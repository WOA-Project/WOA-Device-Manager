using AndroidDebugBridge;
using System.Text;
using UnifiedFlashingPlatform;

namespace Playground
{
    internal class Program
    {
        private static void TestReadParam(UnifiedFlashingPlatformTransport ufp)
        {
            Console.WriteLine("App Type: " + ufp.ReadAppType());
            Console.WriteLine("Reset Protection: " + ufp.ReadResetProtection());
            Console.WriteLine("Bitlocker: " + ufp.ReadBitlocker());
            Console.WriteLine("Build Info: " + ufp.ReadBuildInfo());
            Console.WriteLine("Current Boot Option: " + ufp.ReadCurrentBootOption());
            Console.WriteLine("Device Async Support: " + ufp.ReadDeviceAsyncSupport());
            Console.WriteLine("Directory Entry Sizes (Partition: ESP, Directory: EFI): " + ufp.ReadDirectoryEntriesSize("esp", "EFI"));
            Console.WriteLine("Device Platform ID: " + ufp.ReadDevicePlatformID());
            Console.WriteLine("Device Properties: " + ufp.ReadDeviceProperties());
            Console.WriteLine("Device Targeting Info: " + ufp.ReadDeviceTargetInfo());
            Console.WriteLine("Data Verify Speed: " + ufp.ReadDataVerifySpeed() + "  kB/s");
            Console.WriteLine("Device ID: " + ufp.ReadDeviceID());
            Console.WriteLine("eMMC Test Result: " + ufp.ReadEmmcTestResult());
            Console.WriteLine("eMMC Size: " + ufp.ReadEmmcSize());
            Console.WriteLine("eMMC Write Speed: " + ufp.ReadEmmcWriteSpeed());
            Console.WriteLine("Flash App Info: " + ufp.ReadFlashAppInfo());
            Console.WriteLine("Flash Options: " + ufp.ReadFlashOptions());
            Console.WriteLine("Flashing Status: " + ufp.ReadFlashingStatus());
            Console.WriteLine("File Size (Partition: ESP, File: EFI\\Boot\\bootaa64.efi): " + ufp.ReadFileSize("esp", "EFI\\Boot\\bootaa64.efi"));
            Console.WriteLine("SecureBoot Status: " + ufp.ReadSecureBootStatus());
            Console.WriteLine("UEFI Variable: " + ufp.ReadUEFIVariable(new("{3811BE0C-6AEF-44DD-9382-CD99A24C6619}"), "CurrentMsSurfaceUefiRuntimeModeVariableName", 1));
            Console.WriteLine("UEFI Variable Size: " + ufp.ReadUEFIVariableSize(new("{3811BE0C-6AEF-44DD-9382-CD99A24C6619}"), "CurrentMsSurfaceUefiRuntimeModeVariableName"));
            Console.WriteLine("Largest Memory Region: " + ufp.ReadLargestMemoryRegion());
            Console.WriteLine("Log Size (Type: Flashing): " + ufp.ReadLogSize(UnifiedFlashingPlatformTransport.DeviceLogType.Flashing));
            Console.WriteLine("Mac Address: " + ufp.ReadMacAddress());
            Console.WriteLine("Mode Data: " + ufp.ReadModeData(UnifiedFlashingPlatformTransport.Mode.DiagnosticMode));
            Console.WriteLine("Processor Manufacturer: " + ufp.ReadProcessorManufacturer());
            Console.WriteLine("SD Card Size: " + ufp.ReadSDCardSize());
            Console.WriteLine("Supported FFU Protocol Info: " + ufp.ReadSupportedFFUProtocolInfo());
            Console.WriteLine("SMBIOS Data: " + ufp.ReadSMBIOSData());
            Console.WriteLine("S/N: " + ufp.ReadSerialNumber());
            Console.WriteLine("Size of System Memory: " + ufp.ReadSizeOfSystemMemory() + " kB");
            Console.WriteLine("Security Status: " + ufp.ReadSecurityStatus());
            Console.WriteLine("Telemetry Log Size: " + ufp.ReadTelemetryLogSize());
            Console.WriteLine("Read Transfer Size: " + ufp.ReadTransferSize());
            Console.WriteLine("UEFI Boot Flag: " + ufp.ReadUEFIBootFlag());
            Console.WriteLine("UEFI Boot Options: " + ufp.ReadUEFIBootOptions());
            Console.WriteLine("Unlock ID: " + ufp.ReadUnlockID());
            Console.WriteLine("Unlock Token Files: " + ufp.ReadUnlockTokenFiles());
            Console.WriteLine("USB Speed: " + ufp.ReadUSBSpeed());
            Console.WriteLine("Read Write Buffer Size: " + ufp.ReadWriteBufferSize());
        }

        static void Main(string[] args)
        {
            string usbid = @"\\?\USB#VID_045E&PID_066B#B6697D4B#{dee824ef-729b-4a0e-9c14-b7117d33a817}";

            UnifiedFlashingPlatformTransport ufp = new(usbid);
            TestReadParam(ufp);
            return;

            // OK
            //ufp.Hello();

            // Returns 2...
            /*GPT gpt = ufp.ReadGPT();
            foreach (var partition in gpt.Partitions)
            {
                Console.WriteLine(partition.Name);
                Console.WriteLine(partition.PartitionGuid);
                Console.WriteLine(partition.PartitionTypeGuid);
                Console.WriteLine(partition.FirstSectorAsString);
                Console.WriteLine(partition.LastSectorAsString);
                Console.WriteLine(partition.AttributesAsString);
                Console.WriteLine("-------------------------------");
            }*/

            // TODO: Reboots phone (need to not read the reply for this to work ok)
            //ufp.Shutdown();

            // OK
            //ufp.ResetPhone();

            // TODO: Test on the MTP for safety
            //ufp.FlashSectors();

            //ufp.MassStorage();
            //byte[] phoneInfoResponse = ufp.ReadPhoneInfo();

            //Console.WriteLine(Encoding.ASCII.GetString(phoneInfoResponse));
            //Console.WriteLine(BitConverter.ToString(phoneInfoResponse));

            //ufp.ResetPhone();

            string message = " ___  ___  ________ ________        ___  ________           ________  ________  ________  ___          \r\n|\\  \\|\\  \\|\\  _____\\\\   __  \\      |\\  \\|\\   ____\\         |\\   ____\\|\\   __  \\|\\   __  \\|\\  \\         \r\n\\ \\  \\\\\\  \\ \\  \\__/\\ \\  \\|\\  \\     \\ \\  \\ \\  \\___|_        \\ \\  \\___|\\ \\  \\|\\  \\ \\  \\|\\  \\ \\  \\        \r\n \\ \\  \\\\\\  \\ \\   __\\\\ \\   ____\\     \\ \\  \\ \\_____  \\        \\ \\  \\    \\ \\  \\\\\\  \\ \\  \\\\\\  \\ \\  \\       \r\n  \\ \\  \\\\\\  \\ \\  \\_| \\ \\  \\___|      \\ \\  \\|____|\\  \\        \\ \\  \\____\\ \\  \\\\\\  \\ \\  \\\\\\  \\ \\  \\____  \r\n   \\ \\_______\\ \\__\\   \\ \\__\\          \\ \\__\\____\\_\\  \\        \\ \\_______\\ \\_______\\ \\_______\\ \\_______\\\r\n    \\|_______|\\|__|    \\|__|           \\|__|\\_________\\        \\|_______|\\|_______|\\|_______|\\|_______|\r\n                                           \\|_________|                                                \r\n                                                                                                       \r\n                                                                                                       ";

            
            ufp.DisplayCustomMessage(message, 0);













            //Console.WriteLine(ufp.ReadLog());

            //Debug.ParseUsbLyzerTrace(@"C:\Users\Gus\Downloads\Telegram Desktop\UFP.csv");
            /*//string usbid = @"\\?\USB#VID_045E&PID_0C26#903220503667#{dee824ef-729b-4a0e-9c14-b7117d33a817}";
            string usbid = @"\\?\USB#VID_045E&PID_0C26#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}"; // Android (Duo 2)
            //string usbid = @"\\?\USB#VID_18D1&PID_D001#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}"; // TWRP (Duo 2)

            Console.WriteLine($"Opening {usbid}...");
            using AndroidDebugBridgeTransport transport = new(usbid);

            Console.WriteLine("Connecting...");
            transport.Connect();

            transport.WaitTilConnected();

            Console.WriteLine($"Connected to:");
            Console.WriteLine($"  Protocol version: {transport.PhoneSupportedProtocolVersion}");
            Console.WriteLine($"  Connection environment: {transport.PhoneConnectionEnvironment}");
            Console.WriteLine("  Connection variables:");
            foreach (KeyValuePair<string, string> variable in transport.PhoneConnectionVariables)
            {
                Console.WriteLine($"    {variable.Key}: {variable.Value}");
            }
            Console.WriteLine("  Connection features:");
            foreach (string feature in transport.PhoneConnectionFeatures)
            {
                Console.WriteLine($"    {feature}");
            }

            Console.WriteLine("Opening shell...");
            transport.Shell();

            Console.WriteLine("Shell closed!");

            Console.WriteLine("Rebooting...");
            transport.Reboot();*/
        }
    }
}