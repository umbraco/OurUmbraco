﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using ImageProcessor.Imaging.Filters.Artistic;
using MarkdownSharp;
using uForum.AntiSpam;
using uForum.Library;
using uForum.Models;
using Umbraco.Web;

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

        public static bool DetectSpam(this Comment comment)
        {
            var member = UmbracoContext.Current.Application.Services.MemberService.GetById(comment.MemberId);
            comment.IsSpam = SpamChecker.IsSpam(member, comment.Body);
            return comment.IsSpam;
        }

        public static bool DetectSpam(this Topic topic)
        {
            var member = UmbracoContext.Current.Application.Services.MemberService.GetById(topic.MemberId);
            topic.IsSpam = SpamChecker.IsSpam(member, topic.Body);
            return topic.IsSpam;
        }
    }
}
