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
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Pages
{
    public sealed partial class FlashPage : Page
    {
        public string SelectedFFUPath { get; set; } = "";

        public FlashPage()
        {
            InitializeComponent();
        }

        private static string GetPlatformID()
        {
            string PlatformID = "UNKNOWN.UNKNOWN.UNKNOWN.UNKNOWN";

            switch (DeviceManager.Device.Product)
            {
                case DeviceProduct.Epsilon:
                    {
                        PlatformID = DeviceManager.OEMEP_PLATFORMID;
                        break;
                    }
                case DeviceProduct.Zeta:
                    {
                        PlatformID = DeviceManager.OEMZE_MMWAVE_PLATFORMID;
                        break;
                    }
            }

            if (DeviceManager.Device.IsInUFP)
            {
                PlatformID = DeviceManager.Device.UnifiedFlashingPlatformTransport.ReadDevicePlatformID();
            }

            return PlatformID;
        }

        private static string[] GetFFUPlatformIDs(string SelectedFFUPath)
        {
            using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
            SignedImage signedImage = new(FFUStream);
            return signedImage.Image.Stores[0].PlatformIDs.ToArray();
        }

        private static bool IsCompatible(string SelectedFFUPath)
        {
            bool compatible = false;

            string DevicePlatformID = GetPlatformID();
            string[] DevicePlatformIDParts = DevicePlatformID.Split(".");

            string[] FFUPlatformIDs = GetFFUPlatformIDs(SelectedFFUPath);

            foreach (string FFUPlatformID in FFUPlatformIDs)
            {
                string[] FFUPlatformIDParts = FFUPlatformID.Split(".");

                if (FFUPlatformIDParts.Length <= DevicePlatformIDParts.Length)
                {
                    bool isMatching = true;

                    for (int i = 0; i < FFUPlatformIDParts.Length; i++)
                    {
                        string FFUPlatformIDPart = FFUPlatformIDParts[i];
                        string DevicePlatformIDPart = DevicePlatformIDParts[i];

                        if (FFUPlatformIDPart == "*")
                        {
                            continue;
                        }

                        if (FFUPlatformIDPart != DevicePlatformIDPart)
                        {
                            isMatching = false;
                            break;
                        }
                    }

                    if (isMatching)
                    {
                        compatible = true;
                        break;
                    }
                }
            }

            return compatible;
        }

        private void RefreshStatus(string FFUPath, Device device)
        {
            bool DeviceConnected = device.IsInUFP || device.IsADBEnabled || device.IsFastBootEnabled;

            if (!File.Exists(FFUPath))
            {
                FFUPath = "";
                SelectRun.Text = "Select the FFU-file to flash to the phone...";
                FlashFFUImageButton.IsEnabled = false;
            }
            else
            {
                SelectRun.Text = "Change";
                FlashFFUImageButton.IsEnabled = DeviceConnected;
            }

            if (device.IsInUFP)
            {
                StatusText.Text = "The phone is in Flash mode. You can continue to flash the FFU image.";
            }
            else if (device.IsADBEnabled)
            {
                StatusText.Text = "The phone is in Android mode. You can continue to flash the FFU image.";
            }
            else if (device.IsFastBootEnabled)
            {
                StatusText.Text = "The phone is in Fastboot mode. You can continue to flash the FFU image.";
            }
            else
            {
                StatusText.Text = "You have to connect your phone before you can continue.";
            }

            if (File.Exists(FFUPath) && DeviceConnected)
            {
                FlashFFUImageButton.IsEnabled = false;

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        bool compatible = IsCompatible(FFUPath);

                        _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                        {
                            if (!compatible)
                            {
                                StatusText.Text = $"The FFU file you selected is not intended for this device (FFU Platform ID(s): {string.Join(";", GetFFUPlatformIDs(FFUPath))}, Device Platform ID: {GetPlatformID()}). Please specify a compatible FFU file.";
                                FlashFFUImageButton.IsEnabled = false;
                            }
                            else
                            {
                                FlashFFUImageButton.IsEnabled = true;
                            }
                        });
                    }
                    catch
                    {
                        _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                        {
                            StatusText.Text = "The FFU file you selected is either invalid, corrupt, or not intended for this device. Please specify a compatible FFU file.";
                            FlashFFUImageButton.IsEnabled = false;
                        });
                    }
                });
            }
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

            RefreshStatus(SelectedFFUPath, DeviceManager.Device);

            Bindings.Update();
        }

        private static async Task SwitchToUFPAsync()
        {
            if (DeviceManager.Device.IsADBEnabled)
            {
                await DeviceRebootHelper.RebootToBootloaderAndWait();
            }

            if (DeviceManager.Device.IsFastBootEnabled)
            {
                DeviceManager.Device.FastBootTransport.GetVariable("partition-type:esp", out string espPartitionType);

                if (!DeviceManager.Device.FastBootTransport.ErasePartition("esp"))
                {
                    throw new Exception("Unable to erase ESP partition!");
                }

                await DeviceRebootHelper.RebootToUEFI();

                while (DeviceManager.Device.State is not DeviceState.UFP)
                {
                    await Task.Delay(1000);
                }
            }
        }

        private static void FlashFFUImageWithUI(string FFUPath)
        {
            if (!File.Exists(FFUPath))
            {
                return;
            }

            MainPage.SetStatus("Initializing...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

            using FileStream FFUStream = File.OpenRead(FFUPath);
            SignedImage signedImage = new(FFUStream);
            int chunkSize = signedImage.ChunkSize;
            ulong totalChunkCount = (ulong)FFUStream.Length / (ulong)chunkSize;

            FFUStream.Seek(0, SeekOrigin.Begin);

            ProgressUpdater updater = MainPage.GetProgressUpdater(totalChunkCount, $"Flashing {Path.GetFileName(FFUPath)}...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                using FileStream FFUStream = File.OpenRead(FFUPath);

                try
                {
                    await SwitchToUFPAsync();

                    DeviceManager.Device.UnifiedFlashingPlatformTransport.FlashFFU(FFUStream, updater);

                    MainPage.SetStatus("Rebooting Phone...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                    while (DeviceManager.Device.State == DeviceState.UFP || DeviceManager.Device.State == DeviceState.DISCONNECTED)
                    {
                        await Task.Delay(1000);
                    }

                    if (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                    {
                        MainPage.SetStatus("Escaping Bootloader Menu...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                        DeviceManager.Device.FastBootTransport.SetActiveOther();
                        DeviceManager.Device.FastBootTransport.SetActiveOther();
                        DeviceManager.Device.FastBootTransport.ContinueBoot();

                        while (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                        {
                            await Task.Delay(1000);
                        }

                        MainPage.SetStatus("Booting Phone...", Title: "Flashing FFU", SubTitle: "WOA Device Manager is currently flashing your device with the FFU file you previously selected. Make sure your phone remains plugged in throughout the entire process, that your computer does not go to sleep, nor that this window gets closed. This may take a while.", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.", Emoji: "🔧");

                        while (DeviceManager.Device.State == DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (WPinternalsException ex)
                {
                    MainPage.ShowDialog(ex.Message, ex.SubMessage);
                }
                catch (Exception ex)
                {
                    MainPage.ShowDialog(ex.Message);
                }

                MainPage.SetStatus();
            });
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

            FlashFFUImageWithUI(SelectedFFUPath);
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                RefreshStatus(SelectedFFUPath, device);
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                RefreshStatus(SelectedFFUPath, device);
                Bindings.Update();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DeviceManager.Device.IsInUFP)
            {
                StatusText.Text = "The phone is in Flash mode. You can continue to flash the FFU image.";
            }
            else if (DeviceManager.Device.IsADBEnabled)
            {
                StatusText.Text = "The phone is in Android mode. You can continue to flash the FFU image.";
            }
            else if (DeviceManager.Device.IsFastBootEnabled)
            {
                StatusText.Text = "The phone is in Fastboot mode. You can continue to flash the FFU image.";
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
