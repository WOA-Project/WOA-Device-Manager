using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SAPTeam.AndroCtrl.Adb;
using SAPTeam.AndroCtrl.Adb.Receivers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WOADeviceManager.Entities;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class DevicePage : Page
    {
        private Device device = DeviceManager.Device;

        public DevicePage()
        {
            InitializeComponent();

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                Bindings.Update();
            });
        }

        private string BatteryLevelFormatted
        {
            get
            {
                if (device != null && device.BatteryLevel != null)
                {
                    return $"Battery level: {device.BatteryLevel}%";
                }
                else
                {
                    return "Battery level: Unknown";
                }
            }
        }

        private bool IsDeviceConnected
        {
            get
            {
                return device != null && device.IsConnected;
            }
        }

        private bool IsDeviceDisconnected
        {
            get
            {
                return !IsDeviceConnected;
            }
        }
    }
}
