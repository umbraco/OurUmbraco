using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco;
using umbraco.cms.businesslogic.template;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace uForum {
    public class iNotFoundHandler : umbraco.interfaces.INotFoundHandler {
        #region INotFoundHandler Members

        private int _redirectID = -1;

        public bool CacheUrl {
            get { return false; }
        }

        public bool Execute(string url) {

            HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler","init");

            bool _succes = false;
            url = url.Replace(".aspx", string.Empty);
            string currentDomain = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            if (url.Length > 0) {
                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

                XmlNode urlNode = null;
                string topicId = "";
                string topicTitle = "";

                HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler url", url);


                // We're not at domain root
                if (url.IndexOf("/") != -1) {
                    
                    string theRealUrl = url.Substring(0, url.LastIndexOf("/"));
                    string realUrlXPath = CreateXPathQuery(theRealUrl, true);

                    urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
                    topicTitle = url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1).ToLower();
                    topicId = topicTitle.Split('-')[0];
                    topicTitle = topicTitle.Substring(topicTitle.IndexOf('-') + 1);

                    if(urlNode != null)
                        HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler", topicId + " " + urlNode.Name);
                }

                if (urlNode != null && topicId != "" && urlNode.Name == "Forum") {
                    _redirectID = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);

                    var cookie = new HttpCookie("altTemplate", "displaytopic");
                    HttpContext.Current.Request.Cookies.Add(cookie);
                    HttpContext.Current.Items["altTemplate"] = "displaytopic";
                    HttpContext.Current.Items["topicID"] = topicId;
                    HttpContext.Current.Items["topicTitle"] = topicTitle.Replace('-',' ');

                    HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler",
                                                    string.Format("Templated changed to: '{0}'",
                                                                  HttpContext.Current.Items["altTemplate"]));
                    _succes = true;
                }
            }
            return _succes;            
        }

        public int redirectID {
            get {
                return _redirectID;
            }
        
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
        #endregion
    }
}
