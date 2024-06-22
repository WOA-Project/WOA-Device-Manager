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

            SetStatus("Initializing...");

            using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
            SignedImage signedImage = new(FFUStream);
            int chunkSize = signedImage.ChunkSize;
            ulong totalChunkCount = (ulong)FFUStream.Length / (ulong)chunkSize;

            FFUStream.Seek(0, SeekOrigin.Begin);

            UnifiedFlashingPlatformTransport.ProgressUpdater updater = GetProgressUpdater(totalChunkCount, "Flashing FFU...");

            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                using FileStream FFUStream = File.OpenRead(SelectedFFUPath);
                DeviceManager.Device.UnifiedFlashingPlatformTransport.FlashFFU(FFUStream, updater);

                SetStatus("Rebooting Phone...");

                while (DeviceManager.Device.State == Device.DeviceStateEnum.UFP || DeviceManager.Device.State == Device.DeviceStateEnum.DISCONNECTED)
                {
                    await Task.Delay(1000);
                }

                if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
                {
                    SetStatus("Escaping Bootloader Menu...");

                    DeviceManager.Device.FastBootTransport.SetActiveOther();
                    DeviceManager.Device.FastBootTransport.SetActiveOther();
                    DeviceManager.Device.FastBootTransport.ContinueBoot();

                    while (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
                    {
                        await Task.Delay(1000);
                    }

                    SetStatus("Booting Phone...");

                    while (DeviceManager.Device.State == Device.DeviceStateEnum.DISCONNECTED)
                    {
                        await Task.Delay(1000);
                    }
                }

                SetStatus();
            });
        }

        private UnifiedFlashingPlatformTransport.ProgressUpdater GetProgressUpdater(ulong MaxValue, string Message, string? SubMessage = null)
        {
            return new(MaxValue, (percentage, eta) =>
            {
                _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
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
                        SetStatus(Message, (uint)percentage, NewText, SubMessage);
                    }
                    else
                    {
                        SetStatus("Initializing...");
                    }
                });
            });
        }

        private void SetStatus(string? Message = null, uint? Percentage = null, string? Text = null, string? SubMessage = null)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                if (Message == null && Percentage == null && Text == null && SubMessage == null)
                {
                    ProgressOverlay.Visibility = Visibility.Collapsed;
                    FlashFFUPanel.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    FlashFFUPanel.Visibility = Visibility.Collapsed;
                    ProgressOverlay.Visibility = Visibility.Visible;
                }

                if (Message != null)
                {
                    ProgressMessage.Text = Message;
                    ProgressMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressMessage.Visibility = Visibility.Collapsed;
                }

                if (Percentage != null)
                {
                    LoadingRing.Visibility = Visibility.Collapsed;

                    ProgressPercentageBar.Maximum = 100;
                    ProgressPercentageBar.Minimum = 0;
                    ProgressPercentageBar.Value = (int)Percentage;

                    ProgressPercentageBar.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressPercentageBar.Visibility = Visibility.Collapsed;
                    LoadingRing.Visibility = Visibility.Visible;
                }

                if (Text != null)
                {
                    ProgressText.Text = Text;
                    ProgressText.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressText.Visibility = Visibility.Collapsed;
                }

                if (SubMessage != null)
                {
                    ProgressSubMessage.Text = SubMessage;
                    ProgressSubMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressSubMessage.Visibility = Visibility.Collapsed;
                }
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
    }
}
