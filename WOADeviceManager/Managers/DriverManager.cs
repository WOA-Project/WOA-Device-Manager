using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Managers
{
    public class DriverManager
    {
        public static async Task<bool> UpdateDrivers(string DriverRepo = null)
        {
            if (!DeviceManager.Device.IsInMassStorage || string.IsNullOrEmpty(DeviceManager.Device.MassStorage.Drive))
            {
                return false;
            }

            MainPage.SetStatus("Initializing", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            if (string.IsNullOrEmpty(DriverRepo))
            {
                try
                {
                    await new HttpClient().GetAsync("https://github.com");
                }
                catch
                {
                    throw new Exception("Your computer is offline! We need to be able to reach the internet in order to download the Drivers.");
                }

                MainPage.SetStatus("Downloading latest Driver Package for your device...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

                if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
                {
                    StorageFile driverPackage = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.DRIVERS_EPSILON, true);
                    if (driverPackage == null)
                    {
                        MainPage.SetStatus();
                        return false;
                    }

                    MainPage.SetStatus("Preparing to extract Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

                    string destinationPath = Path.Combine((await driverPackage.GetParentAsync()).Path, Path.GetFileNameWithoutExtension(driverPackage.Name));
                    if (Directory.Exists(destinationPath))
                    {
                        Directory.Delete(destinationPath, true);
                    }
                    Directory.CreateDirectory(destinationPath);
                    SevenZipFileExtractToDirectory(driverPackage.Path, destinationPath, true);

                    DriverRepo = destinationPath;
                }
                else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
                {
                    StorageFile driverPackage = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.DRIVERS_ZETA, true);
                    if (driverPackage == null)
                    {
                        MainPage.SetStatus();
                        return false;
                    }

                    MainPage.SetStatus("Preparing to extract Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

                    string destinationPath = Path.Combine((await driverPackage.GetParentAsync()).Path, Path.GetFileNameWithoutExtension(driverPackage.Name));
                    if (Directory.Exists(destinationPath))
                    {
                        Directory.Delete(destinationPath, true);
                    }
                    Directory.CreateDirectory(destinationPath);
                    SevenZipFileExtractToDirectory(driverPackage.Path, destinationPath, true);

                    DriverRepo = destinationPath;
                }
                else
                {
                    throw new Exception("Unknown device product");
                }
            }

            MainPage.SetStatus("Preparing to install Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            string DriverDefinitions;

            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                DriverDefinitions = $"{DriverRepo}\\definitions\\Desktop\\ARM64\\Internal\\epsilon.xml";
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                DriverDefinitions = $"{DriverRepo}\\definitions\\Desktop\\ARM64\\Internal\\zeta.xml";
            }
            else
            {
                throw new Exception("Unknown device product");
            }

            string PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            Process process = new();
            process.StartInfo.FileName = $"{DriverRepo}\\tools\\DriverUpdater\\{PROCESSOR_ARCHITECTURE}\\DriverUpdater.exe";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.StartInfo.Arguments = $"-r \"{DriverRepo}\" -d \"{DriverDefinitions}\" -p {DeviceManager.Device.MassStorage.Drive}";

            MainPage.SetStatus("Installing Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            try
            {
                process.Start();
                process.WaitForExit();

                MainPage.SetStatus();
                return process.ExitCode == 0;
            }
            catch
            {
                MainPage.SetStatus();
                return false;
            }
        }

        private static void SevenZipFileExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwriteFiles)
        {
            using SevenZipArchive archive = SevenZipArchive.Open(sourceArchiveFileName);

            archive.ExtractToDirectory(destinationDirectoryName, (double Percentage) =>
            {
                uint SanePercentage = (uint)Math.Floor(Percentage * 100);
                MainPage.SetStatus("Extracting Driver Package", SanePercentage, Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your device to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟", SubMessage: $"Progress: {SanePercentage}%");
            });
        }
    }
}
