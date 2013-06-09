using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using umbraco;
using umbraco.cms.businesslogic.member;

namespace uForum.Library {
    public class Xslt {

        public string ReplaceCode(Match m)
        {
            if (m.Success)
                return "<code>" + HttpContext.Current.Server.HtmlEncode(m.Groups[0].Value).Replace("&lt;br /&gt;", "").Replace("<", "&lt;").Replace(">", "&gt;").Trim() + "</code>";
            else
                return "";
        }
        

        public static string CleanBBCode(string html) {

            string str = html;
            Regex exp;

            // format the code tags: [code]my site[code]
            // becomes: <code>my site</code>
            exp = new Regex(@"\[code\](.+?)\[/code\]", RegexOptions.Singleline | RegexOptions.IgnoreCase );
            
            Xslt x = new Xslt();
            MatchEvaluator exp_eval = new MatchEvaluator(x.ReplaceCode);
            // Replace matched characters using the delegate method.
            str = exp.Replace(str, exp_eval);

            // format the url tags: [url=www.website.com]my site[/url]
            // becomes: <a href="www.website.com">my site</a>
            exp = new Regex(@"\[quote\=([^\]]+)\]([^\]]+)\[/quote\]", RegexOptions.Singleline);
            str = exp.Replace(str, "<blockquote>$2</blockquote>");
                                   
            return str;
        }

        public static bool IsMemberInGroup(string GroupName, int memberid) {
                Member m = Utills.GetMember(memberid);

                foreach (MemberGroup mg in m.Groups.Values) {
                    if (mg.Text == GroupName)
                        return true;
                }
                return false;
        }

        public static bool IsInGroup(string GroupName) {

             if (umbraco.library.IsLoggedOn())
                 return IsMemberInGroup(GroupName, Member.CurrentMemberId());
             else
                 return false;
        }

        public static void RegisterRssFeed(string url, string title, string alias) { 
            umbraco.library.RegisterClientScriptBlock(alias, "<link rel=\"alternate\" type=\"application/rss+xml\" title=\"" + title + "\" href=\"" + url + "\" />" ,false);
        }

        public static string NiceTopicUrl(int topicId) {
            Businesslogic.Topic t = new uForum.Businesslogic.Topic(topicId);

            if (t.Exists) {
                string _url = umbraco.library.NiceUrl(t.ParentId);

                if (umbraco.GlobalSettings.UseDirectoryUrls) {
                    return "/" + _url.Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName;
                } else {
                    return "/" + _url.Substring(0, _url.LastIndexOf('.')).Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName + ".aspx";
                }
            } else {
                return "";
            }
        }


        public static int CommentPageNumber(int commentId, int itemsPerPage) {
                Businesslogic.Comment c = new uForum.Businesslogic.Comment(commentId);

                int position = c.Position - 1;

                return (int)(position / itemsPerPage);
        }


        public static string NiceCommentUrl(int topicId, int commentId, int itemsPerPage) {
            string url = NiceTopicUrl(topicId);
            if (!string.IsNullOrEmpty(url)) {
                Businesslogic.Comment c = new uForum.Businesslogic.Comment(commentId);

                int position = c.Position - 1;

                int page = (int)(position / itemsPerPage);
                

                url += "?p=" + page.ToString() + "#comment" + c.Id.ToString();
            }

            return url;
         }

		public static XPathNodeIterator ForumPager(int forumId, int itemsPerPage, int currentPage)
		{
			return ForumPager(forumId, itemsPerPage, currentPage, 10);
		}

		public static XPathNodeIterator ForumPager(int forumId, int itemsPerPage, int currentPage, int distance)
		{
			var xd = new XmlDocument();
			var totalTopics = 0;

			if (forumId == 0)
				totalTopics = Businesslogic.Topic.TotalTopics();
			else
			{
				var f = new uForum.Businesslogic.Forum(forumId);
				totalTopics = f.TotalTopics;
			}

			var pages = xmlHelper.addTextNode(xd, "pages", string.Empty);
			var i = 0;
			var p = 0;

			while (i < (totalTopics))
			{
				var distanceFromCurrent = p - currentPage;
				if (distanceFromCurrent > -distance && distanceFromCurrent < distance)
				{
					var page = xmlHelper.addTextNode(xd, "page", string.Empty);
					page.Attributes.Append(xmlHelper.addAttribute(xd, "index", p.ToString()));

					if (p == currentPage)
					{
						page.Attributes.Append(xmlHelper.addAttribute(xd, "current", "true"));
					}

					pages.AppendChild(page);
				}

				p++;
				i = (i + itemsPerPage);
			}

			return pages.CreateNavigator().Select(".");
		}

