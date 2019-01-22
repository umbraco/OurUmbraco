using Newtonsoft.Json;
using System.Net;
using System.Web.Configuration;
using Umbraco.Core;

namespace OurUmbraco.Location
{
    public class LocationService
    {
        private readonly string AccessKey = WebConfigurationManager.AppSettings["IpStackAccessKey"];
        private readonly string CacheKey = "iplocation-";

        public Models.Location GetLocationByIp(string ip)
        {
            if (IsValidIp(ip) == false)
            {
                return null;
            }

            return (Models.Location)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKey + ip, () =>
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString(string.Format("https://api.ipstack.com/{0}?access_key={1}&fields=ip,continent_code,continent_name,country_code,country_name", ip, AccessKey));
                    var output = JsonConvert.DeserializeObject<Models.Location>(response);
                    return output.Success == true ? output : null;
                }
            });
        }

        private bool IsValidIp(string ip)
        {
            IPAddress result = null;
            return string.IsNullOrEmpty(ip) == false && IPAddress.TryParse(ip, out result);
        }
    }
}
