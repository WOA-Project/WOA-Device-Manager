using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WOADeviceManager.Helpers
{
    public static class HttpsUtils
    {
        public static async Task<string> GetLatestBSPReleaseVersion()
        {
            return await GetLatestGithubReleaseVersion("https://github.com/WOA-Project/SurfaceDuo-Releases");
        }

        public static async Task<string> GetLatestWOADeviceManagerVersion()
        {
            return await GetLatestGithubReleaseVersion("https://github.com/WOA-Project/WOA-Device-Manager");
        }

        private static async Task<string> GetLatestGithubReleaseVersion(string repository)
        {
            string RedirectedUrl = await GetRedirectedUrl($"{repository}/releases/latest");
            return RedirectedUrl.Replace($"{repository}/releases/tag/", "");
        }

        public static async Task<string> GetRedirectedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            int maximumRedirectionCount = 8;  // prevent infinite loops
            string newUrl = url;

            using HttpClient client = new(new HttpClientHandler() { AllowAutoRedirect = false });

            do
            {
                try
                {
                    using HttpResponseMessage resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            {
                                return newUrl;
                            }
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.RedirectKeepVerb:
                        case HttpStatusCode.RedirectMethod:
                            {
                                newUrl = resp.Headers.Location.ToString();
                                if (newUrl == null)
                                {
                                    return url;
                                }

                                if (!newUrl.Contains("://"))
                                {
                                    // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                                    Uri u = new(new Uri(url), newUrl);
                                    newUrl = u.ToString();
                                }
                                break;
                            }
                        default:
                            {
                                return newUrl;
                            }
                    }
                    url = newUrl;
                }
                catch (WebException)
                {
                    // Return the last known good URL
                    return newUrl;
                }
                catch (Exception)
                {
                    return null;
                }
            } while (maximumRedirectionCount-- > 0);

            return newUrl;
        }
    }
}
