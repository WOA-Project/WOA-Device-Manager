using AndroidDebugBridge;
using System.Text;

namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string usbid = @"\\?\USB#VID_045E&PID_066B#B6697D4B#{dee824ef-729b-4a0e-9c14-b7117d33a817}";

            UnifiedFlashingPlatform.UnifiedFlashingPlatformTransport ufp = new(usbid);
            ufp.ReadDevicePlatformID();
            ufp.ReadDeviceProcessorManufacturer();
            ufp.Shutdown();
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