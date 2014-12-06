using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;
using uWiki.Businesslogic;

namespace uRepo
{
    public class Packages
    {
        protected const int _projectsRoot = 1113;
        protected const string _projectAlias = "Project";
        protected const string _projectGroupAlias = "ProjectGroup";

        //used by the repo webservice
        public static SubmitStatus SubmitPackageAsProject(string authorGuid, string packageGuid, byte[] packageFile, byte[] packageDoc, byte[] packageThumbnail, string name, string description)
        {
            try
            {
                if (packageFile.Length == 0)
                    return SubmitStatus.Error;

                umbraco.cms.businesslogic.member.Member mem = new umbraco.cms.businesslogic.member.Member(new Guid(authorGuid));
                Package packageNode = uRepo.Packages.GetPackageByGuid( new Guid(packageGuid));

                if (mem != null)
                {
                    //existing package...
                    if (packageNode != null)
                    {
                        return SubmitStatus.Exists;
                    }
                    else
                    {
                        Document d = Document.MakeNew(name, DocumentType.GetByAlias(_projectAlias), new umbraco.BusinessLogic.User(0), _projectsRoot);

                        d.getProperty("version").Value = "1.0";
                        d.getProperty("description").Value = description;

                        d.getProperty("stable").Value = false;

                        d.getProperty("demoUrl").Value = "";
                        d.getProperty("sourceUrl").Value = "";
                        d.getProperty("websiteUrl").Value = "";

                        d.getProperty("licenseUrl").Value = "";
                        d.getProperty("licenseName").Value = "";

                        d.getProperty("vendorUrl").Value = "";

                        d.getProperty("owner").Value = mem.Id;
                        d.getProperty("packageGuid").Value = packageGuid;
                        
                        uWiki.Businesslogic.WikiFile wf = uWiki.Businesslogic.WikiFile.Create(name,"zip", d.UniqueId, mem.UniqueId, packageFile, "package", new List<UmbracoVersion>(){UmbracoVersion.DefaultVersion()});
                        d.getProperty("file").Value = wf.Id;

                        //Create Documentation
                        if (packageDoc.Length > 0)
                        {
                            uWiki.Businesslogic.WikiFile doc = uWiki.Businesslogic.WikiFile.Create("documentation", "pdf", d.UniqueId, mem.UniqueId, packageDoc, "docs", new List<UmbracoVersion>() { UmbracoVersion.DefaultVersion() });
                            d.getProperty("documentation").Value = doc.Id;
                        }

                        d.XmlGenerate(new XmlDocument());
                        d.Save();

                        d.Publish(new umbraco.BusinessLogic.User(0));
                        umbraco.library.UpdateDocumentCache(d.Id);

                        return SubmitStatus.Complete;
                    }
                }
                else
                {
                    return SubmitStatus.NoAccess;
                }
            }
            catch (Exception ex)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                return SubmitStatus.Error;
            }
        }

        public static Package GetPackageById(int id)
        {
            return convertToPackageFromNode(new Node(id));
        }

        public static Package GetPackageByGuid(Guid guid)
        {
            XPathNodeIterator xpn = umbraco.library.GetXmlNodeByXPath("descendant::node[data [@alias = 'packageGuid'] = '" + guid.ToString() + "']");

            if (xpn.MoveNext())
            {
               if(xpn.Current is IHasXmlNode)
              {
                XmlNode node = ((IHasXmlNode)xpn.Current).GetNode();
                return convertToPackageFromXmlNode(node);
              }
            }

            return null;
        }

        public static List<Package> GetAllPackages()
        {
            string xpath = "/descendant::node [@nodeTypeAlias = '" + _projectAlias + "']";

            List<Package> ps = new List<Package>();

            XPathNodeIterator xni = queryRepo(xpath);
            while(xni.MoveNext()){
            if(xni.Current is IHasXmlNode)
              {
                XmlNode node = ((IHasXmlNode)xni.Current).GetNode();
                ps.Add(convertToPackageFromXmlNode(node));
              }
            }
            ps.Sort();

            return ps;
        }

        public static List<Package> GetPackagesByTag(string tag)
        {
            return new List<Package>();
        }

