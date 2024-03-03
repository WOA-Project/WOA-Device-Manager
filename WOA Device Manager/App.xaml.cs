using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using WOADeviceManager.Managers;

namespace WOADeviceManager
{
    public partial class App : Application
    {
        public static DeviceManager DeviceManager
        {
            get; private set;
        }

        public App()
        {
            InitializeComponent();
            KillAnyADBInstance();
            DeviceManager = DeviceManager.Instance;
        }

        private static void KillAnyADBInstance()
        {
            Process proc = new()
            {
                StartInfo = new("taskkill.exe", "/IM adb.exe /F")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            proc.Start();
            proc.WaitForExit();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow()
            {
                ExtendsContentIntoTitleBar = true,
                Title = "WOA Device Manager"
            };

            m_window_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            MainWindowAW = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(m_window_hwnd));

            mainWindow.Activate();
        }

        public static Window mainWindow;
        public static IntPtr m_window_hwnd;
        public static AppWindow MainWindowAW;
    }
}
