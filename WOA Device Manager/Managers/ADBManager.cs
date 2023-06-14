using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace WOADeviceManager.Managers
{
    class ADBManager
    {
        public static string GetADBPath()
        {
            if (!string.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["CustomADBPath"] as string))
            {
                return $"/C \"\"{ApplicationData.Current.LocalSettings.Values["CustomADBPath"]}\"";
            }
            else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "path")
            {
                return $"/C adb";
            }
            else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "downloaded")
            {
                return $"/C \"\"{ApplicationData.Current.LocalSettings.Values["adbPath"]}\\adb.exe\"";
            }
            throw new Exception("No valid ADB available");
        }

        public static Process GetADBProcess(string argument = null)
        {
            Process process = new Process();
            string args = "";
            if (!string.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["CustomADBPath"] as string))
            {
                args = $"/C \"\"{ApplicationData.Current.LocalSettings.Values["CustomADBPath"]}\" {argument}\"";
            }
            else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "path")
            {
                args = $"/C adb {argument}";
            }
            else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "downloaded")
            {
                args = $"/C \"\"{ApplicationData.Current.LocalSettings.Values["adbPath"]}\\adb.exe\" {argument}\"";
            }
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = $@"cmd.exe",
                Arguments = argument != null ? args : null,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.Start();
            return process;
        }

        public static Process GetADBShell()
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = "adb",
                Arguments = $"shell"
            };
            process.Start();
            return process;
        }

        public static async Task<string> SendADBCommand(string command)
        {
            Process shell = GetADBProcess(command);

            StringBuilder sb = new StringBuilder();

            Thread.Sleep(250);

            while (shell.StandardOutput.EndOfStream == false || shell.StandardError.EndOfStream == false)
            {
                if (shell.StandardOutput.EndOfStream == false) sb.AppendLine(shell.StandardOutput.ReadLine());
                if (shell.StandardError.EndOfStream == false) sb.AppendLine(shell.StandardError.ReadLine());
            }

            shell.Close();

            return sb.ToString();
        }
    }
}
