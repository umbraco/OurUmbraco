using Newtonsoft.Json;
using OurUmbraco.Forum.Models;
using System;
using System.Net.Http;

namespace OurUmbraco.Forum.Services
{
    internal class DiscourseService
    {
        internal DiscourseTopic GetTopicByOldIdAsync(int id)
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"]);
                client.DefaultRequestHeaders.Add("Api-Key", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiKey"]}");
                client.DefaultRequestHeaders.Add("Api-Username", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiUsername"]}");

                var result = client.GetAsync($"t/external_id/{id}.json").Result;
                
                if (result.IsSuccessStatusCode == false)
                {
                    return null;
                }
                else
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    var discourseTopic = JsonConvert.DeserializeObject<DiscourseTopic>(resultContent);
                    return discourseTopic;
                }                
            }
        }
    }
}
