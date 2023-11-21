using System;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using umbraco.BusinessLogic;

namespace OurUmbraco.Forum
{
    public class Slack
    {
        internal void PostSlackMessage(string message)
        {
            var payload = JsonConvert.SerializeObject(new { text = message });
            
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                try
                {
                    ServicePointManager.Expect100Continue = true;                
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    
                    var webhookUrl = $"https://hooks.slack.com/services/{ConfigurationManager.AppSettings["SlackWebhookSecret"]}";
                    client.UploadString(new Uri(webhookUrl), "POST", payload);
                }
                catch (WebException ex)
                {
                    var errorMessage = $"Posting update to Slack failed {ex.Message} {ex.StackTrace}";
                    Log.Add(LogTypes.Error, new User(0), -1, errorMessage);
                }
            }
        }
    }
}