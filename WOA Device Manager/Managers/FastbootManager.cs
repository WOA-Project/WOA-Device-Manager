using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WOADeviceManager.Managers
{
    class FastbootManager
    {
        public static Process GetFastbootProcess(string argument = null, string deviceName = null)
        {
            Process process = new Process();
            string args = $"/C fastboot {(deviceName != null ? $"-s {deviceName}" : "")} {argument}";
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

        public static string SendFastbootCommand(string command, string deviceName = null, int timeout = 10)
        {
            Process shell = GetFastbootProcess(command, deviceName);

            StringBuilder sb = new StringBuilder();

            Thread.Sleep(250);

            Task t = Task.Run(() =>
            {
                try
                {
                    while (shell.StandardOutput.EndOfStream == false || shell.StandardError.EndOfStream == false)
                    {
                        if (shell.StandardOutput.EndOfStream == false) sb.AppendLine(shell.StandardOutput.ReadLine());
                        if (shell.StandardError.EndOfStream == false) sb.AppendLine(shell.StandardError.ReadLine());
                    }
                }
                catch { }
            });

            t.Wait(timeout);
            shell.Close();
            return sb.ToString();
        }
    }
}
