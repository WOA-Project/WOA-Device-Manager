using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class MainPage : Page
    {
        private Device device = null;
        private static MainPage _mainPage;

        public MainPage()
        {
            InitializeComponent();

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
            DeviceManager.ManuallyCheckForADBDevices();

            _mainPage = this;
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                //DebugInfo.Text = $"{device.Name} disconnected.";
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => {
                this.device = device;
                //DebugInfo.Text = $"{device.Name} connected in {device.DeviceStateLocalized}.";
                Bindings.Update();
            });
        }

        private void MainNavigationSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            WelcomeView.Visibility = Visibility.Collapsed;
            if (e.SelectedItem != null)
            {
                var selectedItem = (e.SelectedItem as NavigationViewItem);
                switch (selectedItem.Tag)
                {
                    case "status":
                        break;
                    case "managewindows":
                        break;
                    case "partitions":
                        MainNavigationFrame.Navigate(typeof(PartitionsPage));
                        break;
                    case "debug":
                        MainNavigationFrame.Navigate(typeof(zDebugPage));
                        break;
                }
            }
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

        public static void ToggleLoadingScreen(bool show)
        {
            _mainPage.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, () =>
            {
                if (show) _mainPage.ProgressOverlay.Visibility = Visibility.Visible;
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
                        _mainPage.ProgressOverlay.Visibility = Visibility.Collapsed;
                    };
                }

                Storyboard.SetTarget(fadeAnimation, _mainPage.ProgressOverlay);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(fadeAnimation);
                storyboard.Begin();
            });
        }
    }
}
