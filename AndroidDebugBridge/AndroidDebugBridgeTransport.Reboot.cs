namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        public void Reboot(string mode = "")
        {
            using AndroidDebugBridgeStream stream = OpenStream($"reboot:{mode}");
        }

        public void RebootBootloader()
        {
            Reboot("bootloader");
        }

        public void RebootRecovery()
        {
            Reboot("recovery");
        }

        public void RebootFastBootD()
        {
            Reboot("fastboot");
        }
    }
}