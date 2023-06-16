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
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class MainPage : Page
    {
        Device device = null;

        public MainPage()
        {
            InitializeComponent();

            DeviceManager.Instance.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.Instance.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private void Instance_DeviceDisconnectedEvent(DeviceWatcher sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                DebugInfo.Text = $"{device.Name} Disconnected";
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(DeviceWatcher sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                DebugInfo.Text = $"{device.Name} Connected";
                Bindings.Update();
            });
        }

        private void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            ADBProcedures.RebootToBootloader(device.SerialNumber);
        }
    }
}
