using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    internal class UFPProcedures
    {
        private static object lockObject = new();

        public static void Reboot()
        {
            lock (lockObject)
            {
                DeviceManager.Device.UnifiedFlashingPlatformTransport.RebootPhone();
            }
        }

        public static void ContinueBoot()
        {
            lock (lockObject)
            {
                DeviceManager.Device.UnifiedFlashingPlatformTransport.ContinueBoot();
            }
        }

        public static void Shutdown()
        {
            lock (lockObject)
            {
                DeviceManager.Device.UnifiedFlashingPlatformTransport.Shutdown();
            }
        }

        public static void MassStorage()
        {
            lock (lockObject)
            {
                DeviceManager.Device.UnifiedFlashingPlatformTransport.MassStorage();
            }
        }
    }
}
