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
            TWRP, PARTED, MASS_STORAGE_SCRIPT
        }

        public static Task<StorageFile> RetrieveFile(DownloadableComponent component)
        {
            string downloadPath = string.Empty;
            string fileName = string.Empty;
            switch (component)
            {
                case DownloadableComponent.TWRP:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/InstallWindows/Files/surfaceduo1-twrp.img";
                    fileName = "TWRP.img";
                    break;
                case DownloadableComponent.PARTED:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/InstallWindows/Files/parted";
                    fileName = "parted";
                    break;
                case DownloadableComponent.MASS_STORAGE_SCRIPT:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/InstallWindows/Files/msc.tar";
                    fileName = "msc.tar";
                    break;
            }
            return RetrieveFile(downloadPath, fileName);
        }

        public async static Task<StorageFile> RetrieveFile(string path, string fileName)
        {
            if (!IsFileAlreadyDownloaded(fileName))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (HttpClient client = new())
                {
                    using (var webstream = client.GetStreamAsync(new Uri(path)))
                    {
                        using (FileStream fs = new FileStream(file.Path, FileMode.OpenOrCreate))
                        {
                            webstream.Result.CopyTo(fs);
                            return file;
                        }
                    }
                }
            } else
            {
                return await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
        }

        public static bool IsFileAlreadyDownloaded(string fileName)
        {
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + fileName)) 
            { 
                return true;
            }
            return false;
        }
    }
}
