using FastBoot;
using Img2Ffu.Reader.Data;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnifiedFlashingPlatform;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class FlashPage : Page
    {
        public string SelectedFFUPath { get; set; } = "";

        public FlashPage()
        {
            InitializeComponent();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Downloads,
                FileTypeFilter = { ".ffu" }
            };

            nint windowHandle = WindowNative.GetWindowHandle(App.mainWindow);
            InitializeWithWindow.Initialize(picker, windowHandle);

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                SelectedFFUPath = file.Path;
            }

            if (!File.Exists(SelectedFFUPath))
            {
                SelectedFFUPath = "";
                SelectRun.Text = "Select the FFU-file to flash to the phone...";
                FlashFFUImageButton.IsEnabled = false;
            }
            else
            {
                SelectRun.Text = "Change";
                FlashFFUImageButton.IsEnabled = DeviceManager.Device.UFPConnected;
            }

            Bindings.Update();
        }

        private void FlashFFUImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(SelectedFFUPath))
            {
                SelectedFFUPath = "";
                SelectRun.Text = "Select the FFU-file to flash to the phone...";
                FlashFFUImageButton.IsEnabled = false;
                Bindings.Update();
                return;
            }

            MainPage.SetStatus("Initializing...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

            using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
            SignedImage signedImage = new(FFUStream);
            int chunkSize = signedImage.ChunkSize;
            ulong totalChunkCount = (ulong)FFUStream.Length / (ulong)chunkSize;

            FFUStream.Seek(0, SeekOrigin.Begin);

            UnifiedFlashingPlatformTransport.ProgressUpdater updater = MainPage.GetProgressUpdater(totalChunkCount, $"Flashing {Path.GetFileName(SelectedFFUPath)}...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
                DeviceManager.Device.UnifiedFlashingPlatformTransport.FlashFFU(FFUStream, updater);

                MainPage.SetStatus("Rebooting Phone...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                while (DeviceManager.Device.State == Device.DeviceStateEnum.UFP || DeviceManager.Device.State == Device.DeviceStateEnum.DISCONNECTED)
                {
                    await Task.Delay(1000);
                }

                if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
                {
                    MainPage.SetStatus("Escaping Bootloader Menu...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                    DeviceManager.Device.FastBootTransport.SetActiveOther();
                    DeviceManager.Device.FastBootTransport.SetActiveOther();
                    DeviceManager.Device.FastBootTransport.ContinueBoot();

                    while (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
                    {
                        await Task.Delay(1000);
                    }

                    MainPage.SetStatus("Booting Phone...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                    while (DeviceManager.Device.State == Device.DeviceStateEnum.DISCONNECTED)
                    {
                        await Task.Delay(1000);
                    }
                }

                MainPage.SetStatus();
            });
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                if (!File.Exists(SelectedFFUPath))
                {
                    SelectedFFUPath = "";
                    SelectRun.Text = "Select the FFU-file to flash to the phone...";
                    FlashFFUImageButton.IsEnabled = false;
                }
                else
                {
                    SelectRun.Text = "Change";
                    FlashFFUImageButton.IsEnabled = device.UFPConnected;
                }

                if (device.UFPConnected)
                {
                    StatusText.Text = "The phone is in Flash mode. You can continue to flash the FFU image.";
                }
                else
                {
                    StatusText.Text = "You have to connect your phone before you can continue.";
                }

                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                if (!File.Exists(SelectedFFUPath))
                {
                    SelectedFFUPath = "";
                    SelectRun.Text = "Select the FFU-file to flash to the phone...";
                    FlashFFUImageButton.IsEnabled = false;
                }
                else
                {
                    SelectRun.Text = "Change";
                    FlashFFUImageButton.IsEnabled = device.UFPConnected;
                }

                if (device.UFPConnected)
                {
                    StatusText.Text = "The phone is in Flash mode. You can continue to flash the FFU image.";
                }
                else
                {
                    StatusText.Text = "You have to connect your phone before you can continue.";
                }

                Bindings.Update();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DeviceManager.Device.UFPConnected)
            {
                StatusText.Text = "The phone is in Flash mode. You can continue to flash the FFU image.";
            }
            else
            {
                StatusText.Text = "You have to connect your phone before you can continue.";
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
