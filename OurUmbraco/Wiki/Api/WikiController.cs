using System;
using System.Web.Http;
using OurUmbraco.Wiki.BusinessLogic;
using OurUmbraco.Wiki.Library;
using umbraco.cms.businesslogic.web;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Wiki.Api
{
    public class WikiController : UmbracoApiController
    {

        public string FileUpload(string pageVersion, string memberGuid)
        {
            return "";
        }

        [HttpPost]
        public string Create(int parentId, string body, string title, string keywords)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (parentId > 0 && currentMemberId > 0)
            {
                var isAdmin = (Xslt.IsInGroup("admin") || Xslt.IsInGroup("wiki editor"));
                var doc = new Document(parentId);
                var isLocked = (doc.getProperty("umbracoNoEdit").Value.ToString() == "1");

                if ((isAdmin || isLocked == false) && doc.ContentType.Alias == "WikiPage")
                {
                    var wikiPage = WikiPage.Create(parentId, currentMemberId, body, title, keywords);
                    return umbraco.library.NiceUrl(wikiPage.NodeId);
                }
            }

            return "";
        }

        [HttpPost]
        public string Update(int pageId, string body, string title, string keywords)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            var isAdmin = (Xslt.IsInGroup("admin") || Xslt.IsInGroup("wiki editor"));

            if (pageId > 0 && currentMemberId > 0 && body.Trim() != "" && title.Trim() != "")
            {

                var wikiPage = new WikiPage(pageId);

                if (wikiPage.Exists && (isAdmin || wikiPage.Locked == false))
                {

                    wikiPage.Title = title;
                    wikiPage.Author = currentMemberId;
                    wikiPage.Body = body;
                    wikiPage.Keywords = keywords;
                    wikiPage.Save();

                    return umbraco.library.NiceUrl(wikiPage.NodeId);
                }

                return "not allowed " + isAdmin + " " + wikiPage.Locked + " " + wikiPage.Exists;
            }

            return "";
        }

        [HttpPost]
        public string GetContentVersion(int id, string guid = null)
        {
            if (guid == null)
                guid = string.Empty;

            return Xslt.GetXmlNodeFromVersion(id, guid, false);
        }

        [HttpGet]
        public string Move(int wikiId, int target)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (Xslt.IsMemberInGroup("admin", currentMemberId) || Xslt.IsMemberInGroup("wiki editor", currentMemberId))
            {
                Document document = new Document(wikiId);
                Document documentTarget = new Document(target);

                if (documentTarget.ContentType.Alias == "WikiPage")
                {

                    Document o = new Document(document.Parent.Id);

                    document.Move(documentTarget.Id);
                    document.Save();

                    document.Publish(new umbraco.BusinessLogic.User(0));
                    documentTarget.Publish(new umbraco.BusinessLogic.User(0));
                    o.Publish(new umbraco.BusinessLogic.User(0));

                    umbraco.library.UpdateDocumentCache(document.Id);
                    umbraco.library.UpdateDocumentCache(documentTarget.Id);
                    umbraco.library.UpdateDocumentCache(o.Id);

                    umbraco.library.RefreshContent();

                    return umbraco.library.NiceUrl(document.Id);
                }

            }

            return "";
        }

        [HttpGet]
        public string Delete(int wikiId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;

            if (Xslt.IsMemberInGroup("admin", currentMemberId) || Xslt.IsMemberInGroup("wiki editor", currentMemberId))
            {
                var document = new Document(wikiId);

                umbraco.library.UnPublishSingleNode(document.Id);
                document.delete();
            }

            return "";
        }

        [HttpGet]
        public string VerifyFile(int fileId)
        {
            if (Xslt.IsMemberInGroup("admin", Members.GetCurrentMember().Id))
            {
                var wikiFile = new WikiFile(fileId) { Verified = true };
                wikiFile.Save();
            }

            return "";
        }

        [HttpPost]
        public string Rollback(int pageId, string guid)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            var olddoc = new Document(pageId, new Guid(guid));
            var wikiPage = new WikiPage(pageId);

            if (olddoc != null && wikiPage.Exists && !wikiPage.Locked && currentMemberId > 0 && wikiPage.Version.ToString() != guid)
            {

                wikiPage.Body = olddoc.getProperty("bodyText").Value.ToString();
                wikiPage.Title = olddoc.Text;
                wikiPage.Author = currentMemberId;
                wikiPage.Save();

                return umbraco.library.NiceUrl(wikiPage.NodeId);
            }

            return "";
        }

        [HttpGet]
        public void ClearHelpRequests(string section, string applicationPage)
        {
            Data.SqlHelper.ExecuteNonQuery(
             "Delete from wikiHelpRequest where section = @section and applicationPage = @applicationPage",
             Data.SqlHelper.CreateParameter("@section", section),
             Data.SqlHelper.CreateParameter("@applicationPage", applicationPage));
        }
    }
}
