using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.Our.Businesslogic
{
    public class GoogleAnalytics
    {
        public string UserId { get; set; }

        public GoogleAnalytics(Collection<CookieHeaderValue> gaCookie)
        {
            if (gaCookie.Any() && gaCookie.FirstOrDefault()["_ga"] != null)
            {
                var gaCookieValue = gaCookie.FirstOrDefault()["_ga"].Value;
                if (string.IsNullOrWhiteSpace(gaCookieValue) == false)
                {
                    var gaCookieSplit = gaCookieValue.Split('.');
                    if (gaCookieSplit.Length == 4)
                    {
                        UserId = gaCookieSplit[2] + "." + gaCookieSplit[3];
                    }
                }
            }
        }

        public void SendSearchQuery(string term, string category)
        {
            if (string.IsNullOrWhiteSpace(UserId) == false)
            {
                string gaQueryTemplate =
                    "%2Fsearch%3Fq%3D{0}%26cat%3D{1}";

                var query = string.Format(gaQueryTemplate, term, category);

                // send the query
                using (WebClient client = new WebClient())
                {

                    byte[] response =
                        client.UploadValues("http://www.google-analytics.com/collect", new NameValueCollection()
                        {
                        { "v", "1" },
                        { "t", "pageview" },
                        { "tid", "UA-120590-4" },
                        { "uid", UserId },
                        { "dl", query }
                        });

                    string result = System.Text.Encoding.UTF8.GetString(response);
                }
            }
        }
    }
}
