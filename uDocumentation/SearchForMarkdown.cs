using System.IO;
using System.Web;
using System.Xml;
using uDocumentation.Busineslogic;
using umbraco;
using umbraco.interfaces;

namespace uDocumentation
{
    public class SearchForMarkdown : INotFoundHandler
    {
        private int _redirectId = -1;

        #region Implementation of INotFoundHandler

        public bool Execute(string url)
        {
            bool succes = false;

            // Added because in 4.7.1 for some inexplicable reason, the trailing slash 
            // is being stripped before we get the url in the NotFoundHandler
            url = HttpContext.Current.Items["UmbPage"].ToString();
            if (url.StartsWith("/"))
                url = url.Substring(1);

            url = url.Replace(".aspx", string.Empty);


            if (url.Length > 0 && (url.StartsWith(MarkdownLogic.BaseUrl) || url.Contains("/documentation/")) && !IsImage(url))
            {
                bool redirect = false;
                //take care of those versioned urls
                if (url.StartsWith(MarkdownLogic.BaseUrl + "/master") || url.StartsWith(MarkdownLogic.BaseUrl + "/v480"))
                {
                    //to kill the old requests and guide them to the root folder
                    url = url.Replace(MarkdownLogic.BaseUrl + "/master", MarkdownLogic.BaseUrl);
                    url = url.Replace(MarkdownLogic.BaseUrl + "/v480", MarkdownLogic.BaseUrl);
                }


                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

                var _url = url;
                redirect = doRedirect(_url, out url);

                if (redirect)
                    HttpContext.Current.Response.RedirectPermanent(url);


                XmlNode urlNode = null;
                bool notFound = true;
                string markdownPath = string.Empty;

                // We're not at domain root
                if (url.IndexOf("/") != -1)
                {
                    string theRealUrl = url.Substring(0, url.IndexOf("/"));
                    string realUrlXPath = requestHandler.CreateXPathQuery(theRealUrl, true);

                    urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
                    string markdownRelativePath = url.UnderscoreToDot().Replace('/', '\\').TrimEnd('\\');

                    string filePath = string.Concat(HttpRuntime.AppDomainAppPath, markdownRelativePath, ".md");
                    if (File.Exists(filePath))
                    {
                        notFound = false;
                        markdownPath = filePath;
                    }

                    if (notFound)
                    {
                        string indexPath = string.Concat(HttpRuntime.AppDomainAppPath, markdownRelativePath, "\\index.md");
                        if (File.Exists(indexPath))
                        {
                            notFound = false;
                            markdownPath = indexPath;
                        }
                    }
                }

                if (urlNode != null && !notFound)
                {
                    XmlAttribute legacyNodeTypeAliasAttribute = urlNode.Attributes["nodeTypeAlias"];
                    string nodeTypeAlias = legacyNodeTypeAliasAttribute == null ? string.Empty : legacyNodeTypeAliasAttribute.Value;
                    if (urlNode.Name == MarkdownLogic.DocumentTypeAlias || nodeTypeAlias == MarkdownLogic.DocumentTypeAlias || urlNode.Name == "Projects")
                    {
                        _redirectId = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);

                        HttpContext.Current.Items["altTemplate"] = MarkdownLogic.AlternativeTemplate.ToLower();
                        HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey] = markdownPath;

                        HttpContext.Current.Trace.Write("Markdown Files Handler",
                                                        string.Format("Templated changed to: '{0}'",
                                                                      HttpContext.Current.Items["altTemplate"]));
                        succes = true;
                    }
                }
            }

            return succes;
        }

        public bool CacheUrl
        {
            get { return false; }
        }

        public int redirectID
        {
            get
            {
                return _redirectId;
            }
        }

        private static string[] ImgExts = ".jpg,.jpeg,.gif,.png,.bmp".Split(',');
        private bool IsImage(string url)
        {
            foreach (var imgExt in ImgExts)
            {
                if (url.EndsWith(imgExt))
                    return true;
            }
            return false;
        }

        private static bool doRedirect(string url, out string returnurl)
        {
            if (url.EndsWith("/"))
            {
                string markdownRelativePath = url.UnderscoreToDot().Replace('/', '\\').TrimEnd('\\');
                string filePath = string.Concat(HttpRuntime.AppDomainAppPath, markdownRelativePath, "\\index.md");

                if (!File.Exists(filePath))
                {
                    returnurl = url.TrimEnd('/');
                    return true;
                }

            }
            else
            {
                string markdownRelativePath = url.UnderscoreToDot().Replace('/', '\\').TrimEnd('\\');
                string filePath = string.Concat(HttpRuntime.AppDomainAppPath, markdownRelativePath, ".md");
                if (!File.Exists(filePath))
                {
                    returnurl = url + "/";
                    return true;
                }
            }

            returnurl = url;
            return false;
        }
        #endregion
    }
}