        public static List<Package> GetPackagesByCategory(int category)
        {
            string xpath = "/node [@id = " + category.ToString() + "]/descendant::node [@nodeTypeAlias = '" + _projectAlias + "']";
            XPathNodeIterator xni = queryRepo(xpath);
            return iteratorToPackageList(xni);
        }

        public static List<Package> GetPackagesByProperty(string propertyAlias, string value) {
          string xpath = "//" + _projectAlias + "[" + propertyAlias + " = '" + value + "']";
          XPathNodeIterator xni = queryRepo(xpath);
          return iteratorToPackageList(xni);
        }
        public XPathNodeIterator GetProjects()
        {
            const string xpath = "//" + _projectAlias;
            return queryRepo(xpath);
        }

        public static List<Category> GetPackagesByPropertyCategorized(string propertyAlias, string value) {
          string xpath = "/descendant::node [@nodeTypeAlias = '" + _projectGroupAlias + "']";
          
          XPathNodeIterator xni = queryRepo(xpath);

          List<Category> ps = new List<Category>();

          while (xni.MoveNext()) {
            if (xni.Current is IHasXmlNode) {
              XmlNode node = ((IHasXmlNode)xni.Current).GetNode();
              
              Category c = convertToCategoryFromXmlNode(node, false);

              string pXpath = "./descendant::node [@nodeTypeAlias = '" + _projectAlias + "' and data [@alias = '" + propertyAlias + "'] = '" + value + "']";

              XPathNodeIterator pI = xni.Current.Select(pXpath);
              while (pI.MoveNext()) {
                if (pI.Current is IHasXmlNode) {
                  XmlNode pNode = ((IHasXmlNode)pI.Current).GetNode();
                  Package p = convertToPackageFromXmlNode(pNode);

                  if(p != null)
                    c.Packages.Add(p);
                }
              }
              
              if (c != null && c.Packages.Count > 0)
                ps.Add(c);
            }
          }

          ps.Sort();

          return ps;
        }

        public static List<Package> GetPackagesByCategory(string categoryName)
        {
            string xpath = "/node [@nodeName = " + categoryName + "]/descendant::node [@nodeTypeAlias = '" + _projectAlias + "']";
            XPathNodeIterator xni = queryRepo(xpath);
            return iteratorToPackageList(xni);
        }

        public static List<Category> GetSubCategories(string categoryName, bool includePackages)
        {
            string xpath = "/node [@nodeName = " + categoryName + "]/descendant::node [@nodeTypeAlias = '" + _projectGroupAlias + "']";
            XPathNodeIterator xni = queryRepo(xpath);

            return iteratorToCategoryList(xni, includePackages);
        }

        public static List<Category> GetSubCategories(int id, bool includePackages)
        {
            string xpath = "/node [@id = " + id.ToString() + "]/descendant::node [@nodeTypeAlias = '" + _projectGroupAlias + "']";
            XPathNodeIterator xni = queryRepo(xpath);

            return iteratorToCategoryList(xni, includePackages);
        }

        public static List<Category> Categories(bool includePackages, bool hideHQCategories)
        {
            string xpath = "/" +  _projectGroupAlias;
            if(hideHQCategories)
                xpath += " [hqOnly != '1']";

            XPathNodeIterator xpn = queryRepo(xpath);
            return iteratorToCategoryList(xpn, includePackages);
        }

        public static List<Package> Search(string term)
        {
            return new List<Package>();
        }

        //general method for fetching repository specific data... 
        private static XPathNodeIterator queryRepo(string xpath)
        {
            return umbraco.library.GetXmlNodeByXPath("descendant-or-self::*[@isDoc and @id = " + _projectsRoot.ToString() + "]" + xpath);
        }
        
        private static List<Package> iteratorToPackageList(XPathNodeIterator xni)
        {
            List<Package> ps = new List<Package>();

            while (xni.MoveNext())
            {
                if (xni.Current is IHasXmlNode)
                {
                    XmlNode node = ((IHasXmlNode)xni.Current).GetNode();
                    Package p = convertToPackageFromXmlNode(node);

                    if (p != null)
                        ps.Add(p);
                }
            }

            ps.Sort();

            return ps;
        }

