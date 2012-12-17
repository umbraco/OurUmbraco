using System;
using System.Collections.Generic;
using System.Xml;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco;
using umbraco.cms.businesslogic.template;

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
                    string realUrlXPath = requestHandler.CreateXPathQuery(theRealUrl, true);

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

        #endregion
    }
}
