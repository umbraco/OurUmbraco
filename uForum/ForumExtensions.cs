using HtmlAgilityPack;
using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using uForum.Library;
using uForum.Models;
using uForum.Services;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace uForum
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


        public static HtmlString Sanitize(this string html){
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
                    var replace = false;
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

                        replace = true;
                    }

                    if (replace)
                    {
                        html = root.OuterHtml;
                    }
                }
            }

            return new HtmlString(Utils.Sanitize(html));
        }

        public static bool DetectSpam(this Comment comment)
        {
            var member = Umbraco.Web.UmbracoContext.Current.Application.Services.MemberService.GetById(comment.MemberId);
            comment.IsSpam = AntiSpam.SpamChecker.IsSpam(member, comment.Body);
            return comment.IsSpam;
        }

        public static bool DetectSpam(this Topic topic)
        {
            var member = Umbraco.Web.UmbracoContext.Current.Application.Services.MemberService.GetById(topic.MemberId);
            topic.IsSpam = AntiSpam.SpamChecker.IsSpam(member, topic.Body);
            return topic.IsSpam;
        }
    }
}
