using AndroidDebugBridge;

namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string usbid = @"\\?\USB#VID_045E&PID_0C26#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}";

            Console.WriteLine($"Opening {usbid}...");
            using AndroidDebugBridgeTransport androidDebugBridgeTransport = new (usbid);

            Console.WriteLine("Connecting...");
            androidDebugBridgeTransport.Connect();

            Console.WriteLine("Opening shell...");
            androidDebugBridgeTransport.Shell();

            Console.WriteLine("Shell closed!");

            Console.WriteLine("Rebooting...");
            androidDebugBridgeTransport.Reboot();
        }
    }
}
