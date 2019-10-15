using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using HtmlAgilityPack;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Library;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace OurUmbraco.Forum.AntiSpam
{
    internal class SpamChecker
    {
        public static bool IsSpam(IMember member, string body)
        {
            // Members with over 50 karma are trusted automatically
            if (member.Karma() >= 50)
                return false;

            var roles = Roles.GetRolesForUser(member.Username);
            var isSpam = roles.Contains("potentialspam") || roles.Contains("newaccount") || NewAndPostsALot(member) || TextContainsSpam(body) || IsSuspiciousBehavior(body);

            if (isSpam)
            {
                //Deduct karma
                var reputationTotal = member.GetValue<int>("reputationTotal");
                member.SetValue("reputationTotal", reputationTotal >= 0 ? reputationTotal - 1 : 0);

                var reputationCurrent = member.GetValue<int>("reputationCurrent");
                member.SetValue("reputationCurrent", reputationCurrent >= 0 ? reputationCurrent - 1 : 0);

                var memberService = ApplicationContext.Current.Services.MemberService;
                memberService.Save(member);
                memberService.AssignRole(member.Id, "potentialspam");
            }

            return isSpam;
        }

        public static void SendSlackSpamReport(string postBody, int topicId, string commentType, int memberId)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            using (var client = new WebClient())
            {
                var topic = ts.GetById(topicId);
                var post = string.Format("Topic title: *{0}*\n\n Link to topic: https://our.umbraco.com{1}\n\n", topic.Title, topic.GetUrl());
                post = post + string.Format("{0} text: {1}\n\n", commentType, postBody);
                post = post + string.Format("Go to member https://our.umbraco.com/member/{0}\n\n", memberId);

                var body = string.Format("The following forum post was marked as spam by the spam system, if this is incorrect make sure to mark it as ham.\n\n{0}", post);
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
            var doc = new HtmlDocument();
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

        private static bool NewAndPostsALot(IMember member)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            var topics = ts.GetAuthorLatestTopics(member.Id);
            var topicsInHourAfterSignup = new List<ReadOnlyTopic>();
            var topicsInFirstDayAfterSignup = new List<ReadOnlyTopic>();

            foreach (var topic in topics)
            {
                if((topic.Created - member.CreateDate).Hours <= 1)
                    topicsInHourAfterSignup.Add(topic);

                if ((topic.Created - member.CreateDate).Days <= 1)
                    topicsInFirstDayAfterSignup.Add(topic);
            }

            if (topicsInHourAfterSignup.Count >= 3)
                return true;

            if (topicsInFirstDayAfterSignup.Count >= 5)
                return true;

            var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);
            var comments = cs.GetAllCommentsForMember(member.Id);

            var commentsInHourAfterSignup = new List<Comment>();
            var commentsInFirstDayAfterSignup = new List<Comment>();

            foreach (var comment in comments)
            {
                if ((comment.Created - member.CreateDate).Hours <= 1)
                    commentsInHourAfterSignup.Add(comment);

                if ((comment.Created - member.CreateDate).Days <= 1)
                    commentsInFirstDayAfterSignup.Add(comment);
            }

            if (commentsInHourAfterSignup.Count >= 3)
                return true;

            if (commentsInFirstDayAfterSignup.Count >= 5)
                return true;

            return false;
        }
    }
}
