using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            InitializeComponent();

            DeviceManager.Instance.DeviceFoundEvent += DeviceManager_DeviceFoundEvent;
        }

        private void DeviceManager_DeviceFoundEvent(DeviceWatcher sender, Device device)
        {
            // TODO: You stopped here last time. Event gets called only at launch and doesn't get called when a device is plugged...

            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                DebugInfo.Text = $"{device.DeviceName} Found!";
            });
        }
    }
}
