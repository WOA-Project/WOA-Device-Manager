using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class SwitchModePage : Page
    {
        public SwitchModePage()
        {
            InitializeComponent();

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private async void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToAndroidAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToBootloaderAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToFastbootd_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToFastbootDAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToMassStorageMode_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToMSCAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToRecovery_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToRecoveryAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToTWRPAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToWindows_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            try
            {
                await DeviceRebootHelper.RebootToUEFIAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
    }
}
