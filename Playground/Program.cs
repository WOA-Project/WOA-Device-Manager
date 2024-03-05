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
            Console.WriteLine("Directory Entry Sizes: " + ufp.ReadDirectoryEntriesSize("esp", "EFI"));
            Console.WriteLine("Device Platform ID: " + ufp.ReadDevicePlatformID());
            Console.WriteLine("Device Properties: " + ufp.ReadDeviceProperties());
            Console.WriteLine("Device Targeting Info: " + ufp.ReadDeviceTargetInfo());
            ufp.ReadDataVerifySpeed();
            ufp.ReadDeviceID();
            ufp.ReadEmmcTestResult();
            ufp.ReadEmmcSize();
            ufp.ReadEmmcWriteSpeed();
            ufp.ReadFlashAppInfo();
            ufp.ReadFlashOptions();
            ufp.ReadFlashingStatus();
            ufp.ReadFileSize();
            ufp.ReadSecureBootStatus();
            ufp.ReadUEFIVariable();
            ufp.ReadUEFIVariableSize();
            ufp.ReadLargestMemoryRegion();
            ufp.ReadLogSize();
            ufp.ReadMacAddress();
            ufp.ReadModeData();
            ufp.ReadProcessorManufacturer();
            ufp.ReadSDCardSize();
            ufp.ReadSupportedFFUProtocolInfo();
            ufp.ReadSMBIOSData();
            ufp.ReadSerialNumber();
            ufp.ReadSizeOfSystemMemory();
            ufp.ReadSecurityStatus();
            ufp.ReadTelemetryLogSize();
            ufp.ReadTransferSize();
            ufp.ReadUEFIBootFlag();
            ufp.ReadUEFIBootOptions();
            ufp.ReadUnlockID();
            ufp.ReadUnlockTokenFiles();
            ufp.ReadUSBSpeed();
            ufp.ReadWriteBufferSize();
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