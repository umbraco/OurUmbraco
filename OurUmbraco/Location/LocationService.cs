using System;
using Newtonsoft.Json;
using System.Net;
using System.Web.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace OurUmbraco.Location
{
    public class LocationService
    {
        private readonly string AccessKey = WebConfigurationManager.AppSettings["IpStackAccessKey"];
        private const string CacheKey = "iplocation-";

        public Models.Location GetLocationByIp(string ip)
        {
            if (IsValidIp(ip) == false)
                return null;

            try
            {
                return (Models.Location) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                    CacheKey + ip, () =>
                    {
                        using (var client = new WebClient())
                        {
                            // From https://stackoverflow.com/a/62336888
                            // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
                            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                            // Add TLS 1.2
                            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                            
                            var response = client.DownloadString(
                                $"https://api.ipstack.com/{ip}?access_key={AccessKey}&fields=ip,continent_code,continent_name,country_code,country_name");
                            var output = JsonConvert.DeserializeObject<Models.Location>(response);
                            return output.Success ? output : null;
                        }
                    });
            }
            catch (Exception ex)
            {
                LogHelper.Error<LocationService>("Problem getting location", ex);
            }

            return null;
        }

        private bool IsValidIp(string ip)
        {
            IPAddress result = null;
            return string.IsNullOrEmpty(ip) == false && IPAddress.TryParse(ip, out result);
        }
    }
}
