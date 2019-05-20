using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;

namespace OurUmbraco.Our.Services
{
   public class SlackService
    {
        public void SendSlackNotification(string post, string slackChannel = null)
        {
            using (var client = new WebClient())
            {
                post = post.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                var values = new NameValueCollection
                             {
                                 {"channel", slackChannel?? ConfigurationManager.AppSettings["SlackChannel"] },
                                 {"token", ConfigurationManager.AppSettings["SlackToken"]},
                                 {"username", ConfigurationManager.AppSettings["SlackUsername"]},
                                 {"icon_url", ConfigurationManager.AppSettings["SlackIconUrl"]},
                                 {"text", post}
                             };

                
                    var data = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);
                    var response = client.Encoding.GetString(data);
               
            }
        }
    }
}
