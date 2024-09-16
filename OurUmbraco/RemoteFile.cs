using ImageProcessor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OurUmbraco
{
    //
    // Summary:
    //     Encapsulates methods used to download files from a website address.
    //
    // Remarks:
    //     The purpose of this class is so there's one core way of downloading remote files
    //     with url[s] that are from outside users. There's various areas in application
    //     where an attacker could supply an external url to the server and tie up resources.
    //
    //
    //     For example, the ImageProcessingModule accepts off-server addresses as a path.
    //     An attacker could, for instance, pass the url to a file that's a few gigs in
    //     size, causing the server to get out-of-memory exceptions or some other errors.
    //     An attacker could also use this same method to use one application instance to
    //     hammer another site by, again, passing an off-server address of the victims site
    //     to the ImageProcessor.Web.HttpModules.ImageProcessingModule. This class will
    //     not throw an exception if the Uri supplied points to a resource local to the
    //     running application instance.
    //
    //     There shouldn't be any security issues there, as the internal System.Net.Http.HttpClient
    //     instance is still calling it remotely. Any local files that shouldn't be accessed
    //     by this and won't be allowed by the remote call.
    internal sealed class RemoteFile
    {
        //
        // Summary:
        //     The default message handler used by HttpClient instances.
        public static readonly HttpClientHandler Handler = new HttpClientHandler
        {
            AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate),
            Credentials = CredentialCache.DefaultNetworkCredentials
        };

        private readonly HttpClient client;

        //
        // Summary:
        //     Gets or sets the timespan to wait before the request times out.
        public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(100.0);


        //
        // Summary:
        //     Gets the maximum download size, in bytes, that a remote file download attempt
        //     can be.
        public int MaxDownloadSize { get; } = int.MaxValue;


        //
        // Summary:
        //     Gets the UserAgent header to be passed when requesting the remote file
        public string UserAgent { get; }

        //
        // Summary:
        //     Initializes a new instance of the ImageProcessor.Web.Helpers.RemoteFile class.
        //
        //
        // Parameters:
        //   client:
        //     The client used for creating the web request.
        //
        //   timeoutMilliseconds:
        //     The maximum time, in milliseconds, to wait before the request times out.
        //
        //   maxDownloadSize:
        //     The maximum download size, in bytes, that a remote file download attempt can
        //     be.
        public RemoteFile(HttpClient client, int timeoutMilliseconds, int maxDownloadSize)
            : this(client, timeoutMilliseconds, maxDownloadSize, null)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the ImageProcessor.Web.Helpers.RemoteFile class.
        //
        //
        // Parameters:
        //   client:
        //     The client used for creating the web request.
        //
        //   timeoutMilliseconds:
        //     The maximum time, in milliseconds, to wait before the request times out.
        //
        //   maxDownloadSize:
        //     The maximum download size, in bytes, that a remote file download attempt can
        //     be.
        //
        //   userAgent:
        //     The User-Agent header to be passed when requesting the remote file.
        public RemoteFile(HttpClient client, int timeoutMilliseconds, int maxDownloadSize, string userAgent)
        {
            this.client = client ?? new HttpClient(Handler);
            if (timeoutMilliseconds >= 0)
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            }

            if (maxDownloadSize > 0)
            {
                MaxDownloadSize = maxDownloadSize;
            }

            UserAgent = userAgent;
            this.client.Timeout = Timeout;
            if (!string.IsNullOrWhiteSpace(UserAgent) && !this.client.DefaultRequestHeaders.TryGetValues("User-Agent", out var _))
            {
                this.client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }
        }

        //
        // Summary:
        //     Returns the System.Net.Http.HttpResponseMessage used to download this file.
        //
        //     This method is meant for outside users who need specific access to the HttpResponseMessage
        //     this class generates. They're responsible for disposing of it.
        //
        // Parameters:
        //   uri:
        //     The request uri.
        //
        // Returns:
        //     The System.Net.Http.HttpResponseMessage used to download this file.
        internal async Task<HttpResponseMessage> GetResponseAsync(Uri uri)
        {
            HttpResponseMessage response = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                response = await client.GetAsync(uri).ConfigureAwait(continueOnCapturedContext: false);
                statusCode = response.StatusCode;
                response.EnsureSuccessStatusCode();
                long? contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue)
                {
                    if (contentLength > MaxDownloadSize)
                    {
                        response.Dispose();
                        ImageProcessorBootstrapper.Instance.Logger.Log<RemoteFile>("An attempt to download a remote file has been halted because the file is larger than allowed.", "GetResponseAsync", 150);
                        throw new SecurityException("An attempt to download a remote file has been halted because the file is larger than allowed.");
                    }

                    return response;
                }

                response.Dispose();
                throw new HttpException(404, $"No image exists at {uri}");
            }
            catch (HttpRequestException ex2)
            {
                switch (statusCode)
                {
                    case HttpStatusCode.NotFound:
                        Log(ex2);
                        response?.Dispose();
                        throw new HttpException((int)statusCode, $"No image exists at {uri}", ex2);
                    default:
                        Log(ex2);
                        break;
                    case HttpStatusCode.NotModified:
                        break;
                }
            }

            return null;
        }

        private static void Log(HttpRequestException ex)
        {
            ImageProcessorBootstrapper.Instance.Logger.Log<RemoteFile>(ex.Message, "GetResponseAsync", 132);
        }
    }
}
