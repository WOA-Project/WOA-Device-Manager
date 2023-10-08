using SAPTeam.AndroCtrl.Adb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace WOADeviceManager.Managers
{
    class ADBManager
    {
        private static AdbServer server;
        private static AdbClient client;
        public static AdbClient Client
        {
            get
            {
                if (client == null) client = new AdbClient();
                return client;
            }
        }

        private static ADBManager _instance;
        public static ADBManager Instance
        {
            get
            {
                if (_instance == null) _instance = new ADBManager();
                return _instance;
            }
        }

        private ADBManager()
        {
            server = new AdbServer();
            server.StartServer(@"X:\Tools\ADB\adb.exe", restartServerIfNewer: true);
        }

        // TODO: To be reimplemented
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

        [Obsolete]
        public static Process GetADBProcess(string argument = null, string deviceName = null)
        {
            Process process = new Process();
            string args = $"/C adb {(deviceName != null ? $"-s {deviceName}" : "")} {argument}";
            // TODO: These need to be reimplemented
            //if (!string.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["CustomADBPath"] as string))
            //{
            //    args = $"/C \"\"{ApplicationData.Current.LocalSettings.Values["CustomADBPath"]}\" {argument}\"";
            //}
            //else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "path")
            //{
            //    args = $"/C adb {argument}";
            //}
            //else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "downloaded")
            //{
            //    args = $"/C \"\"{ApplicationData.Current.LocalSettings.Values["adbPath"]}\\adb.exe\" {argument}\"";
            //}
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

        [Obsolete]
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

        [Obsolete]
        public static string SendADBCommand(string command, string deviceName = null, int timeout = 10)
        {
            Process shell = GetADBProcess(command, deviceName);

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
                } catch { }
            });

            t.Wait(timeout);
            shell.Close();
            return sb.ToString();
        }

        [Obsolete]
        public static async Task<string> SendShellCommand(string command, string deviceName = null, bool restart = false, string workingPath = null)
        {
            StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"shellout{command}{DateTime.Now.Ticks}.text", CreationCollisionOption.ReplaceExisting);
            TextWriter textWriter = new StreamWriter(tempFile.Path);
            int exitCode = -999;
            string adbpath = "adb";
            // TODO: These need to be reimplemented
            //if (!string.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["CustomADBPath"] as string))
            //{
            //    adbpath = $"\"{ApplicationData.Current.LocalSettings.Values["CustomADBPath"]}\"";
            //}
            //else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "path")
            //{
            //    adbpath = $"adb";
            //}
            //else if (ApplicationData.Current.LocalSettings.Values["adbLocation"] as string == "downloaded")
            //{
            //    adbpath = $"\"{ApplicationData.Current.LocalSettings.Values["adbPath"]}\\adb.exe\"";
            //}
            try
            {
                exitCode = await StartProcess(
                    adbpath,
                    $"{(deviceName != null ? $"-s {deviceName} " : "")}shell {command}",
                    workingPath,
                    10000,
                    textWriter,
                    textWriter
                );
            }
            catch (TaskCanceledException)
            {
            }

            while (exitCode == -999) Thread.Sleep(100);
            textWriter.Close();

            StreamReader reader = new StreamReader(tempFile.Path);
            string output = reader.ReadToEnd();
            reader.Close();
            return output;
        }

        // These next three methods were stolen somewhere on the web, replace them
        [Obsolete]
        public static async Task<int> StartProcess(string filename, string arguments, string workingDirectory = null, int? timeout = null, TextWriter outputTextWriter = null, TextWriter errorTextWriter = null)
        {
            using (var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    Arguments = arguments,
                    FileName = filename,
                    RedirectStandardOutput = outputTextWriter != null,
                    RedirectStandardError = errorTextWriter != null,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory
                }
            })
            {
                var cancellationTokenSource = timeout.HasValue ?
                    new CancellationTokenSource(timeout.Value) :
                    new CancellationTokenSource();

                process.Start();

                var tasks = new List<Task>(3) { process.WaitForExitAsync(cancellationTokenSource.Token) };
                if (outputTextWriter != null)
                {
                    tasks.Add(ReadAsync(
                        x =>
                        {
                            process.OutputDataReceived += x;
                            process.BeginOutputReadLine();
                        },
                        x => process.OutputDataReceived -= x,
                        outputTextWriter,
                        cancellationTokenSource.Token));
                }

                if (errorTextWriter != null)
                {
                    tasks.Add(ReadAsync(
                        x =>
                        {
                            process.ErrorDataReceived += x;
                            process.BeginErrorReadLine();
                        },
                        x => process.ErrorDataReceived -= x,
                        errorTextWriter,
                        cancellationTokenSource.Token));
                }

                await Task.WhenAll(tasks);
                return process.ExitCode;
            }
        }

        [Obsolete]
        public static Task WaitForExitAsync(Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            process.EnableRaisingEvents = true;

            var taskCompletionSource = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (sender, args) =>
            {
                process.Exited -= handler;
                taskCompletionSource.TrySetResult(null);
            };
            process.Exited += handler;

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        process.Exited -= handler;
                        taskCompletionSource.TrySetCanceled();
                    });
            }

            return taskCompletionSource.Task;
        }

        [Obsolete]
        public static Task ReadAsync(Action<DataReceivedEventHandler> addHandler, Action<DataReceivedEventHandler> removeHandler, TextWriter textWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            DataReceivedEventHandler handler = null;
            handler = new DataReceivedEventHandler(
                (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        removeHandler(handler);
                        taskCompletionSource.TrySetResult(null);
                    }
                    else
                    {
                        textWriter.WriteLine(e.Data);
                    }
                });

            addHandler(handler);

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        removeHandler(handler);
                        taskCompletionSource.TrySetCanceled();
                    });
            }

            return taskCompletionSource.Task;
        }
    }
}
