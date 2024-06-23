using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using UnifiedFlashingPlatform;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class MainPage : Page
    {
        private Device device = DeviceManager.Device;
        private static MainPage _mainPage;

        public MainPage()
        {
            InitializeComponent();

            _mainPage = this;
        }

        private bool DeviceNeededOverlayShown = false;

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                if (_mainPage.ProgressOverlay.Visibility == Visibility.Collapsed)
                {
                    DeviceNeededOverlayShown = true;
                    SetStatus(Emoji: "📱", Title: "Connect a compatible device", SubTitle: "WOA Device Manager helps you install, update and manage Windows on your Android device. Please connect a compatible device.", Message: "Checking for new connected devices...");
                }

                this.device = device;
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                if (_mainPage.ProgressOverlay.Visibility == Visibility.Visible && DeviceNeededOverlayShown)
                {
                    SetStatus();
                }

                DeviceNeededOverlayShown = false;

                this.device = device;
                Bindings.Update();
            });
        }

        private void MainNavigationSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                NavigationViewItem selectedItem = e.SelectedItem as NavigationViewItem;
                switch (selectedItem.Tag)
                {
                    case "status":
                        _ = MainNavigationFrame.Navigate(typeof(DevicePage));
                        break;
                    case "manualmode":
                        _ = MainNavigationFrame.Navigate(typeof(SwitchModePage));
                        break;
                    case "flashwindows":
                        _ = MainNavigationFrame.Navigate(typeof(FlashPage));
                        break;
                    case "unlockbootloader":
                    case "restorebootloader":
                    case "enabledualboot":
                    case "disabledualboot":
                    case "backupwindows":
                    case "updatewindows":
                    case "downloadwindows":
                        break;
                    case "partitions":
                        _ = MainNavigationFrame.Navigate(typeof(PartitionsPage));
                        break;
                    case "debug":
                        _ = MainNavigationFrame.Navigate(typeof(zDebugPage));
                        break;
                }
            }
        }

        public static void ToggleLoadingScreen(bool show, bool resetStatus = true)
        {
            _ = _mainPage.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, () =>
            {
                if (show)
                {
                    _mainPage.ProgressOverlay.Visibility = Visibility.Visible;

                    if (resetStatus)
                    {
                        _mainPage.BusyControl.SetStatus("Waiting for device...");
                    }
                }

                DoubleAnimation fadeAnimation = new()
                {
                    From = show ? 0 : 1,
                    To = show ? 1 : 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                };

                if (!show)
                {
                    fadeAnimation.Completed += (s, e) =>
                    {
                        _mainPage.BusyControl.SetStatus();
                        _mainPage.ProgressOverlay.Visibility = Visibility.Collapsed;
                    };
                }

                Storyboard.SetTarget(fadeAnimation, _mainPage.ProgressOverlay);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                Storyboard storyboard = new();
                storyboard.Children.Add(fadeAnimation);
                storyboard.Begin();
            });
        }

        public static void SetStatus(string? Message = null, uint? Percentage = null, string? Text = null, string? SubMessage = null, string? Title = null, string? SubTitle = null, string? Emoji = null)
        {
            bool IsNull = Message == null && Percentage == null && Text == null && SubMessage == null && Title == null && SubTitle == null && Emoji == null;

            _ = _mainPage.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, () =>
            {
                if (IsNull && _mainPage.ProgressOverlay.Visibility == Visibility.Visible)
                {
                    ToggleLoadingScreen(false, false);
                }
                else if (!IsNull && _mainPage.ProgressOverlay.Visibility == Visibility.Collapsed)
                {
                    ToggleLoadingScreen(true, false);
                }

                _mainPage.BusyControl.SetStatus(Message, Percentage, Text, SubMessage, Title, SubTitle, Emoji);
            });
        }

        public static ProgressUpdater GetProgressUpdater(ulong MaxValue, string Message, string? SubMessage = null, string? Title = null, string? SubTitle = null, string? Emoji = null)
        {
            return new(MaxValue, (percentage, eta) =>
            {
                string NewText = null;
                if (percentage != null)
                {
                    NewText = $"Progress: {percentage}%";
                }

                if (eta != null)
                {
                    if (NewText == null)
                    {
                        NewText = "";
                    }
                    else
                    {
                        NewText += " - ";
                    }

                    NewText += $"Estimated time remaining: {eta:h\\:mm\\:ss}";
                }

                if (NewText != null)
                {
                    SetStatus(Message, (uint)percentage, NewText, SubMessage, Title, SubTitle, Emoji);
                }
                else
                {
                    SetStatus("Initializing...");
                }
            });
        }

        private string BatteryLevelFormatted => device != null && device.BatteryLevel != null ? $"Battery level: {device.BatteryLevel}%" : "Battery level: Unknown";

        private bool IsDeviceConnected => device != null && device.IsConnected;

        private bool IsDeviceDisconnected => !IsDeviceConnected;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DeviceManager.Device.IsDisconnected)
            {
                DeviceNeededOverlayShown = true;
                SetStatus(Emoji: "📱", Title: "Connect a compatible device", SubTitle: "WOA Device Manager helps you install, update and manage Windows on your Android device. Please connect a compatible device.", Message: "Checking for new connected devices...");
            }

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DeviceManager.DeviceConnectedEvent -= DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent -= Instance_DeviceDisconnectedEvent;
        }
    }
}