        public static XPathNodeIterator TopicPager(int topicId, int itemsPerPage, int currentPage) {
            XmlDocument xd = new XmlDocument();
            Businesslogic.Topic t = new uForum.Businesslogic.Topic(topicId);

            XmlNode pages = umbraco.xmlHelper.addTextNode(xd, "pages", "");

            int i = 0;
            int p = 0;

            while (i < (t.Replies)) {
                XmlNode page = umbraco.xmlHelper.addTextNode(xd, "page", "");
                page.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "index", p.ToString()));
                if(p == currentPage){
                    page.Attributes.Append( umbraco.xmlHelper.addAttribute(xd, "current", "true"));
                }
                pages.AppendChild(page);
                
                p++;
                i = (i + itemsPerPage);
            }

            return pages.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator MemberTopicPager(int memberId, int itemsPerPage, int currentPage)
        {
            XmlDocument xd = new XmlDocument();
            XmlNode pages = umbraco.xmlHelper.addTextNode(xd, "pages", "");

            int i = 0;
            int p = 0;
            int total = uForum.Businesslogic.Forum.TotalTopicsAndComments(memberId);

            while (i < (total))
            {
                XmlNode page = umbraco.xmlHelper.addTextNode(xd, "page", "");
                page.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "index", p.ToString()));
                if (p == currentPage)
                {
                    page.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "current", "true"));
                }
                pages.AppendChild(page);

                p++;
                i = (i + itemsPerPage);
            }

            return pages.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator AllForums(bool includeLatestData) {

          
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("forums");

            List<Businesslogic.Forum> forums = Businesslogic.Forum.Forums();
            foreach (Businesslogic.Forum f in forums) {
                x.AppendChild(f.ToXml(xd, includeLatestData));
            }

            return x.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator Forum(int id, bool includeLatestData) {
            XmlDocument xd = new XmlDocument();

           
            return new Businesslogic.Forum(id).ToXml(xd, includeLatestData).CreateNavigator().Select(".");

        }

        public static XPathNodeIterator Forums(int parentId, bool includeLatestData) {
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("forums");

            List<Businesslogic.Forum> forums = Businesslogic.Forum.Forums(parentId);
            foreach (Businesslogic.Forum f in forums) {
                x.AppendChild(f.ToXml(xd, includeLatestData));
            }

            return x.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator ForumTopics(int nodeId, int amount){
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("topics");

            List<Businesslogic.Topic> topics = Businesslogic.Topic.TopicsInForum(nodeId, amount, 1);
            foreach (Businesslogic.Topic t in topics) {
                x.AppendChild(t.ToXml(xd));
            }

            return x.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator Topic(int topicID) {
            XmlDocument xd = new XmlDocument();
            Businesslogic.Topic t = new uForum.Businesslogic.Topic(topicID);

            if (t.Exists)
                xd.AppendChild(t.ToXml(xd));

            return xd.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator Comment(int commentID) {

            XmlDocument xd = new XmlDocument();
            Businesslogic.Comment c = new uForum.Businesslogic.Comment(commentID);

            return c.ToXml(xd).CreateNavigator().Select(".");
        }

        public static XPathNodeIterator TopicComments(int topicID) {
            
            XmlDocument xd = new XmlDocument();
            Businesslogic.Topic t = new uForum.Businesslogic.Topic(topicID);

            if (t.Exists) {
                XmlNode comments = umbraco.xmlHelper.addTextNode(xd, "comments", "");

                foreach (Businesslogic.Comment cc in t.Comments()) {
                    comments.AppendChild( cc.ToXml(xd) );
                }

                xd.AppendChild(comments);
            }

            return xd.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator LatestTopics(int amount) {
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("topics");

            List<Businesslogic.Topic> topics = Businesslogic.Topic.Latest(amount);
            foreach (Businesslogic.Topic t in topics) {
                x.AppendChild(t.ToXml(xd));
            }

            return x.CreateNavigator().Select(".");
        }

        public static string Sanitize(string html) {
            return Utills.Sanitize(html);
        }
    
        public static string TimeDiff(string secondDate)
        {
            DateTime date;
            if (!DateTime.TryParse(secondDate, out date))
                return secondDate;

            var TS = DateTime.Now.Subtract(date);
            var span = int.Parse(Math.Round(TS.TotalSeconds, 0).ToString());

            if (span < 60)
                return "1 minute ago";

            if (span >= 60 && span < 3600)
                return string.Concat(Math.Round(TS.TotalMinutes), " minutes ago");

            if (span >= 3600 && span < 7200)
                return "1 hour ago";

            if (span >= 3600 && span < 86400)
                return string.Concat(Math.Round(TS.TotalHours), " hours ago");

            if (span >= 86400 && span < 172800)
                return "1 day ago";

            if (span >= 172800 && span < 604800)
                return string.Concat(Math.Round(TS.TotalDays), " days ago");

            if (span >= 604800 && span < 1209600)
                return "1 week ago";

            return date.ToString("MMMM d, yyyy @ hh:mm");
        }


        private static readonly Regex linkFinderRegex = new Regex("(((?<!src=\")http://|(?<!src=\".+)www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=%\\-_\\./]{2,})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex anchorFinderRegex = new Regex("(<a(.*?)href=\"(.*?)\"(.*?)>)(.*?)(</a>)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex preTagFinderRegex = new Regex("(<pre(.*?)>)(.*?)(</pre>)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly string link = "<a rel=\"nofollow\" href=\"{0}{1}\">{2}</a>";
        private static readonly string imgLink = "<img src=\"{0}{1}\" border=\"0\" alt=\"\" />";
        private static readonly Regex Base64ImageRegex = new Regex("(<img src=\"data:image(.*?)/>)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex imageTagFinderRegex = new Regex("(<img(.*?)/>)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public static void TopicNotFound(int topicId)
        {
            HttpContext.Current.Response.RedirectPermanent(string.Concat("/?topicNotFound=", topicId));
        }

        public static string ResolveLinks(string body)
        {



            if (string.IsNullOrEmpty(body))
                return body;

            //REMOVE BASE64 Images
            body = Base64ImageRegex.Replace(body, "");


            Dictionary<string, string> replaceValues = new Dictionary<string, string>();

            int k = 0;
            foreach (Match match in preTagFinderRegex.Matches(body))
            {
                string placeholder = string.Format("[[[[PreTagReplace{0}]]]]", k);
                body = body.Replace(match.Value, placeholder);
                replaceValues.Add(placeholder, "<pre>" + match.Groups[3].Value + "</pre>");
                k++;
            }

            int j = 0;
            foreach (Match match in anchorFinderRegex.Matches(body))
            {
                string placeholder = string.Format("[[[[AnchorReplacer{0}]]]]", j);
                body = body.Replace(match.Value, placeholder);


                if (!match.Groups[5].Value.Contains("://") || imageTagFinderRegex.IsMatch(match.Groups[5].Value))
                {
                    if (imageTagFinderRegex.IsMatch(match.Groups[5].Value))
                    {
                        replaceValues.Add(placeholder, string.Format(imgLink, string.Empty, match.Groups[3].Value, HttpUtility.UrlDecode(match.Groups[5].Value)));
                    }
                    else
                    {
                        replaceValues.Add(placeholder, string.Format(link, string.Empty, match.Groups[3].Value, HttpUtility.UrlDecode(match.Groups[5].Value)));
                    }
                }
                else
                {
                    replaceValues.Add(placeholder, string.Format(link, string.Empty, match.Groups[3].Value, HttpUtility.UrlDecode(ShortenUrl(match.Groups[5].Value, 50))));
                }
                j++;
            }


            int i = 0;
            foreach (Match match in linkFinderRegex.Matches(body))
            {
                string placeholder = string.Format("[[[[LinkReplacer{0}]]]]", i);
                body = body.Replace(match.Value, placeholder);
                if (!match.Value.Contains("://"))
                {
                    replaceValues.Add(placeholder, string.Format(link, "http://", match.Value, HttpUtility.UrlDecode(ShortenUrl(match.Value, 50))));
                }
                else
                {
                    replaceValues.Add(placeholder, string.Format(link, string.Empty, match.Value, HttpUtility.UrlDecode(ShortenUrl(match.Value, 50))));
                }
                i++;
            }

    
            foreach (KeyValuePair<string, string> replaceValue in replaceValues)
            {
                body = body.Replace(replaceValue.Key, replaceValue.Value);
            }

            return body;
        }

        private static string ShortenUrl(string url, int max) {
            if (url.Length <= max)
                return url;

            try
            {
                // Remove the protocal
                int startIndex = url.IndexOf("://");

                if (startIndex > -1)
                    url = url.Substring(startIndex + 3);

                if (url.Length <= max)
                    return url;

                // Remove the folder structure
                int firstIndex = url.IndexOf("/") + 1;
                int lastIndex = url.LastIndexOf("/");
                if (firstIndex < lastIndex)
                    url = url.Replace(url.Substring(firstIndex, lastIndex - firstIndex), "...");

                if (url.Length <= max)
                    return url;

                // Remove URL parameters
                int queryIndex = url.IndexOf("?");
                if (queryIndex > -1)
                    url = url.Substring(0, queryIndex);

                if (url.Length <= max)
                    return url;

                // Remove URL fragment
                int fragmentIndex = url.IndexOf("#");
                if (fragmentIndex > -1)
                    url = url.Substring(0, fragmentIndex);

                if (url.Length <= max)
                    return url;

                // Shorten page
                firstIndex = url.LastIndexOf("/") + 1;
                lastIndex = url.LastIndexOf(".");
                if (lastIndex - firstIndex > 10)
                {
                    string page = url.Substring(firstIndex, lastIndex - firstIndex);
                    int length = url.Length - max + 3;
                    url = url.Replace(page, "..." + page.Substring(length));
                }


                return url;
            }
            catch {
                return url;
            }
        }
    
    
    }
}
