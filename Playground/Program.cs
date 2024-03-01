using AndroidDebugBridge;

namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            string usbid = @"\\?\USB#VID_045E&PID_0C26#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}";
            using AndroidDebugBridgeTransport androidDebugBridgeTransport = new AndroidDebugBridgeTransport(usbid);
            androidDebugBridgeTransport.Connect();
        }
    }
}
