using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;

namespace OurUmbraco.Community.BlogPosts {

    /// <summary>
    /// Class with information about a community blog.
    /// </summary>
    public class BlogInfo {

        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string Url { get; private set; }

        public string RssUrl { get; private set; }

        public string LogoUrl { get; internal set; }

        public int MemberId { get; private set; }

        // Blog has no category info and posts things unrelated to Umbraco, check there's related keywords in the title
        public bool CheckTitles { get; private set; }

        public bool HasLogoUrl {
            get { return !String.IsNullOrWhiteSpace(LogoUrl); }
        }

        /// <summary>
        /// Gets the encoding of the RSS feed. Currently this is just hardcoded to <see cref="System.Text.Encoding.UTF8"/>.
        /// </summary>
        public Encoding Encoding { get; set; }

        private BlogInfo(JObject obj) {
            Id = obj.GetString("id", Guid.Parse);
            Title = obj.GetString("title");
            Url = obj.GetString("url");
            RssUrl = obj.GetString("rss");
            LogoUrl = obj.GetString("logo");
            MemberId = obj.GetInt32("memberId");
            CheckTitles = obj.GetBoolean("checkTitles");
            Encoding = Encoding.UTF8;
        }

        public static BlogInfo Parse(JObject obj) {
            return obj == null ? null : new BlogInfo(obj);
        }

    }

}