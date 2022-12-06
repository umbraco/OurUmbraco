using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace OurUmbraco.Forum.AntiSpam
{
    public class StopForumSpamService
    {
        public StopForumSpamResult CheckEmail(string email)
        {
            // Only check the same email every 12 hours so as not to overload the stopforumspam service
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<StopForumSpamResult>("SpamCheck" + email, () =>
            {
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://api.stopforumspam.org/api?email={email}";
                    var response = httpClient.GetAsync(url).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var httpResult = response.Content.ReadAsStringAsync().Result;
                    var serializer = new XmlSerializer(typeof(StopForumSpamResult));
                    using (TextReader reader = new StringReader(httpResult))
                    {
                        var spamResult = (StopForumSpamResult)serializer.Deserialize(reader);
                        if (spamResult.Success)
                        {
                            return spamResult;
                        }
                    }
                }

                return null;
            }, TimeSpan.FromHours(12));
        }
    }
    
    [XmlRoot(ElementName="response")]
    public class StopForumSpamResult 
    { 

        [XmlElement(ElementName="type")] 
        public string Type { get; set; } 

        [XmlElement(ElementName="appears")] 
        public string Appears { get; set; } 

        [XmlElement(ElementName="lastseen")] 
        public string LastSeen { get; set; } 

        [XmlElement(ElementName="frequency")] 
        public int Frequency { get; set; } 

        [XmlAttribute(AttributeName="success")] 
        public bool Success { get; set; } 

        [XmlText] 
        public string Text { get; set; } 
    }
}