        private static List<Category> iteratorToCategoryList(XPathNodeIterator xni, bool includePackages)
        {
            List<Category> ps = new List<Category>();

            while (xni.MoveNext())
            {
                if (xni.Current is IHasXmlNode)
                {
                    XmlNode node = ((IHasXmlNode)xni.Current).GetNode();
                    Category c = convertToCategoryFromXmlNode(node, includePackages);
                    if (c != null)
                        ps.Add(c);
                }
            }

            ps.Sort();

            return ps;
        }


        private static Package convertToPackageFromXmlNode(XmlNode node)
        {
            return convertToPackageFromNode(new Node(node));
        }

        private static Package convertToPackageFromNode(Node packageNode)
        {
                if (packageNode != null && packageNode.NodeTypeAlias == _projectAlias)
                {
                    Package retVal = new Package();

                    retVal.Text = packageNode.Name;
                    retVal.RepoGuid = new Guid(safeProperty(packageNode,"packageGuid"));
                    retVal.Description = safeProperty(packageNode,"description");
                    retVal.Protected = false;
                    
                    retVal.Icon = "";
                    retVal.Thumbnail = safeProperty(packageNode, "defaultScreenshotPath");
                    retVal.Demo = "";

                    retVal.IsModule = false;
                    if (safeProperty(packageNode, "isModule") == "1")
                      retVal.IsModule = true;

                    if (umbraco.library.NiceUrl(packageNode.Id) != "")
                    {
                        retVal.Url = umbraco.library.NiceUrl(packageNode.Id);
                        retVal.Accepted = true;
                    }

                    retVal.HasUpgrade = false;

                    return retVal;
                }

            return null;
        }

        private static Category convertToCategoryFromXmlNode(XmlNode node, bool includePackages)
        {
            return convertToCategoryFromNode(new Node(node), includePackages);
        }

        private static Category convertToCategoryFromNode(Node categoryNode, bool includePackages)
        {
            if (categoryNode != null && categoryNode.NodeTypeAlias == _projectGroupAlias)
                {
                    Category retVal = new Category();

                    retVal.Text = categoryNode.Name;
                    retVal.Id = categoryNode.Id;
                    retVal.Description = safeProperty(categoryNode,"description");
                    retVal.Url = umbraco.library.NiceUrl(categoryNode.Id); 
                    
                    if(includePackages){
                        foreach(Node p in categoryNode.Children){
                            Package pack = convertToPackageFromNode(p);
                            if(pack != null)
                                retVal.Packages.Add(pack);
                        }
                    }

                    return retVal;
                }

            return null;
        }
        
        private static string safeProperty(umbraco.presentation.nodeFactory.Node n, string alias)
        {
            if (n.GetProperty(alias) != null && !string.IsNullOrEmpty(n.GetProperty(alias).Value))
                return n.GetProperty(alias).Value;
            else
                return string.Empty;
        }

        internal static uWiki.Businesslogic.WikiFile PackageFileByGuid(Guid pack)
        {
            XPathNodeIterator xpn = umbraco.library.GetXmlNodeByXPath("descendant::* [@isDoc and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = translate('" + pack.ToString() + "','ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')]");

            if (xpn.MoveNext())
            {
                if (xpn.Current is IHasXmlNode)
                {
                    Node node = new Node(((IHasXmlNode)xpn.Current).GetNode());
                    string fileId = safeProperty(node, "file");
                    int _id;

                    if (int.TryParse(fileId, out _id))
                    {
                        string cookieName = "ProjectFileDownload" + fileId;

                        //we clear the cookie on the server just to be sure the download is accounted for
                        if (HttpContext.Current.Request.Cookies[cookieName] != null)
                        {
                            HttpCookie myCookie = new HttpCookie(cookieName);
                            myCookie.Expires = DateTime.Now.AddDays(-1d);
                            HttpContext.Current.Response.Cookies.Add(myCookie);
                        }
                        
                        uWiki.Businesslogic.WikiFile wf = new uWiki.Businesslogic.WikiFile(_id);
                        wf.UpdateDownloadCounter(true,true);
                        
                        return wf;
                    }
                }
            }

            return null;

        }
    }
}
