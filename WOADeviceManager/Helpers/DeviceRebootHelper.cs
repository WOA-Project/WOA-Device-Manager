using FastBoot;
using System;
using System.Threading.Tasks;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Helpers
{
    public class DeviceRebootHelper
    {
        private static void EnsureBootability()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("current-slot", out string currentSlot);
            if (!result)
            {
                // Device may not have slot support, ignore then.
                //throw new Exception("Error while getting current slot.");
            }
            else
            {
                result = DeviceManager.Device.FastBootTransport.GetVariable("slot-successful:" + currentSlot, out string successSlotStr);
                if (!result)
                {
                    throw new Exception("Error while getting slot " + currentSlot + " successful state.");
                }

                bool successSlot = successSlotStr == "yes";

                if (!successSlot)
                {
                    result = DeviceManager.Device.FastBootTransport.SetActiveOther();
                    if (!result)
                    {
                        throw new Exception("Error while setting other slot active state.");
                    }

                    result = DeviceManager.Device.FastBootTransport.SetActiveOther();
                    if (!result)
                    {
                        throw new Exception("Error while setting current slot active state.");
                    }
                }
            }
        }

        public static async Task RebootToBootloaderAndWait()
        {
            if (DeviceManager.Device.State == DeviceState.BOOTLOADER)
            {
                return;
            }

            switch (DeviceManager.Device.State)
            {
                case DeviceState.UEFI:
                case DeviceState.FASTBOOTD:
                    {
                        EnsureBootability();

                        bool result = DeviceManager.Device.FastBootTransport.RebootBootloader();
                        if (!result)
                        {
                            throw new Exception("Error while rebooting.");
                        }

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.UFP:
                    {
                        MainPage.SetStatus("First rebooting the device from UFP mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Bootloader mode...");

                        UFPProcedures.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        // We will then end up in one state, so call us again (we cant directly go from UFP to something else here).
                        while (DeviceManager.Device.State is DeviceState.ANDROID_ADB_DISABLED
                                                            or DeviceState.ANDROID
                                                            or DeviceState.OFFLINE_CHARGING
                                                            or DeviceState.RECOVERY_ADB_DISABLED
                                                            or DeviceState.SIDELOAD_ADB_DISABLED
                                                            or DeviceState.TWRP_ADB_DISABLED
                                                            or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED
                                                            or DeviceState.WINDOWS
                                                            or DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        MainPage.SetStatus("Now rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Bootloader mode...");

                        await RebootToBootloaderAndWait();

                        return;
                    }
                case DeviceState.SIDELOAD_ADB_ENABLED:
                case DeviceState.TWRP_ADB_ENABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                case DeviceState.RECOVERY_ADB_ENABLED:
                case DeviceState.ANDROID_ADB_ENABLED:
                    {
                        DeviceManager.Device.AndroidDebugBridgeTransport.RebootBootloader();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.ANDROID_ADB_DISABLED:
                case DeviceState.ANDROID:
                case DeviceState.BOOTLOADER:
                case DeviceState.OFFLINE_CHARGING:
                case DeviceState.RECOVERY_ADB_DISABLED:
                case DeviceState.SIDELOAD_ADB_DISABLED:
                case DeviceState.TWRP_ADB_DISABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED:
                case DeviceState.WINDOWS:
                case DeviceState.DISCONNECTED:
                    {
                        throw new Exception($"Rebooting from {DeviceManager.Device.DeviceStateLocalized} to bootloader is still unsupported.");
                    }
            }

            while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToAndroidAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.ANDROID or DeviceState.ANDROID_ADB_ENABLED or DeviceState.ANDROID_ADB_DISABLED)
            {
                return;
            }

            switch (DeviceManager.Device.State)
            {
                case DeviceState.BOOTLOADER:
                    {
                        EnsureBootability();

                        bool result = DeviceManager.Device.FastBootTransport.ContinueBoot();
                        if (!result)
                        {
                            throw new Exception("Error while continuing the boot process.");
                        }

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.UEFI:
                case DeviceState.FASTBOOTD:
                    {
                        EnsureBootability();

                        bool result = DeviceManager.Device.FastBootTransport.Reboot();
                        if (!result)
                        {
                            throw new Exception("Error while rebooting.");
                        }

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.UFP:
                    {
                        UFPProcedures.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.SIDELOAD_ADB_ENABLED:
                case DeviceState.TWRP_ADB_ENABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                case DeviceState.RECOVERY_ADB_ENABLED:
                    {
                        DeviceManager.Device.AndroidDebugBridgeTransport.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.ANDROID_ADB_DISABLED:
                case DeviceState.ANDROID_ADB_ENABLED:
                case DeviceState.ANDROID:
                case DeviceState.OFFLINE_CHARGING:
                case DeviceState.RECOVERY_ADB_DISABLED:
                case DeviceState.SIDELOAD_ADB_DISABLED:
                case DeviceState.TWRP_ADB_DISABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED:
                case DeviceState.WINDOWS:
                case DeviceState.DISCONNECTED:
                    {
                        throw new Exception($"Rebooting from {DeviceManager.Device.DeviceStateLocalized} to bootloader is still unsupported.");
                    }
            }

            while (DeviceManager.Device.State is not DeviceState.ANDROID_ADB_ENABLED and not DeviceState.ANDROID and not DeviceState.ANDROID_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToTWRPAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_ADB_DISABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                return;
            }

            if (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                MainPage.SetStatus("First rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to TWRP mode...");

                await RebootToBootloaderAndWait();

                MainPage.SetStatus("Now rebooting the device to TWRP mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to TWRP mode...");
            }

            if (DeviceManager.Device.State is DeviceState.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootTWRP();

                while (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED and not DeviceState.TWRP_ADB_DISABLED)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task RebootToUEFI(string UEFIFile = null)
        {
            if (DeviceManager.Device.State is DeviceState.WINDOWS or DeviceState.UFP)
            {
                return;
            }

            if (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                MainPage.SetStatus("First rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Windows mode...");

                await RebootToBootloaderAndWait();

                MainPage.SetStatus("Now rebooting the device to Windows mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Windows mode...");
            }

            if (DeviceManager.Device.State is DeviceState.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootUEFI(UEFIFile);

                /*while (DeviceManager.Device.State is not DeviceState.WINDOWS and not DeviceState.UFP)
                {
                    await Task.Delay(1000);
                }*/
            }
        }

        public static async Task RebootToMSCAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                return;
            }

            if (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED)
            {
                await RebootToTWRPAndWait();

                while (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED)
                {
                    await Task.Delay(1000);
                }
            }

            await ADBProcedures.EnableMassStorageMode();

            while (DeviceManager.Device.State is not DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED and not DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToRecoveryAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.RECOVERY_ADB_ENABLED or DeviceState.RECOVERY_ADB_DISABLED or DeviceState.SIDELOAD_ADB_ENABLED or DeviceState.SIDELOAD_ADB_DISABLED)
            {
                return;
            }

            switch (DeviceManager.Device.State)
            {
                case DeviceState.UEFI:
                case DeviceState.FASTBOOTD:
                case DeviceState.BOOTLOADER:
                    {
                        EnsureBootability();

                        bool result = DeviceManager.Device.FastBootTransport.RebootRecovery();
                        if (!result)
                        {
                            throw new Exception("Error while rebooting.");
                        }

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.UFP:
                    {
                        MainPage.SetStatus("Now rebooting the device from UFP mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Recovery mode...");

                        UFPProcedures.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        // We will then end up in one state, so call us again (we cant directly go from UFP to something else here).
                        while (DeviceManager.Device.State is DeviceState.ANDROID_ADB_DISABLED
                                                            or DeviceState.ANDROID
                                                            or DeviceState.OFFLINE_CHARGING
                                                            or DeviceState.SIDELOAD_ADB_DISABLED
                                                            or DeviceState.TWRP_ADB_DISABLED
                                                            or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED
                                                            or DeviceState.WINDOWS
                                                            or DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        MainPage.SetStatus("Now rebooting the device to Recovery mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Recovery mode...");

                        await RebootToRecoveryAndWait();

                        return;
                    }
                case DeviceState.SIDELOAD_ADB_ENABLED:
                case DeviceState.ANDROID_ADB_ENABLED:
                    {
                        DeviceManager.Device.AndroidDebugBridgeTransport.RebootRecovery();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.TWRP_ADB_ENABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                    {
                        MainPage.SetStatus("First rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Recovery mode...");

                        // We cant switch to many things from TWRP, go to Bootloader first.
                        await RebootToBootloaderAndWait();

                        MainPage.SetStatus("Now rebooting the device to Recovery mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Recovery mode...");

                        await RebootToRecoveryAndWait();

                        return;
                    }
                case DeviceState.ANDROID_ADB_DISABLED:
                case DeviceState.ANDROID:
                case DeviceState.OFFLINE_CHARGING:
                case DeviceState.RECOVERY_ADB_DISABLED:
                case DeviceState.RECOVERY_ADB_ENABLED:
                case DeviceState.SIDELOAD_ADB_DISABLED:
                case DeviceState.TWRP_ADB_DISABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED:
                case DeviceState.WINDOWS:
                case DeviceState.DISCONNECTED:
                    {
                        throw new Exception($"Rebooting from {DeviceManager.Device.DeviceStateLocalized} to bootloader is still unsupported.");
                    }
            }

            while (DeviceManager.Device.State is not DeviceState.RECOVERY_ADB_ENABLED and not DeviceState.RECOVERY_ADB_DISABLED and not DeviceState.SIDELOAD_ADB_ENABLED and not DeviceState.SIDELOAD_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToSideloadAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.SIDELOAD_ADB_DISABLED or DeviceState.SIDELOAD_ADB_ENABLED)
            {
                return;
            }

            switch (DeviceManager.Device.State)
            {
                case DeviceState.UEFI:
                case DeviceState.FASTBOOTD:
                    {
                        MainPage.SetStatus("First rebooting the device to Android mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        await RebootToAndroidAndWait();

                        MainPage.SetStatus("Now rebooting the device to Sideload mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        await RebootToSideloadAndWait();

                        return;
                    }
                case DeviceState.UFP:
                    {
                        MainPage.SetStatus("First rebooting the device from UFP mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        UFPProcedures.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        // We will then end up in one state, so call us again (we cant directly go from UFP to something else here).
                        while (DeviceManager.Device.State is DeviceState.ANDROID_ADB_DISABLED
                                                            or DeviceState.ANDROID
                                                            or DeviceState.BOOTLOADER
                                                            or DeviceState.OFFLINE_CHARGING
                                                            or DeviceState.RECOVERY_ADB_DISABLED
                                                            or DeviceState.TWRP_ADB_DISABLED
                                                            or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED
                                                            or DeviceState.WINDOWS
                                                            or DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        MainPage.SetStatus("Now rebooting the device to Sideload mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        await RebootToSideloadAndWait();

                        return;
                    }
                case DeviceState.RECOVERY_ADB_ENABLED:
                case DeviceState.ANDROID_ADB_ENABLED:
                    {
                        DeviceManager.Device.AndroidDebugBridgeTransport.RebootSideload();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.TWRP_ADB_ENABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                    {
                        MainPage.SetStatus("First rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        // We cant switch to many things from TWRP, go to Bootloader first.
                        await RebootToBootloaderAndWait();

                        MainPage.SetStatus("Now rebooting the device to Sideload mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to Sideload mode...");

                        await RebootToSideloadAndWait();

                        return;
                    }
                case DeviceState.ANDROID_ADB_DISABLED:
                case DeviceState.ANDROID:
                case DeviceState.BOOTLOADER:
                case DeviceState.OFFLINE_CHARGING:
                case DeviceState.RECOVERY_ADB_DISABLED:
                case DeviceState.SIDELOAD_ADB_DISABLED:
                case DeviceState.SIDELOAD_ADB_ENABLED:
                case DeviceState.TWRP_ADB_DISABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED:
                case DeviceState.WINDOWS:
                case DeviceState.DISCONNECTED:
                    {
                        throw new Exception($"Rebooting from {DeviceManager.Device.DeviceStateLocalized} to bootloader is still unsupported.");
                    }
            }

            while (DeviceManager.Device.State is not DeviceState.RECOVERY_ADB_ENABLED and not DeviceState.RECOVERY_ADB_DISABLED and not DeviceState.SIDELOAD_ADB_ENABLED and not DeviceState.SIDELOAD_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToFastBootDAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.FASTBOOTD)
            {
                return;
            }

            switch (DeviceManager.Device.State)
            {
                case DeviceState.UEFI:
                case DeviceState.BOOTLOADER:
                    {
                        EnsureBootability();

                        bool result = DeviceManager.Device.FastBootTransport.RebootFastBootD();
                        if (!result)
                        {
                            throw new Exception("Error while rebooting.");
                        }

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.UFP:
                    {
                        MainPage.SetStatus("First rebooting the device from UFP mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to FastBootD mode...");

                        UFPProcedures.Reboot();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        // We will then end up in one state, so call us again (we cant directly go from UFP to something else here).
                        while (DeviceManager.Device.State is DeviceState.ANDROID_ADB_DISABLED
                                                            or DeviceState.ANDROID
                                                            or DeviceState.OFFLINE_CHARGING
                                                            or DeviceState.RECOVERY_ADB_DISABLED
                                                            or DeviceState.SIDELOAD_ADB_DISABLED
                                                            or DeviceState.TWRP_ADB_DISABLED
                                                            or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED
                                                            or DeviceState.WINDOWS
                                                            or DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        MainPage.SetStatus("Now rebooting the device to FastBootD mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to FastBootD mode...");

                        await RebootToFastBootDAndWait();

                        return;
                    }
                case DeviceState.RECOVERY_ADB_ENABLED:
                case DeviceState.SIDELOAD_ADB_ENABLED:
                case DeviceState.ANDROID_ADB_ENABLED:
                    {
                        DeviceManager.Device.AndroidDebugBridgeTransport.RebootFastBootD();

                        // We are meant to disconnect immediately, if not, wait.
                        while (DeviceManager.Device.State != DeviceState.DISCONNECTED)
                        {
                            await Task.Delay(1000);
                        }

                        break;
                    }
                case DeviceState.TWRP_ADB_ENABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                    {
                        MainPage.SetStatus("First rebooting the device to Bootloader mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to FastBootD mode...");

                        // We cant switch to many things from TWRP, go to Bootloader first.
                        await RebootToBootloaderAndWait();

                        MainPage.SetStatus("Now rebooting the device to FastBootD mode...", Emoji: "🔄️", SubMessage: "Rebooting the device to FastBootD mode...");

                        await RebootToFastBootDAndWait();

                        return;
                    }
                case DeviceState.ANDROID_ADB_DISABLED:
                case DeviceState.ANDROID:
                case DeviceState.FASTBOOTD:
                case DeviceState.OFFLINE_CHARGING:
                case DeviceState.RECOVERY_ADB_DISABLED:
                case DeviceState.SIDELOAD_ADB_DISABLED:
                case DeviceState.TWRP_ADB_DISABLED:
                case DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED:
                case DeviceState.WINDOWS:
                case DeviceState.DISCONNECTED:
                    {
                        throw new Exception($"Rebooting from {DeviceManager.Device.DeviceStateLocalized} to bootloader is still unsupported.");
                    }
            }

            while (DeviceManager.Device.State != DeviceState.FASTBOOTD)
            {
                await Task.Delay(1000);
            }
        }
    }
}
