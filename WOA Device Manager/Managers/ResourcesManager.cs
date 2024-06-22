using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace WOADeviceManager.Managers
{
    public class ResourcesManager
    {
        public enum DownloadableComponent
        {
            UEFI_EPSILON, UEFI_ZETA, TWRP_EPSILON, TWRP_ZETA, PARTED
        }

        public static Task<StorageFile> RetrieveFile(DownloadableComponent component, bool redownload = false)
        {
            string releaseVersion = "2406.36";
            string downloadPath = string.Empty;
            string fileName = string.Empty;
            switch (component)
            {
                case DownloadableComponent.TWRP_EPSILON:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/surfaceduo1-twrp.img";
                    fileName = "surfaceduo1-twrp.img";
                    break;
                case DownloadableComponent.TWRP_ZETA:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/surfaceduo2-twrp.img";
                    fileName = "surfaceduo2-twrp.img";
                    break;
                case DownloadableComponent.PARTED:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/parted";
                    fileName = "parted";
                    break;
                case DownloadableComponent.UEFI_EPSILON:
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    fileName = $"Surface.Duo.1st.Gen.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    break;
                case DownloadableComponent.UEFI_ZETA:
                    downloadPath = $"https://github.com/WOA-Project/SurfaceDuo-Releases/releases/download/{releaseVersion}/Surface.Duo.2.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    fileName = $"Surface.Duo.2.UEFI-v{releaseVersion}.Fast.Boot.zip";
                    break;
            }
            return RetrieveFile(downloadPath, fileName, redownload);
        }

        public static async Task<StorageFile> RetrieveFile(string path, string fileName, bool redownload = false)
        {
            if (redownload || !IsFileAlreadyDownloaded(fileName))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using HttpClient client = new();
                using Task<Stream> webstream = client.GetStreamAsync(new Uri(path));
                using FileStream fs = new(file.Path, FileMode.OpenOrCreate);
                webstream.Result.CopyTo(fs);
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
