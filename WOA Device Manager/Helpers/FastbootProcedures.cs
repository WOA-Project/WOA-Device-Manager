using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO.Compression;
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
                        MainPage.SetStatus("Initializing...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                        while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
                        {
                            MainPage.SetStatus("Rebooting phone to Bootloader mode...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                            try
                            {
                                await DeviceRebootHelper.RebootToBootloaderAndWait();
                            }
                            catch { }
                        }

                        MainPage.SetStatus("Waiting for User to accept the prompt on the phone.", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your phone bootloader", SubMessage: "Use your volume buttons to go up and down, and your power button to confirm.");

                        bool result = DeviceManager.Device.FastBootTransport.FlashingUnlock();

                        while (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                        {
                            Thread.Sleep(300);
                        }

                        MainPage.SetStatus("Device is going to reboot in a moment...", Emoji: "🔓", Title: "Unlocking Bootloader", SubTitle: "WOA Device Manager is preparing to unlock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

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
                    MainPage.SetStatus("Initializing...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                    while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
                    {
                        MainPage.SetStatus("Rebooting phone to Bootloader mode...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

                        try
                        {
                            await DeviceRebootHelper.RebootToBootloaderAndWait();
                        }
                        catch { }
                    }

                    MainPage.SetStatus("Waiting for User to accept the prompt on the phone.", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your phone bootloader", SubMessage: "Use your volume buttons to go up and down, and your power button to confirm.");

                    bool result = DeviceManager.Device.FastBootTransport.FlashingLock();

                    while (DeviceManager.Device.State == DeviceState.BOOTLOADER)
                    {
                        Thread.Sleep(300);
                    }

                    MainPage.SetStatus("Device is going to reboot in a moment...", Emoji: "🔒", Title: "Locking Bootloader", SubTitle: "WOA Device Manager is preparing to lock your phone bootloader", SubMessage: "Your phone may reboot into different operating modes. This is expected behavior. Do not interfere with this process.");

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
            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_EPSILON, true);
                return twrp != null && DeviceManager.Device.FastBootTransport.BootImageIntoRam(twrp.Path);
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_ZETA, true);
                return twrp != null && DeviceManager.Device.FastBootTransport.BootImageIntoRam(twrp.Path);
            }
            else
            {
                throw new Exception("Unknown device product");
            }
        }

        public static async Task<bool> BootUEFI()
        {
            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                StorageFile uefi = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_EPSILON, true);
                if (uefi == null)
                {
                    return false;
                }

                string destinationPath = (await uefi.GetParentAsync()).Path;
                ZipFile.ExtractToDirectory(uefi.Path, destinationPath, true);

                return DeviceManager.Device.FastBootTransport.BootImageIntoRam($"{destinationPath}\\Surface Duo (1st Gen) UEFI (Fast Boot)\\uefi.img");
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                StorageFile uefi = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_ZETA, true);
                if (uefi == null)
                {
                    return false;
                }

                string destinationPath = (await uefi.GetParentAsync()).Path;
                ZipFile.ExtractToDirectory(uefi.Path, destinationPath, true);

                return DeviceManager.Device.FastBootTransport.BootImageIntoRam($"{destinationPath}\\Surface Duo 2 UEFI (Fast Boot)\\uefi.img");
            }
            else
            {
                throw new Exception("Unknown device product");
            }
        }

        public static string GetDeviceBatteryLevel()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("battery-level", out string batteryLevel);
            return result ? batteryLevel : null;
        }
    }
}
