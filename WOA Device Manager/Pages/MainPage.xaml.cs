using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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

            // TODO: Initial enabling/disabling of UI based on device availability

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
            ToggleLoadingScreen(true);
            ADBProcedures.RebootToBootloader(device.SerialNumber);
            ToggleLoadingScreen(false);
        }

        private void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            ToggleLoadingScreen(true);
            FastbootProcedures.Reboot(device.SerialNumber);
            ToggleLoadingScreen(false);
        }

        private void FlashUnlock_Click(object sender, RoutedEventArgs e)
        {
            _ = FastbootProcedures.FlashUnlock(device.SerialNumber, this);
        }

        private void FlashLock_Click(object sender, RoutedEventArgs e)
        {
            FastbootProcedures.FlashLock(device.SerialNumber, this);
        }

        private async void BootTWRP_Click(object sender, RoutedEventArgs e)
        {
            ToggleLoadingScreen(true);
            await FastbootProcedures.BootTWRP(device.SerialNumber);
            ToggleLoadingScreen(false);
        }

        private void ToggleLoadingScreen(bool show)
        {
            if (show) ProgressOverlay.Visibility = Visibility.Visible;
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = show ? 0 : 1,
                To = show ? 1 : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            };

            if (!show)
            {
                fadeAnimation.Completed += (s, e) =>
                {
                    ProgressOverlay.Visibility = Visibility.Collapsed;
                };
            }

            Storyboard.SetTarget(fadeAnimation, ProgressOverlay);
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            storyboard.Begin();
        }
    }
}
