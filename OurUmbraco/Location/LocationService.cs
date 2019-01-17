using Newtonsoft.Json;
using System.Net;
using System.Web.Configuration;

namespace OurUmbraco.Location
{
    public class LocationService
    {
        private readonly string AccessKey = WebConfigurationManager.AppSettings["IpStackAccessKey"];

        public Models.Location GetLocationByIp(string ip)
        {
            if (IsValidIp(ip) == false)
            {
                return null;
            }

            using (var client = new WebClient())
            {
                var response = client.DownloadString(string.Format("http://api.ipstack.com/{0}?access_key={1}&fields=ip,continent_code,continent_name,country_code,country_name", ip, AccessKey));
                var output = JsonConvert.DeserializeObject<Models.Location>(response);
                return output.Continent == null || output.Country == null ? null : output;
            }
        }

        private bool IsValidIp(string ip)
        {
            IPAddress result = null;
            return string.IsNullOrEmpty(ip) == false && IPAddress.TryParse(ip, out result);
        }
    }
}
