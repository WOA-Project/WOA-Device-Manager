using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WOADeviceManager.Managers;

namespace WOADeviceManager
{
    public partial class App : Application
    {
        public DeviceManager DeviceManager;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow()
            {
                ExtendsContentIntoTitleBar = true,
                Title = "WOA Device Manager"
            };

            m_window_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            MainWindowAW = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(m_window_hwnd));

            mainWindow.Activate();

            DeviceManager = new DeviceManager();
        }

        public static Window mainWindow;
        public static IntPtr m_window_hwnd;
        public static AppWindow MainWindowAW;
    }
}
