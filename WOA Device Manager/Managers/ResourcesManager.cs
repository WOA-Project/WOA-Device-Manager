using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

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

        private async void StartDownload(BackgroundTransferPriority priority, string url, string destination)
        {
            Uri source;
            if (!Uri.TryCreate(url, UriKind.Absolute, out source))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            StorageFile destinationFile;
            try
            {
                StorageFolder picturesLibrary = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.PicturesLibrary);
                destinationFile = await picturesLibrary.CreateFileAsync(
                    destination,
                    CreationCollisionOption.GenerateUniqueName);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            BackgroundDownloader downloader = new();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);

            download.Priority = priority;

            await HandleDownloadAsync(download, true);
        }

        private void DownloadProgress(DownloadOperation download)
        {
            BackgroundDownloadProgress currentProgress = download.Progress;

            double percent = 100;
            if (currentProgress.TotalBytesToReceive > 0)
            {
                percent = currentProgress.BytesReceived * 100 / currentProgress.TotalBytesToReceive;
            }

            if (currentProgress.HasRestarted)
            {

            }

            if (currentProgress.HasResponseChanged)
            {
                // We have received new response headers from the server.
                // Be aware that GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                ResponseInformation response = download.GetResponseInformation();
                int headersCount = response != null ? response.Headers.Count : 0;

                // If you want to stream the response data this is a good time to start.
                // download.GetResultStreamAt(0);
            }
        }

        private List<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                // GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                string statusCode = response != null ? response.StatusCode.ToString() : string.Empty;
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Execution error", ex, download))
                {
                    throw;
                }
            }
            finally
            {
                activeDownloads.Remove(download);
            }
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
            }
            else
            {
            }

            return true;
        }
    }
}
