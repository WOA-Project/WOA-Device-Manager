using SAPTeam.AndroCtrl.Adb;

namespace WOADeviceManager.Managers
{
    internal class ADBManager
    {
        private static AdbServer server;
        private static AdbClient client;
        public static AdbClient Client
        {
            get
            {
                client ??= new AdbClient();
                return client;
            }
        }

        private static ShellSocket shell;
        public static ShellSocket Shell
        {
            get
            {
                shell ??= client.StartShell(DeviceManager.Device.AndroidDebugBridgeTransport);
                return shell;
            }
        }

        private static ADBManager _instance;
        public static ADBManager Instance
        {
            get
            {
                _instance ??= new ADBManager();
                return _instance;
            }
        }

        private ADBManager()
        {
            server = new AdbServer();
            _ = server.StartServer(@"G:\_tools\_platform-tools\platform-tools__\adb.exe", restartServerIfNewer: true);
        }
    }
}
