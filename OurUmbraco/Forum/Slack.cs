using System.Configuration;
using System.Net;
using System.Net.Http;
using Umbraco.Core.Logging;

namespace OurUmbraco.Forum
{
    public class Slack
    {
        public void PostSlackMessage(string message)
        {
            var bearerToken = ConfigurationManager.AppSettings["CollabBearerToken"];
            var url = ConfigurationManager.AppSettings["SlackRelayUrl"];
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                    var msg = new SlackPost { Message = message };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                    var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(url, httpContent).Result;
                    var responseMessage = response.Content.ReadAsStringAsync().Result;
                    LogHelper.Debug<Slack>("Post to Slack resulted in: " + responseMessage);
                }
            }
            catch (WebException ex)
            {
                LogHelper.Error<Slack>("Posting update to Slack failed", ex);
            }
        }

        private class SlackPost
        {
            public string Message { get; set; }
        }
    }
}