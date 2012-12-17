using System;
using System.Collections.Generic;
using System.Xml;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco;
using umbraco.cms.businesslogic.template;
using System.Configuration;

namespace uWiki {
    public class iNotFoundHandler : umbraco.interfaces.INotFoundHandler {
        #region INotFoundHandler Members

        private int _redirectID = -1;

        public bool CacheUrl {
            get { return false; }
        }

        public bool Execute(string url) {

            HttpContext.Current.Trace.Write("umbraco.wikiHandler", "init");

            bool _succes = false;
            url = url.Replace(".aspx", string.Empty);
            string currentDomain = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            if (url.Length > 0) {
                if (url.Substring(0, 1) == "/")
                    url = url.Substring(1, url.Length - 1);

                XmlNode urlNode = null;
                string topic = "";

                // We're not at domain root
                if (url.IndexOf("/") != -1) {

                    string theRealUrl = url.Substring(0, url.LastIndexOf("/"));
                    string realUrlXPath = requestHandler.CreateXPathQuery(theRealUrl, true);

                    urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
                    topic = url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1);

                }

                if (url.Contains("wiki/umbraco-help"))
                {
                    string[] urlparts = url.Split('/');

                    string section = urlparts[urlparts.Length - 2];
                    string application = "";
                    string applicationPage = urlparts[urlparts.Length - 1];

                    //log the request
                    Businesslogic.WikiHelpRequest.Create(section.ToLower(), application, applicationPage, url);

                    
                    //if it isn't a page create request
                    if (HttpContext.Current.Request["wikiEditor"] == null)
                    {
                        //set redirect id to main umbraco help page
                        if (Library.Utills.GetWikiHelpFallBackPage(section) > 0)
                        {
                            _redirectID = Library.Utills.GetWikiHelpFallBackPage(section);
                            return true;
                        }
                    }
               
                }


                if (urlNode != null && topic != "" && urlNode.Name == "WikiPage") {
                    _redirectID = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);

                    HttpContext.Current.Items["altTemplate"] = "createwikipage";
                    HttpContext.Current.Items["topic"] = url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1);

                    HttpContext.Current.Trace.Write("umbraco.altTemplateHandler",
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

        #endregion
    }
}
