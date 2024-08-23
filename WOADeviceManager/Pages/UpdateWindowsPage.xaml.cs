using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Pages
{
    public sealed partial class UpdateWindowsPage : Page
    {
        public UpdateWindowsPage()
        {
            InitializeComponent();
        }

        private void ServiceWindowsDriversButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                MainPage.SetStatus("First rebooting the device to Mass Storage mode...", Emoji: "🔄️", SubMessage: "Servicing Windows Drivers");

                try
                {
                    await DeviceRebootHelper.RebootToMSCAndWait();
                    await DriverManager.UpdateDrivers();
                }
                catch (Exception ex)
                {
                    MainPage.ToggleLoadingScreen(false);
                    MainPage.ShowDialog(ex.Message);
                }
            });
        }

        private async void ServiceWindowsDriversButton_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            MainPage.SetStatus("First rebooting the device to Mass Storage mode...", Emoji: "🔄️", SubMessage: "Servicing Windows Drivers");

            string? DriverRepo = null;

            FolderPicker picker = new()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Downloads
            };

            nint windowHandle = WindowNative.GetWindowHandle(App.mainWindow);
            InitializeWithWindow.Initialize(picker, windowHandle);

            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null && Directory.Exists(folder.Path))
            {
                DriverRepo = folder.Path;
            }
            else
            {
                MainPage.ToggleLoadingScreen(false);
                return;
            }

            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                try
                {
                    await DeviceRebootHelper.RebootToMSCAndWait();
                    await DriverManager.UpdateDrivers(DriverRepo);
                }
                catch (Exception ex)
                {
                    MainPage.ToggleLoadingScreen(false);
                    MainPage.ShowDialog(ex.Message);
                }
            });
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                bool isInCompatibleMode = DeviceManager.Device.State is DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceState.BOOTLOADER or DeviceState.ANDROID_ADB_ENABLED;
                if (isInCompatibleMode)
                {
                    StatusText.Text = "The device is in a supported mode. You can continue to service the Windows drivers.";
                    ServiceWindowsDriversButton.IsEnabled = true;
                }
                else
                {
                    StatusText.Text = "You have to connect your device in Mass Storage, TWRP, Bootloader, Android (ADB Connected) before you can continue.";
                    ServiceWindowsDriversButton.IsEnabled = false;
                }
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                bool isInCompatibleMode = DeviceManager.Device.State is DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceState.BOOTLOADER or DeviceState.ANDROID_ADB_ENABLED;
                if (isInCompatibleMode)
                {
                    StatusText.Text = "The device is in a supported mode. You can continue to service the Windows drivers.";
                    ServiceWindowsDriversButton.IsEnabled = true;
                }
                else
                {
                    StatusText.Text = "You have to connect your device in Mass Storage, TWRP, Bootloader, Android (ADB Connected) before you can continue.";
                    ServiceWindowsDriversButton.IsEnabled = false;
                }
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool isInCompatibleMode = DeviceManager.Device.State is DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceState.BOOTLOADER or DeviceState.ANDROID_ADB_ENABLED;
            if (isInCompatibleMode)
            {
                StatusText.Text = "The device is in a supported mode. You can continue to service the Windows drivers.";
                ServiceWindowsDriversButton.IsEnabled = true;
            }
            else
            {
                StatusText.Text = "You have to connect your device in Mass Storage, TWRP, Bootloader, Android (ADB Connected) before you can continue.";
                ServiceWindowsDriversButton.IsEnabled = false;
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
