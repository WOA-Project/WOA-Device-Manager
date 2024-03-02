using AndroidDebugBridge;

namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string usbid = @"\\?\USB#VID_045E&PID_0C26#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}"; // Android (Duo 2)
            //string usbid = @"\\?\USB#VID_18D1&PID_D001#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}"; // TWRP (Duo 2)

            Console.WriteLine($"Opening {usbid}...");
            using AndroidDebugBridgeTransport transport = new(usbid);

            Console.WriteLine("Connecting...");
            transport.Connect();

            transport.WaitTilConnected();

            Console.WriteLine($"Connected to: {transport.PhoneConnectionString}");
            Console.WriteLine($"Protocol version: {transport.PhoneSupportedProtocolVersion}");

            Console.WriteLine("Opening shell...");
            transport.Shell();

            Console.WriteLine("Shell closed!");

            Console.WriteLine("Rebooting...");
            transport.Reboot();
        }
    }
}
