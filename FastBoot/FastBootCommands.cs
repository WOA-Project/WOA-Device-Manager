using System.IO;
using System.Linq;

namespace FastBoot
{
    public static class FastBootCommands
    {
        /*
         * Supported commands on Surface Duo (1st Gen) LinuxLoader (ShipMode ON):
         * 
         *  boot                                       DONE
         *  continue                                   DONE
         *  download:                                  DONE
         *  erase:                                     DONE
         *  flash:                                     DONE (TODO: Verify if large files are ok, very unsure here)
         *  flashing get_unlock_ability                DONE
         *  flashing lock                              DONE
         *  flashing unlock                            DONE
         *  getvar:                                    DONE
         *  reboot-bootloader                          DONE
         *  reboot                                     DONE
         *  set_active                                 DONE
         *  snapshot-update
         *  oem battery-rnrmode
         *  oem battery-shipmode
         *  oem clear-devinforollback
         *  oem clear-display-factory-kernel-param
         *  oem clear-mfg-mode
         *  oem clear-sfpd-tamper
         *  oem del-boot-prop
         *  oem device-info
         *  oem disable_act
         *  oem disable-charger-screen
         *  oem enable_act
         *  oem enable-charger-screen
         *  oem get-boot-prop
         *  oem get-display-factory-mode
         *  oem get-mfg-blob
         *  oem get-mfg-mode
         *  oem get-mfg-values
         *  oem get-oem-keys
         *  oem get-sfpd-tamper
         *  oem get-soc-serial
         *  oem select-display-panel
         *  oem set-boot-prop
         *  oem set-display-factory-kernel-param
         *  oem set-oem-keys
         *  oem set-successful
         *  oem show-devinfo
         *  oem show-hw-devinfo
         *  oem touch-fw-version
        */

        public static bool BootImageIntoRam(this FastBootTransport fastBootTransport, Stream stream)
        {
            FastBootStatus status;

            try
            {
                (status, string _, byte[] _) = fastBootTransport.SendData(stream);
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand("boot");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool BootImageIntoRam(this FastBootTransport fastBootTransport, string filePath)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            return BootImageIntoRam(fastBootTransport, fileStream);
        }

        public static bool GetVariable(this FastBootTransport fastBootTransport, string variable, out string response)
        {
            bool result = GetVariable(fastBootTransport, variable, out string[] responses);
            response = string.Join("\n", responses);
            return result;
        }

        public static bool GetVariable(this FastBootTransport fastBootTransport, string variable, out string[] response)
        {
            (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"getvar:{variable}");
            (FastBootStatus status, string _, byte[] _) = responses.Last();

            response = responses.Select(response => response.response).ToArray();

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool GetAllVariables(this FastBootTransport fastBootTransport, out string variablesResponse)
        {
            return GetVariable(fastBootTransport, "all", out variablesResponse);
        }

        public static bool GetAllVariables(this FastBootTransport fastBootTransport, out string[] variablesResponses)
        {
            return GetVariable(fastBootTransport, "all", out variablesResponses);
        }

        public static bool GetAllVariables(this FastBootTransport fastBootTransport, out (string, string)[] variables)
        {
            bool result = GetAllVariables(fastBootTransport, out string[] responses);
            variables = responses.Where(t => t.Contains(':')).Select(t => (t.Split(":")[0], string.Join(":", t.Split(":").Skip(1)))).ToArray();
            return result;
        }

        public static bool ContinueBoot(this FastBootTransport fastBootTransport)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand("continue");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool Reboot(this FastBootTransport fastBootTransport, string mode)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"reboot-{mode}");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool Reboot(this FastBootTransport fastBootTransport)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"reboot");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool RebootRecovery(this FastBootTransport fastBootTransport)
        {
            return Reboot(fastBootTransport, "recovery");
        }

        public static bool RebootFastBootD(this FastBootTransport fastBootTransport)
        {
            return Reboot(fastBootTransport, "fastboot");
        }

        public static bool RebootBootloader(this FastBootTransport fastBootTransport)
        {
            return Reboot(fastBootTransport, "bootloader");
        }

        public static bool FlashPartition(this FastBootTransport fastBootTransport, string partition, Stream stream)
        {
            FastBootStatus status;

            try
            {
                (status, string _, byte[] _) = fastBootTransport.SendData(stream);
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"flash:{partition}");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool FlashPartition(this FastBootTransport fastBootTransport, string partition, string filePath)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            return FlashPartition(fastBootTransport, partition, fileStream);
        }

        public static bool ErasePartition(this FastBootTransport fastBootTransport, string partition)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"erase:{partition}");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool PowerDown(this FastBootTransport fastBootTransport)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"powerdown");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool SetActive(this FastBootTransport fastBootTransport, string slot)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"set_active:{slot}");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool SetActiveOther(this FastBootTransport fastBootTransport)
        {
            bool result = GetVariable(fastBootTransport, "current-slot", out string CurrentSlot);
            if (!result)
            {
                return false;
            }

            if (CurrentSlot == "a")
            {
                return SetActiveB(fastBootTransport);
            }

            if (CurrentSlot == "b")
            {
                return SetActiveA(fastBootTransport);
            }

            return false;
        }

        public static bool SetActiveA(this FastBootTransport fastBootTransport)
        {
            return SetActive(fastBootTransport, "a");
        }

        public static bool SetActiveB(this FastBootTransport fastBootTransport)
        {
            return SetActive(fastBootTransport, "b");
        }

        public static bool FlashingGetUnlockAbility(this FastBootTransport fastBootTransport, out bool canUnlock)
        {
            FastBootStatus status;
            string response;
            canUnlock = false;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"flashing get_unlock_ability");
                (status, response, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            // Sample response:
            // get_unlock_ability: 1

            if (response.StartsWith("get_unlock_ability: "))
            {
                canUnlock = response.EndsWith('1');
                return true;
            }

            return false;
        }

        public static bool FlashingLock(this FastBootTransport fastBootTransport)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"flashing lock");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }

        public static bool FlashingUnlock(this FastBootTransport fastBootTransport)
        {
            FastBootStatus status;

            try
            {
                (FastBootStatus status, string response, byte[] rawResponse)[] responses = fastBootTransport.SendCommand($"flashing unlock");
                (status, string _, byte[] _) = responses.Last();
            }
            catch
            {
                return false;
            }

            if (status != FastBootStatus.OKAY)
            {
                return false;
            }

            return true;
        }
    }
}
