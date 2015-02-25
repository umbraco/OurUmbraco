using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using uDocumentation.Busineslogic;
using umbraco;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using umbraco.interfaces;
using umbraco.NodeFactory;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace uDocumentation
{
    public class DocumentationContentFinder : IContentFinder
    {
        public bool TryFindContent(PublishedContentRequest contentRequest)
        {
            bool succes = false;

            // Added because in 4.7.1 for some inexplicable reason, the trailing slash 
            // is being stripped before we get the url in the NotFoundHandler
            
            var url = HttpContext.Current.Items["UmbPage"].ToString();
            if (url.StartsWith("/"))
                url = url.Substring(1);

            url = url.Replace(".aspx", string.Empty);


            if (url.Length > 0 && (url.ToLower().StartsWith(MarkdownLogic.BaseUrl) || url.ToLower().Contains("/documentation/")) && !IsImage(url))
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
                    string realUrlXPath = CreateXPathQuery(theRealUrl, true);

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
                        HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey] = markdownPath;
                        HttpContext.Current.Items["topicTitle"] = string.Join(" - ", HttpContext.Current.Request.RawUrl
                            .Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries)
                            .Skip(1)
                            .Reverse());
                        
                        var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                        var content = umbracoHelper.TypedContent(urlNode.Attributes["id"].Value);
                        contentRequest.PublishedContent = content;
                        
                        const string altTemplate = "DocumentationSubpage"; 
                        var templateIsSet = contentRequest.TrySetTemplate(altTemplate);

                        HttpContext.Current.Trace.Write("Markdown Files Handler",
                                                        string.Format("Templated changed to: '{0}' is {1}", altTemplate, templateIsSet));
                        succes = true;
                    }
                }
            }

            return succes;
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

        private const string PageXPathQueryStart = "/root";
        private const string UrlName = "@urlName";

        public static string CreateXPathQuery(string url, bool checkDomain)
        {

            string _tempQuery = "";
            if (GlobalSettings.HideTopLevelNodeFromPath && checkDomain)
            {
                _tempQuery = "/root" + GetChildContainerName() + "/*";
            }
            else if (checkDomain)
                _tempQuery = "/root" + GetChildContainerName();


            string[] requestRawUrl = url.Split("/".ToCharArray());

            // Check for Domain prefix
            string domainUrl = "";
            if (checkDomain && Domain.Exists(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]))
            {
                // we need to get the node based on domain
                INode n = new Node(Domain.GetRootFromDomain(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]));
                domainUrl = n.UrlName; // we don't use niceUrlFetch as we need more control
                if (n.Parent != null)
                {
                    while (n.Parent != null)
                    {
                        n = n.Parent;
                        domainUrl = n.UrlName + "/" + domainUrl;
                    }
                }
                domainUrl = "/" + domainUrl;

                // If at domain root
                if (url == "")
                {
                    _tempQuery = "";
                    requestRawUrl = domainUrl.Split("/".ToCharArray());
                    HttpContext.Current.Trace.Write("requestHandler",
                                                    "Redirecting to domain: " +
                                                    HttpContext.Current.Request.ServerVariables["SERVER_NAME"] +
                                                    ", nodeId: " +
                                                    Domain.GetRootFromDomain(
                                                        HttpContext.Current.Request.ServerVariables["SERVER_NAME"]).
                                                        ToString());
                }
                else
                {
                    // if it matches a domain url, skip all other xpaths and use this!
                    string langXpath = CreateXPathQuery(domainUrl + "/" + url, false);
                    if (content.Instance.XmlContent.DocumentElement.SelectSingleNode(langXpath) != null)
                        return langXpath;
                }
            }
            else if (url == "" && !GlobalSettings.HideTopLevelNodeFromPath)
                _tempQuery += "/*";

            bool rootAdded = false;
            if (GlobalSettings.HideTopLevelNodeFromPath && requestRawUrl.Length == 1)
            {
                HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");
                if (_tempQuery == "")
                    _tempQuery = "/root" + GetChildContainerName() + "/*";
                _tempQuery = "/root" + GetChildContainerName() + "/* [" + UrlName +
                             " = \"" + requestRawUrl[0].Replace(".aspx", "").ToLower() + "\"] | " + _tempQuery;
                HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");
                rootAdded = true;
            }


            for (int i = 0; i <= requestRawUrl.GetUpperBound(0); i++)
            {
                if (requestRawUrl[i] != "")
                    _tempQuery += GetChildContainerName() + "/* [" + UrlName + " = \"" + requestRawUrl[i].Replace(".aspx", "").ToLower() +
                                  "\"]";
            }

            if (GlobalSettings.HideTopLevelNodeFromPath && requestRawUrl.Length == 2)
            {
                _tempQuery += " | " + PageXPathQueryStart + GetChildContainerName() + "/* [" + UrlName + " = \"" +
                              requestRawUrl[1].Replace(".aspx", "").ToLower() + "\"]";
            }
            HttpContext.Current.Trace.Write("umbracoRequestHandler", "xpath: '" + _tempQuery + "'");

            Debug.Write(_tempQuery + "(" + PageXPathQueryStart + ")");

            if (checkDomain)
                return _tempQuery;
            else if (!rootAdded)
                return PageXPathQueryStart + _tempQuery;
            else
                return _tempQuery;
        }

        private static string GetChildContainerName()
        {
            if (string.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME) == false)
                return "/" + UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME;
            return "";
        }
    }

    public class MyTest : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
            ContentFinderResolver.Current
                .InsertTypeBefore<ContentFinderByNiceUrl, DocumentationContentFinder>();
        }
    }
}
