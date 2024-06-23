using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Pages
{
    public sealed partial class UnlockBootloaderPage : Page
    {
        private Device device = DeviceManager.Device;

        public UnlockBootloaderPage()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Bindings.Update();
        }

        private async void UnlockBootloaderButton_Click(object sender, RoutedEventArgs e)
        {
            while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                MainPage.SetStatus("Rebooting phone to Bootloader mode...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                try
                {
                    await DeviceRebootHelper.RebootToBootloaderAndWait();
                }
                catch { }

                MainPage.ToggleLoadingScreen(false);
            }

            if (FastBootProcedures.CanUnlock())
            {
                FastBootProcedures.FlashUnlock(this);
            }
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
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
