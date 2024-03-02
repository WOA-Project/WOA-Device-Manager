namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        public void Reboot(string mode = "")
        {
            uint shellLocalId = ++LocalId;

            AndroidDebugBridgeMessage RebootMessage = AndroidDebugBridgeMessage.GetOpenMessage(shellLocalId, $"reboot:{mode}");
            SendMessage(RebootMessage);

            // The phone can reply ok here but not always the case!
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