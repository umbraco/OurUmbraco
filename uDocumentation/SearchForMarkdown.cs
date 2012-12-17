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
            url = url.Replace(".aspx", string.Empty);
            if (url.Length > 0 && (url.StartsWith(MarkdownLogic.BaseUrl) || url.StartsWith(string.Format("/{0}", MarkdownLogic.BaseUrl))))
            {
                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

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
                    if (urlNode.Name == MarkdownLogic.DocumentTypeAlias || nodeTypeAlias == MarkdownLogic.DocumentTypeAlias)
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

        #endregion
    }
}