using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using HtmlAgilityPack;
using MarkdownSharp;
using OurUmbraco.Forum.AntiSpam;
using OurUmbraco.Forum.Library;
using OurUmbraco.Forum.Models;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.Forum.Extensions
{
    public static class ForumExtensions
    {

        public static string ConvertToRelativeTime(this DateTime date)
        {

            var ts = DateTime.Now.Subtract(date);
            int span;
            int.TryParse(Math.Round(ts.TotalSeconds, 0).ToString(CultureInfo.InvariantCulture), out span);

            if (span < 60)
                return "1 minute ago";

            if (span >= 60 && span < 3600)
                return string.Concat(Math.Round(ts.TotalMinutes), " minutes ago");

            if (span >= 3600 && span < 7200)
                return "1 hour ago";

            if (span >= 3600 && span < 86400)
                return string.Concat(Math.Round(ts.TotalHours), " hours ago");

            if (span >= 86400 && span < 172800)
                return "1 day ago";

            if (span >= 172800 && span < 604800)
                return string.Concat(Math.Round(ts.TotalDays), " days ago");

            if (span >= 604800 && span < 1209600)
                return "1 week ago";

            if (span >= 1209600 && span < 2592000)
                return string.Concat(Math.Round(ts.TotalDays), " days ago");

            return date.ToString("MMM dd, yyyy @ HH:mm");
        }


        public static HtmlString Sanitize(this string html)
        {
            // Run it through Markdown first
            var md = new Markdown();
            html = md.Transform(html);

            // Linkify images if they are shown as resized versions (only relevant for new Markdown comments)
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var root = doc.DocumentNode;
            if (root != null)
            {
                var images = root.SelectNodes("//img");
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        var src = image.GetAttributeValue("src", "");
                        var orgSrc = src.Replace("rs/", "");

                        if (src == orgSrc || image.ParentNode.Name == "a") continue;

                        var a = doc.CreateElement("a");
                        a.SetAttributeValue("href", orgSrc);
                        a.SetAttributeValue("target", "_blank");

                        a.AppendChild(image.Clone());

                        image.ParentNode.ReplaceChild(a, image);
                    }
                }

                // Any links not going to an "approved" domain need to be marked as nofollow
                var links = root.SelectNodes("//a");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        if (link.Attributes["href"] != null && (SpamChecker.CountValidLinks(link.Attributes["href"].Value, 0) == 0))
                        {
                            if (link.Attributes["rel"] != null)
                            {
                                link.Attributes.Remove("rel");
                            }
                            link.Attributes.Add("rel", "nofollow");
                        }
                    }
                }

                // Remove styles from all elements
                var elementsWithStyleAttribute = root.SelectNodes("//@style");
                if (elementsWithStyleAttribute != null)
                {
                    foreach (var element in elementsWithStyleAttribute)
                    {
                        element.Attributes.Remove("style");
                    }
                }

                using (var writer = new StringWriter())
                {
                    doc.Save(writer);
                    html = writer.ToString();
                }
            }

            return new HtmlString(Utils.Sanitize(html));
        }

        public static string SanitizeEdit(this string input)
        {
            var replaceTags = "script,iframe";

            foreach (var tag in replaceTags.Split(','))
            {
                var replaceRegex = new Regex(string.Format("<{0}.*?</{0}>", tag), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var replaceRegexMatches = replaceRegex.Matches(input);
                for (var i = 0; i < replaceRegexMatches.Count; i++)
                {
                    input = input.Replace(replaceRegexMatches[i].Value, string.Format("\n\n    {0}\n\n", replaceRegexMatches[i].Value));
                }
            }

            return input;
        }

        public static bool DetectSpam(this Comment comment)
        {
            var member = UmbracoContext.Current.Application.Services.MemberService.GetById(comment.MemberId);
            comment.IsSpam = SpamChecker.IsSpam(member, comment.Body);
            return comment.IsSpam;
        }

        public static bool DetectSpam(this Topic topic)
        {
            var member = UmbracoContext.Current.Application.Services.MemberService.GetById(topic.MemberId);
            topic.IsSpam = SpamChecker.IsSpam(member, string.Format("{0} {1}", topic.Title, topic.Body));
            return topic.IsSpam;
        }

        public static List<string> GetRoles(this IPublishedContent member)
        {
            var roles = Roles.GetRolesForUser(member.GetPropertyValue<string>("UserName"));
            var memberRoles = new List<string>();
            
            foreach (var role in roles)
            {
                 if(role == "standard" || role.StartsWith("201") || role.ToLowerInvariant().Contains("vendor".ToLowerInvariant()) || role.ToLowerInvariant().Contains("wiki".ToLowerInvariant()) || role.ToLowerInvariant().Contains("potentialspam".ToLowerInvariant()) || role.ToLowerInvariant().Contains("newaccount".ToLowerInvariant()))
                    continue;

                if (role == "CoreContrib")
                {
                    memberRoles.Add("c-trib");
                    continue;
                }

                memberRoles.Add(role.ToLower());
            }

            return memberRoles;
        }

        public static bool NewTopicsAllowed(this IPublishedContent forum)
        {
            return forum.Level <= 3 || forum.GetPropertyValue<bool>("forumAllowNewTopics");
        }
    }
}
