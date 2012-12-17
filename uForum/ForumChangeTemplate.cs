//using System.Linq;
//using System.Web;
//using System.Xml;
//using umbraco;

//namespace uForum
//{
//    public class ForumChangeTemplate : UmbracoDefault
//    {
//        public ForumChangeTemplate()
//        {
//            AfterRequestInit += HandleAfterRequestInit;
//        }

//        private static void HandleAfterRequestInit(object sender, RequestInitEventArgs eventArgs)
//        {
//            HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler", "init");

//            var url = HttpContext.Current.Request.RawUrl;

//            url = url.Replace(".aspx", string.Empty);

//            if (url.Length <= 0)
//                return;

//            if (url.Substring(0, 1) == "/")
//                url = url.Substring(1, url.Length - 1);

//            XmlNode urlNode = null;
//            string topicId = "";
//            string topicTitle = "";

//            HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler url", url);

//            // We're not at domain root
//            if (url.IndexOf("/") != -1)
//            {

//                string theRealUrl = url.Substring(0, url.LastIndexOf("/"));
//                string realUrlXPath = requestHandler.CreateXPathQuery(theRealUrl, true);

//                urlNode = content.Instance.XmlContent.SelectSingleNode(realUrlXPath);
//                topicTitle = url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf(("/")) - 1).ToLower();
//                topicId = topicTitle.Split('-')[0];
//                topicTitle = topicTitle.Substring(topicTitle.IndexOf('-') + 1);

//                HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler", topicId + " " + urlNode.Name);
//            }

//            if (urlNode == null || topicId == "" || urlNode.Name != "Forum") 
//                return;

//            var page = (UmbracoDefault) sender;
//            page.MasterPageFile = template.GetMasterPageName(eventArgs.Page.Template, "displaytopic");
            
//            HttpContext.Current.Items["RedirectID"] = int.Parse(urlNode.Attributes.GetNamedItem("id").Value);
//            HttpContext.Current.Items["topicID"] = topicId;
//            HttpContext.Current.Items["topicTitle"] = topicTitle.Replace('-', ' ');

//            HttpContext.Current.Trace.Write("umbraco.faltTemplateHandler", string.Format("Templated changed to: '{0}'", HttpContext.Current.Items["altTemplate"]));
//        }
//    }
//}