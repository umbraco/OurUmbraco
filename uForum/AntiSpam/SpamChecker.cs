using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using uForum.Library;
using uForum.Models;
using uForum.Services;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Web;


namespace uForum.AntiSpam
{
    internal class SpamChecker
    {
        public static bool IsSpam(Umbraco.Core.Models.IMember member, string body)
        {
            int reputationTotal;

            // Members with over 50 karma are trusted automatically
            if (member.Karma() >= 50)
                return false;

            var isSpam = TextContainsSpam(body) || IsSuspiciousBehavior(body);

            //if (isSpam)
            //{
            //    // Deduct karma
            //    //member getProperty("reputationTotal").Value = reputationTotal >= 0 ? reputationTotal - 1 : 0;

            //    //int reputationCurrent;
            //    //int.TryParse(member.getProperty("reputationCurrent").Value.ToString(), out reputationCurrent);
            //    //member.getProperty("reputationCurrent").Value = reputationCurrent >= 0 ? reputationCurrent - 1 : 0;
            //    //member.Save();
            //    SendSlackSpamReport(body, topicId, commentType, member.Id);
            //}

            return isSpam;
        }

        public static void SendSlackSpamReport(string postBody, int topicId, string commentType, int memberId)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            using (var client = new WebClient())
            {
                var topic = ts.GetById(topicId);
                var post = string.Format("Topic title: *{0}*\n\n Link to topic: http://our.umbraco.org{1}\n\n", topic.Title, topic.GetUrl());
                post = post + string.Format("{0} text: {1}\n\n", commentType, postBody);
                post = post + string.Format("Go to member http://our.umbraco.org/member/{0}\n\n", memberId);

                var body = string.Format("The following forum post was marked as spam by the spam system, if this is incorrect make sure to mark it as ham.\n\n{0}", post);

                if (memberId != 0)
                {
                    var member = ApplicationContext.Current.Services.MemberService.GetById(memberId);

                    if (member != null)
                    {
                        var querystring = string.Format("api?ip={0}&email={1}&f=json", Utils.GetIpAddress(), HttpUtility.UrlEncode(member.Email));
                        body = body + string.Format("Check the StopForumSpam rating: http://api.stopforumspam.org/{0}", querystring);
                    }
                    
                }

                body = body.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                var values = new NameValueCollection
                             {
                                 {"channel", ConfigurationManager.AppSettings["SlackChannel"]},
                                 {"token", ConfigurationManager.AppSettings["SlackToken"]},
                                 {"username", ConfigurationManager.AppSettings["SlackUsername"]},
                                 { "icon_url", ConfigurationManager.AppSettings["SlackIconUrl"]},
                                 {"text", body}
                             };

                try
                {
                    var data = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);
                    var response = client.Encoding.GetString(data);
                }
                catch (Exception ex)
                {
                    Log.Add(LogTypes.Error, new User(0), -1, string.Format("Posting update to Slack failed {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        // Remember this one only kicks in if a user has < 50 karma
        private static bool IsSuspiciousBehavior(string body)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(body);

            var validLinksCount = 0;
            var otherLinksCount = 0;

            var anchorNodes = doc.DocumentNode.SelectNodes("//a");
            if (anchorNodes != null)
            {
                foreach (var anchorNode in anchorNodes)
                {
                    if (anchorNode.Attributes != null && anchorNode.Attributes["href"] != null)
                    {
                        var href = anchorNode.Attributes["href"].Value;
                        validLinksCount = CountValidLinks(href, validLinksCount);
                    }
                }
                otherLinksCount = anchorNodes.Count - validLinksCount;

                // If there are links that don't go to umbraco domains, it might be spam
                if (otherLinksCount > 0)
                    return true;
            }


            // Same for markdown style links
            var rx = new Regex(@"\[\s*([a-zA-Z0-9_-]+)\s*\]\s*:\s*(\S+)\s*("".*?"")?");
            var matches = rx.Matches(body);
            validLinksCount = 0;

            foreach (Match match in matches)
            {
                var groups = match.Groups;
                string href = null;
                try
                {
                    href = groups[2].Value;
                }
                catch (Exception ex) { }

                validLinksCount = CountValidLinks(href, validLinksCount);
            }

            otherLinksCount = matches.Count - validLinksCount;

            // If there are links that don't go to umbraco domains, it might be spam
            if (otherLinksCount > 0)
                return true;

            return false;
        }

        internal static int CountValidLinks(string href, int validLinksCount)
        {
            if (href != null && (href.TrimStart().StartsWith("/media")
                                 || href.TrimStart().StartsWith("/forum")
                                 || href.TrimStart().StartsWith("@")
                                 || href.TrimStart().StartsWith("localhost")
                                 || href.Contains(".local")
                                 || href.Contains("stackoverflow.com")
                                 || href.Contains("umbraco.org")
                                 || href.Contains("umbraco.tv")
                                 || href.Contains("umbraco.io")
                                 || href.Contains("azure.com")
                                 || href.Contains("microsoft.com")
                                 || href.Contains("asp.net")
                                 || href.Contains("github.com")
                                 || href.Contains("example.com")))
            {
                validLinksCount = validLinksCount + 1;
            }

            return validLinksCount;
        }

        private static bool TextContainsSpam(string text)
        {
            var spamWords = ConfigurationManager.AppSettings["uForumSpamWords"];
            return spamWords.Split(',').Any(spamWord => text.ToLowerInvariant().Contains(spamWord.Trim().ToLowerInvariant()));
        }
    }
}
