using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Core;

namespace OurUmbraco
{

    //
    // Summary:
    //     The remote image service.
    public class RemoteImageService : IImageService
    {
        private static readonly HttpClient Client = new HttpClient(RemoteFile.Handler);

        private RemoteFile remoteFile;

        private Dictionary<string, string> settings = new Dictionary<string, string>
    {
        { "MaxBytes", "4194304" },
        { "Timeout", "30000" },
        { "Protocol", "http" },
        {
            "UserAgent",
            string.Empty
        }
    };

        //
        // Summary:
        //     Gets or sets the prefix for the given implementation. This value is used as a
        //     prefix for any image requests that should use this service.
        public string Prefix { get; set; } = "remote.axd";


        //
        // Summary:
        //     Gets a value indicating whether the image service requests files from the locally
        //     based file system.
        public bool IsFileLocalService => false;

        //
        // Summary:
        //     Gets or sets any additional settings required by the service.
        public Dictionary<string, string> Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
                InitRemoteFile();
            }
        }

        //
        // Summary:
        //     Gets or sets the white list of System.Uri.
        public Uri[] WhiteList { get; set; } = new Uri[0];


        //
        // Summary:
        //     Gets a value indicating whether the current request passes sanitizing rules.
        //
        //
        // Parameters:
        //   path:
        //     The image path.
        //
        // Returns:
        //     True if the request is valid; otherwise, False.
        public virtual bool IsValidRequest(string path)
        {
            string host = new Uri(path).Host;
            bool flag = false;

            Uri[] whiteList = WhiteList;
            foreach (Uri uri in whiteList)
            {
                if (uri.IsAbsoluteUri)
                {
                    flag = (host).InvariantEquals(uri.Host);
                }

                if (flag)
                {
                    break;
                }
            }

            return flag;
        }

        //
        // Summary:
        //     Gets the image using the given identifier.
        //
        // Parameters:
        //   id:
        //     The value identifying the image to fetch.
        //
        // Returns:
        //     The System.Byte array containing the image data.
        public virtual async Task<byte[]> GetImage(object id)
        {
            if (remoteFile == null)
            {
                InitRemoteFile();
            }

            HttpResponseMessage httpResponseMessage = await remoteFile.GetResponseAsync(new Uri(id.ToString())).ConfigureAwait(continueOnCapturedContext: false);
            if (httpResponseMessage == null)
            {
                return null;
            }

            byte[] result;
            using (MemoryStream memoryStream = MemoryStreamPool.Shared.GetStream())
            {
                using (HttpResponseMessage response = httpResponseMessage)
                {
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        await responseStream.CopyToAsync(memoryStream).ConfigureAwait(continueOnCapturedContext: false);
                        memoryStream.Position = 0L;
                        result = memoryStream.ToArray();
                    }
                }
            }

            return result;
        }

        private void InitRemoteFile()
        {
            int timeoutMilliseconds = int.Parse(Settings["Timeout"]);
            int maxDownloadSize = int.Parse(Settings["MaxBytes"]);
            Settings.TryGetValue("Useragent", out var value);
            remoteFile = new RemoteFile(Client, timeoutMilliseconds, maxDownloadSize, value);
        }
    }
}
