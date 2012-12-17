using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using umbraco.cms.businesslogic.web;
using System.Xml;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;

namespace uWiki.Library {
    public class Xslt {
        public static XPathNodeIterator PageHistory(int nodeId) {
            Document doc = new Document(nodeId);
            XmlDocument xd = new XmlDocument(); 

            XmlNode versions = umbraco.xmlHelper.addTextNode(xd, "versions", "");
                        
            if (doc != null) {
                DocumentVersionList[] dvlList = doc.GetVersions();
                PropertyType authorType = doc.getProperty("author").PropertyType;

                foreach (DocumentVersionList dvl in dvlList) {
                    
                    Document d = new Document(nodeId, dvl.Version);
                    int author = getPropertyFromVersion<int>(authorType, dvl.Version, "dataInt");
                    
                    XmlNode x = umbraco.xmlHelper.addTextNode(xd, "version", "");
                    x.AppendChild(umbraco.xmlHelper.addTextNode(xd, "author", author.ToString()));
                    x.AppendChild(umbraco.xmlHelper.addTextNode(xd, "guid", dvl.Version.ToString()));
                    x.AppendChild(umbraco.xmlHelper.addTextNode(xd, "name", dvl.Text));
                    x.AppendChild(umbraco.xmlHelper.addTextNode(xd, "date", dvl.Date.ToString("s")));


                    versions.AppendChild( x );

                    x = null;
                    d = null;
                }
            }

            return versions.CreateNavigator().Select(".");
        }


        private static T getPropertyFromVersion<T>(PropertyType pt, Guid version, string type)
        {
            return umbraco.BusinessLogic.Application.SqlHelper.ExecuteScalar<T>("select " + type + " from cmsPropertyData where versionId = @version AND propertyTypeId = @id",
                    umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@id", pt.Id),
                    umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@version", version)
                    );
        }

        public static XPathNodeIterator GetXmlNodeFromVersion(int id, string guid, bool deep) {

            Document d;

            if (!string.IsNullOrEmpty(guid))
                d = new Document(id, new Guid(guid));
            else
                d = new Document(id);

            XmlDocument xd = new XmlDocument();
            
            XmlNode x = umbraco.xmlHelper.addTextNode(xd, "node", "");
            d.XmlPopulate(xd, ref x, deep); 

            return x.CreateNavigator().Select(".");
        }

     

        public static XPathNodeIterator GetAttachedFiles(int id) {
            return Businesslogic.Data.GetDataSet("SELECT * FROM wikiFiles where nodeId = " + id.ToString(), "file");    
        }


        public static XPathNodeIterator GetAttachedFile(int Fileid) {
            return Businesslogic.Data.GetDataSet("SELECT * FROM wikiFiles where id = " + Fileid.ToString(), "file");
        }

        public static bool IsMemberInGroup(string GroupName, int memberid)
        {
            Member m = Utills.GetMember(memberid);

            foreach (MemberGroup mg in m.Groups.Values)
            {
                if (mg.Text == GroupName)
                    return true;
            }
            return false;
        }

        public static bool IsInGroup(string GroupName)
        {

            if (umbraco.library.IsLoggedOn())
                return IsMemberInGroup(GroupName, Member.CurrentMemberId());
            else
                return false;
        }

        public static void AddWikiHelpRequest(string section)
        {
            string url =  HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
            string application = "";
            string applicationPage = url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf("/") - 1);

            Businesslogic.WikiHelpRequest.Create(section.ToLower(), application, applicationPage,url);
        }
        public static XPathNodeIterator GetWikiHelpRequests()
        {
            return Businesslogic.Data.GetDataSet("SELECT applicationPage, COUNT(*) AS numberOfRequests FROM wikiHelpRequest GROUP BY applicationPage ", "requests");
        }

        public static XPathNodeIterator GetWikiHelpRequests(string section)
        {
            return Businesslogic.Data.GetDataSet("SELECT applicationPage, COUNT(*) AS numberOfRequests FROM wikiHelpRequest where section = '" + section + "' GROUP BY applicationPage", "requests");
        }

        public static XPathNodeIterator FindPackageForUmbracoVersion(int nodeid, string umbracoVersion)
        {
            XmlDocument xd = new XmlDocument();

            Businesslogic.WikiFile wf = Businesslogic.WikiFile.FindPackageForUmbracoVersion(nodeid, umbracoVersion);

            if (wf != null)
                xd.AppendChild(wf.ToXml(xd));

            return xd.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator FindPackageDocumentationForUmbracoVersion(int nodeid, string umbracoVersion)
        {
            XmlDocument xd = new XmlDocument();

            Businesslogic.WikiFile wf = Businesslogic.WikiFile.FindPackageDocumentationForUmbracoVersion(nodeid, umbracoVersion);

            if (wf != null)
                xd.AppendChild(wf.ToXml(xd));

            return xd.CreateNavigator().Select(".");
        }

    }
}
