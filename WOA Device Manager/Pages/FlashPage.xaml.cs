using FastBoot;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Windows.Forms;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private async void FlashFFUImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(SelectedFFUPath))
            {
                SelectedFFUPath = "";
                SelectRun.Text = "Select the FFU-file to flash to the phone...";
                FlashFFUImageButton.IsEnabled = false;
                Bindings.Update();
                return;
            }

            LoadingRing.Visibility = Visibility.Visible;
            ProgressMessage.Text = "Initializing...";

            ProgressPercentageBar.Visibility = Visibility.Collapsed;
            ProgressSubMessage.Visibility = Visibility.Collapsed;
            ProgressText.Visibility = Visibility.Collapsed;

            FlashFFUPanel.Visibility = Visibility.Collapsed;
            ProgressOverlay.Visibility = Visibility.Visible;

            using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
            Img2Ffu.Reader.Data.SignedImage signedImage = new(FFUStream);
            int chunkSize = signedImage.ChunkSize;
            ulong totalChunkSize = (ulong)FFUStream.Length / (ulong)chunkSize;

            FFUStream.Seek(0, SeekOrigin.Begin);

            UnifiedFlashingPlatform.UnifiedFlashingPlatformTransport.ProgressUpdater updater = new(totalChunkSize, (percentage, eta) =>
            {
                _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    string NewText = null;
                    if (percentage != null)
                    {
                        NewText = "Progress: " + ((int)percentage).ToString() + "%";
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

                        NewText += "Estimated time remaining: " + ((TimeSpan)eta).ToString(@"h\:mm\:ss");
                    }

                    if (NewText != null)
                    {
                        LoadingRing.Visibility = Visibility.Collapsed;
                        ProgressSubMessage.Visibility = Visibility.Collapsed;

                        ProgressPercentageBar.Visibility = Visibility.Visible;
                        ProgressPercentageBar.Maximum = 100;
                        ProgressPercentageBar.Minimum = 0;

                        ProgressText.Visibility = Visibility.Visible;

                        ProgressMessage.Text = "Flashing FFU...";
                        ProgressText.Text = NewText;
                        ProgressPercentageBar.Value = percentage;
                    }
                    else
                    {
                        LoadingRing.Visibility = Visibility.Visible;
                        ProgressMessage.Text = "Initializing...";

                        ProgressPercentageBar.Visibility = Visibility.Collapsed;
                        ProgressSubMessage.Visibility = Visibility.Collapsed;
                        ProgressText.Visibility = Visibility.Collapsed;
                    }
                });
            });

            ThreadPool.QueueUserWorkItem((o) =>
            {
                using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
                DeviceManager.Device.UnifiedFlashingPlatformTransport.FlashFFU(FFUStream, updater, true, 0);

                DeviceManager.DeviceConnectedEvent += BootloaderMenuEscape_DeviceConnectedEvent;

                _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    LoadingRing.Visibility = Visibility.Visible;
                    ProgressMessage.Text = "Initializing...";

                    ProgressPercentageBar.Visibility = Visibility.Collapsed;
                    ProgressSubMessage.Visibility = Visibility.Collapsed;
                    ProgressText.Visibility = Visibility.Collapsed;

                    ProgressOverlay.Visibility = Visibility.Collapsed;
                    FlashFFUPanel.Visibility = Visibility.Visible;
                });
            });
        }

        private void BootloaderMenuEscape_DeviceConnectedEvent(object sender, Device device)
        {
            EscapeBootloaderMenu();
            DeviceManager.DeviceConnectedEvent -= BootloaderMenuEscape_DeviceConnectedEvent;
        }

        private void EscapeBootloaderMenu()
        {
            if (!DeviceManager.Device.BootloaderConnected)
            {
                return;
            }

            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                LoadingRing.Visibility = Visibility.Visible;
                ProgressMessage.Text = "Escaping Bootloader Menu...";

                ProgressPercentageBar.Visibility = Visibility.Collapsed;
                ProgressSubMessage.Visibility = Visibility.Collapsed;
                ProgressText.Visibility = Visibility.Collapsed;

                FlashFFUPanel.Visibility = Visibility.Collapsed;
                ProgressOverlay.Visibility = Visibility.Visible;
            });

            DeviceManager.Device.FastBootTransport.SetActiveOther();
            DeviceManager.Device.FastBootTransport.SetActiveOther();
            DeviceManager.Device.FastBootTransport.ContinueBoot();

            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                LoadingRing.Visibility = Visibility.Visible;
                ProgressMessage.Text = "Initializing...";

                ProgressPercentageBar.Visibility = Visibility.Collapsed;
                ProgressSubMessage.Visibility = Visibility.Collapsed;
                ProgressText.Visibility = Visibility.Collapsed;

                ProgressOverlay.Visibility = Visibility.Collapsed;
                FlashFFUPanel.Visibility = Visibility.Visible;
            });
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
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
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
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
    }
}
