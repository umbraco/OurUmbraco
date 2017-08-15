using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OurUmbraco.Announcements.Services
{

    public class EcomApiService
    {
        private static HttpClient _client = new HttpClient();
        private static bool _isInitialized;
        private static void init()
        {
            if (_isInitialized == false)
            {
                _client.BaseAddress = new Uri("http://ecomapi.umbraco.com/api/Shop/");
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _isInitialized = true;
            }
        }

        public static async Task<Models.LocationDisplayModel> GetLocationAsync(string ip)
        {
            init();

            if (IsReady())
            {

                var ipModel = new Models.LocationModel()
                {
                    ApiToken = Guid.Parse(ConfigurationManager.AppSettings["ecomApiToken"]),
                    AppId = "our",
                    Ip = ip
                };
                Models.LocationDisplayModel location = null;
                HttpResponseMessage response = await _client.PostAsJsonAsync("PostCountryFromIp", ipModel);
                if (response.IsSuccessStatusCode)
                {
                    location = await response.Content.ReadAsAsync<Models.LocationDisplayModel>();
                }
                return location;
            }
            else
            {
                throw new ArgumentException("Missing valid API Token in web.config");
            }
        }


        public static bool IsReady()
        {
            // if there's no valid ecom api token, then don't use this feature
            var ecomApiToken = ConfigurationManager.AppSettings["ecomApiToken"];
            Guid tryParseGuid;
            return string.IsNullOrWhiteSpace(ecomApiToken) == false && Guid.TryParse(ecomApiToken, out tryParseGuid);
        }
    }
}
