using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Helpers
{
    internal class FastBootProcedures
    {
        public static string GetProduct()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("product", out string productGetVar);
            return !result ? null : productGetVar;
        }

        public static void Reboot()
        {
            _ = DeviceManager.Device.FastBootTransport.Reboot();
        }

        public static void RebootBootloader()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootBootloader();
        }

        public static void RebootRecovery()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootRecovery();
        }

        public static void RebootFastBootD()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootFastBootD();
        }

        public static bool IsUnlocked()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("unlocked", out string unlockedVariable);
            if (!result)
            {
                return false;
            }

            return unlockedVariable == "yes";
        }

        public static bool CanUnlock()
        {
            bool result = DeviceManager.Device.FastBootTransport.FlashingGetUnlockAbility(out bool canUnlock);
            if (!result)
            {
                return false;
            }

            return canUnlock;
        }

        public static async void FlashUnlock(Control frameHost = null)
        {
            if (DeviceManager.Device.CanUnlock)
            {
                ContentDialog dialog = new()
                {
                    Title = "⚠️ EVERYTHING WILL BE FORMATTED",
                    Content = "Flash unlocking requires everything to be formatted. MAKE SURE YOU HAVE MADE A COPY OF EVERYTHING. We're not responsible for data loss.",
                    PrimaryButtonText = "⚠️ Proceed",
                    CloseButtonText = "Cancel"
                };

                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    new Task(async () =>
                    {
                        MainPage.SetStatus("Initializing...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                        while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
                        {
                            MainPage.SetStatus("Rebooting the device to Bootloader mode...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                            try
                            {
                                await DeviceRebootHelper.RebootToBootloaderAndWait();
                            }
                            catch { }
                        }

                        MainPage.SetStatus("Waiting for User to accept the prompt on the device.", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your device bootloader", SubMessage: "Use your volume buttons to go up and down, and your power button to confirm.");

                        bool result = DeviceManager.Device.FastBootTransport.FlashingUnlock();

                        while (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                        {
                            Thread.Sleep(300);
                        }

                        MainPage.SetStatus("Device is going to reboot in a moment...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                        while (DeviceManager.Device.State == DeviceState.DISCONNECTED)
                        {
                            Thread.Sleep(300);
                        }

                        MainPage.SetStatus();
                    }).Start();
                }
            }
            else
            {
                ContentDialog dialog = new()
                {
                    Title = "Unlocking is disabled",
                    Content = "Flash Unlocking is disabled from Developer Settings in Android. Please enable it manually from there.",
                    CloseButtonText = "OK"
                };

                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }

                await dialog.ShowAsync();
            }
        }

        public static async void FlashLock(Control frameHost = null)
        {
            // TODO: Check that the device doesn't have Windows installed
            ContentDialog dialog = new()
            {
                Title = "⚠️ Your bootloader will be locked",
                Content = "This procedure will lock your bootloader. You usually don't want to do this unless you have to sell your device.",
                PrimaryButtonText = "⚠️ Proceed",
                CloseButtonText = "Cancel"
            };

            if (frameHost != null)
            {
                dialog.XamlRoot = frameHost.XamlRoot;
            }

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                new Task(async () =>
                {
                    MainPage.SetStatus("Initializing...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                    while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
                    {
                        MainPage.SetStatus("Rebooting the device to Bootloader mode...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                        try
                        {
                            await DeviceRebootHelper.RebootToBootloaderAndWait();
                        }
                        catch { }
                    }

                    MainPage.SetStatus("Waiting for User to accept the prompt on the device.", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your device bootloader", SubMessage: "Use your volume buttons to go up and down, and your power button to confirm.");

                    bool result = DeviceManager.Device.FastBootTransport.FlashingLock();

                    while (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                    {
                        Thread.Sleep(300);
                    }

                    MainPage.SetStatus("Device is going to reboot in a moment...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your device bootloader", SubMessage: "Your device may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                    while (DeviceManager.Device.State == DeviceState.DISCONNECTED)
                    {
                        Thread.Sleep(300);
                    }

                    MainPage.SetStatus();
                }).Start();
            }
        }

        public static async Task<bool> BootTWRP()
        {
            try
            {
                await new HttpClient().GetAsync("https://github.com");
            }
            catch
            {
                throw new Exception("Your computer is offline! We need to be able to reach the internet in order to download the TWRP image.");
            }

            MainPage.SetStatus("Downloading TWRP...", Emoji: "⬇️");

            StorageFile? TWRP = null;

            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                TWRP = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_EPSILON, true);
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                TWRP = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_ZETA, true);
            }
            else
            {
                throw new Exception("Unknown device product");
            }

            if (TWRP == null)
            {
                throw new Exception("Unknown file path");
            }

            MainPage.SetStatus("Rebooting the device to TWRP mode...", Emoji: "🔄️");

            return DeviceManager.Device.FastBootTransport.BootImageIntoRam(TWRP.Path);
        }

        public static async Task<bool> BootUEFI(string? UEFIFile = null)
        {
            if (string.IsNullOrEmpty(UEFIFile))
            {
                try
                {
                    await new HttpClient().GetAsync("https://github.com");
                }
                catch
                {
                    throw new Exception("Your computer is offline! We need to be able to reach the internet in order to download the UEFI.");
                }

                MainPage.SetStatus("Downloading UEFI...", Emoji: "⬇️");

                if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
                {
                    StorageFile UEFI = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_EPSILON, true);
                    if (UEFI == null)
                    {
                        return false;
                    }

                    string destinationPath = (await UEFI.GetParentAsync()).Path;
                    ZipFile.ExtractToDirectory(UEFI.Path, destinationPath, true);

                    UEFIFile = $"{destinationPath}\\Surface Duo (1st Gen) UEFI (Fast Boot)\\uefi.img";
                }
                else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
                {
                    StorageFile UEFI = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_ZETA, true);
                    if (UEFI == null)
                    {
                        return false;
                    }

                    string destinationPath = (await UEFI.GetParentAsync()).Path;
                    ZipFile.ExtractToDirectory(UEFI.Path, destinationPath, true);

                    UEFIFile = $"{destinationPath}\\Surface Duo 2 UEFI (Fast Boot)\\uefi.img";
                }
                else
                {
                    throw new Exception("Unknown device product");
                }

                MainPage.SetStatus("Rebooting the device to Windows mode...", Emoji: "🔄️");
            }

            return DeviceManager.Device.FastBootTransport.BootImageIntoRam(UEFIFile);
        }

        public static string? GetDeviceBatteryLevel()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("battery-level", out string batteryLevel);
            return result ? batteryLevel : null;
        }
    }
}
