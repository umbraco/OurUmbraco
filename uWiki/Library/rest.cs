using System;
using System.Collections.Generic;
using System.Web;
using umbraco.cms.businesslogic.web;
using System.Xml.XPath;
using System.Xml;
using uWiki.Businesslogic;
using System.Web.Security;

namespace uWiki.Library {
    public class Rest {
        //public static int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

        public static string FileUpload(string pageVersion, string memberGuid) {
             return "";
        }

        public static string Create(int parentID) {
            string _body = HttpContext.Current.Request["body"];
            string _title = HttpContext.Current.Request["title"];
            string _keywords = HttpContext.Current.Request["keywords"];

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (parentID > 0 && _currentMember > 0) {

                bool isAdmin = (Library.Xslt.IsInGroup("admin") || Library.Xslt.IsInGroup("wiki editor"));
                umbraco.cms.businesslogic.web.Document doc = new umbraco.cms.businesslogic.web.Document(parentID);
                bool isLocked = (doc.getProperty("umbracoNoEdit").Value.ToString() == "1");
                
                if ((isAdmin || !isLocked) && doc.ContentType.Alias == "WikiPage") {
                    Businesslogic.WikiPage wp = Businesslogic.WikiPage.Create(parentID, _currentMember, _body, _title, _keywords);
                    return umbraco.library.NiceUrl(wp.NodeId);
                }
            }

            return "";
        }

        public static string Update(int ID) {
            int _currentMember = umbraco.cms.businesslogic.member.Member.CurrentMemberId();
            string _body = HttpContext.Current.Request["body"];
            string _title = HttpContext.Current.Request["title"];
            string _keywords = HttpContext.Current.Request["keywords"];

            bool isAdmin = (Library.Xslt.IsInGroup("admin") || Library.Xslt.IsInGroup("wiki editor"));

            if (ID > 0 && _currentMember > 0 && _body.Trim() != "" && _title.Trim() != "") {

                Businesslogic.WikiPage wp = new uWiki.Businesslogic.WikiPage(ID);
                                
                if (wp.Exists && (isAdmin || !wp.Locked)) {

                    wp.Title = _title;
                    wp.Author = _currentMember;
                    wp.Body = _body;
                    wp.Keywords = _keywords;
                    wp.Save();
                   
                    return umbraco.library.NiceUrl(wp.NodeId);
                }

                return "not allowed " + isAdmin.ToString() + " " + wp.Locked + " " + wp.Exists;
            }

            return "";
        }

        public static XPathNodeIterator GetContentVersion(int id, string guid) {
            return Xslt.GetXmlNodeFromVersion(id, guid, false);
        }

        public static string Move(int ID, int target)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", _currentMember) || Xslt.IsMemberInGroup("wiki editor", _currentMember))
            {
                Document d = new Document(ID);
                Document t = new Document(target);
                
                if(t.ContentType.Alias == "WikiPage"){

                    Document o = new Document(d.Parent.Id);

                    d.Move(t.Id);
                    d.Save();
                    
                    d.Publish(new umbraco.BusinessLogic.User(0));
                    t.Publish(new umbraco.BusinessLogic.User(0));
                    o.Publish(new umbraco.BusinessLogic.User(0));
                                        
                    umbraco.library.UpdateDocumentCache(d.Id);
                    umbraco.library.UpdateDocumentCache(t.Id);
                    umbraco.library.UpdateDocumentCache(o.Id);

                    umbraco.library.RefreshContent();

                    return umbraco.library.NiceUrl(d.Id);
                }
                
            }

            return "";
        }

        public static string Delete(int ID)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", _currentMember) || Xslt.IsMemberInGroup("wiki editor", _currentMember))
            {
                Document d = new Document(ID);

                if (d != null)
                {
                    umbraco.library.UnPublishSingleNode(d.Id);
                    d.delete();
                }
            }

            return "";
        }


        public static string VerifyFile(int ID)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", _currentMember))
            {
                WikiFile wf = new WikiFile(ID);
                wf.Verified = true;
                wf.Save();
            }

            return "";
        }

        public static string Rollback(int ID, string guid) {

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            umbraco.cms.businesslogic.web.Document olddoc = new umbraco.cms.businesslogic.web.Document(ID, new Guid(guid));
            Businesslogic.WikiPage wp = new uWiki.Businesslogic.WikiPage(ID);
            
            if (olddoc != null && wp.Exists && !wp.Locked && _currentMember > 0 && wp.Version.ToString() != guid) {

                wp.Body =  olddoc.getProperty("bodyText").Value.ToString();
                wp.Title = olddoc.Text;
                wp.Author = _currentMember;
                wp.Save();

                return umbraco.library.NiceUrl(wp.NodeId);
            }
            
            return "";
        }

        public static void ClearHelpRequests(string section, string applicationPage)
        {

            Data.SqlHelper.ExecuteNonQuery(
             "Delete from wikiHelpRequest where section = @section and applicationPage = @applicationPage",
             Data.SqlHelper.CreateParameter("@section", section),
             Data.SqlHelper.CreateParameter("@applicationPage", applicationPage));
        }

    }
}
