using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Helpers;

namespace WOADeviceManager.Managers
{
    public class ResourcesManager
    {
        public enum DownloadableComponent
        {
            DRIVERS_EPSILON,
            DRIVERS_ZETA,
            FD_EPSILON,
            FD_SECUREBOOT_DISABLED_EPSILON,
            FD_SECUREBOOT_DISABLED_ZETA,
            FD_ZETA,
            PARTED,
            TWRP_EPSILON,
            TWRP_ZETA,
            UEFI_EPSILON,
            UEFI_SECUREBOOT_DISABLED_EPSILON,
            UEFI_SECUREBOOT_DISABLED_ZETA,
            UEFI_ZETA
        }

        public static async Task<StorageFile> RetrieveFile(DownloadableComponent component, bool redownload = false)
        {
            string downloadPath = string.Empty;
            string fileName = string.Empty;
            string releaseVersion = string.Empty;

            switch (component)
            {
                case DownloadableComponent.PARTED:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/parted";
                    fileName = "parted";
                    break;

                case DownloadableComponent.TWRP_EPSILON:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/surfaceduo1-twrp.img";
                    fileName = "surfaceduo1-twrp.img";
                    break;
                case DownloadableComponent.UEFI_EPSILON:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    fileName = $"Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    break;
                case DownloadableComponent.UEFI_SECUREBOOT_DISABLED_EPSILON:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Secure.Boot.Disabled.Fast.Boot.zip";
                    fileName = $"Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Secure.Boot.Disabled.Fast.Boot.zip";
                    break;
                case DownloadableComponent.FD_EPSILON:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.FD.for.making.your.own.Dual.Boot.Image.zip";
                    fileName = $"Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.FD.for.making.your.own.Dual.Boot.Image.zip";
                    break;
                case DownloadableComponent.FD_SECUREBOOT_DISABLED_EPSILON:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Secure.Boot.Disabled.FD.for.making.your.own.Dual.Boot.Image.zip";
                    fileName = $"Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Secure.Boot.Disabled.FD.for.making.your.own.Dual.Boot.Image.zip";
                    break;
                case DownloadableComponent.DRIVERS_EPSILON:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/SurfaceDuo-Drivers-v{releaseVersion}-Desktop-Epsilon.7z";
                    fileName = $"SurfaceDuo-Drivers-v{releaseVersion}-Desktop-Epsilon.7z";
                    break;

                case DownloadableComponent.TWRP_ZETA:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/surfaceduo2-twrp.img";
                    fileName = "surfaceduo2-twrp.img";
                    break;
                case DownloadableComponent.UEFI_ZETA:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.2.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    fileName = $"Surface.Duo.2.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    break;
                case DownloadableComponent.UEFI_SECUREBOOT_DISABLED_ZETA:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.2.UEFI-v{releaseVersion}.Secure.Boot.Disabled.Fast.Boot.zip";
                    fileName = $"Surface.Duo.2.UEFI-v{releaseVersion}.Secure.Boot.Disabled.Fast.Boot.zip";
                    break;
                case DownloadableComponent.FD_ZETA:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.2.UEFI-v{releaseVersion}.FD.for.making.your.own.Dual.Boot.Image.zip";
                    fileName = $"Surface.Duo.2.UEFI-v{releaseVersion}.FD.for.making.your.own.Dual.Boot.Image.zip";
                    break;
                case DownloadableComponent.FD_SECUREBOOT_DISABLED_ZETA:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.2.UEFI-v{releaseVersion}.Secure.Boot.Disabled.FD.for.making.your.own.Dual.Boot.Image.zip";
                    fileName = $"Surface.Duo.2.UEFI-v{releaseVersion}.Secure.Boot.Disabled.FD.for.making.your.own.Dual.Boot.Image.zip";
                    break;
                case DownloadableComponent.DRIVERS_ZETA:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/SurfaceDuo-Drivers-v{releaseVersion}-Desktop-Zeta.7z";
                    fileName = $"SurfaceDuo-Drivers-v{releaseVersion}-Desktop-Zeta.7z";
                    break;
            }
            return await RetrieveFile(downloadPath, fileName, redownload);
        }

        public static async Task<StorageFile> RetrieveFile(string path, string fileName, bool redownload = false)
        {
            if (redownload || !IsFileAlreadyDownloaded(fileName))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using HttpClient client = new();
                using Task<Stream> webStream = client.GetStreamAsync(new Uri(path));
                using FileStream fs = new(file.Path, FileMode.OpenOrCreate);
                webStream.Result.CopyTo(fs);
                return file;
            }
            else
            {
                return await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
        }

        public static bool IsFileAlreadyDownloaded(string fileName)
        {
            return File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + fileName);
        }
    }
}
