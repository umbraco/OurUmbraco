using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using OurUmbraco.Our.Businesslogic;
using umbraco.BusinessLogic;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    [Authorize]
    public class CommunityController : UmbracoApiController
    {
        [HttpGet]
        public void AddTag(int nodeId, string group, string tag)
        {
            var cleanedtag = tag.Replace("<", "").Replace("'", "").Replace("\"", "").Replace(">", "");
            umbraco.editorControls.tags.library.addTagsToNode(nodeId, cleanedtag, group);
        }

        [HttpGet]
        public void RemoveTag(int nodeId, string group, string tag)
        {
            umbraco.editorControls.tags.library.RemoveTagFromNode(nodeId, tag, group);
        }

        public static void SetTags(string nodeId, string group, string tags)
        {
            int tagId = 0;

            //first clear out all items associated with this ID...
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND ([group] = @group));",
                    sqlHelper.CreateParameter("@nodeId", nodeId),
                    sqlHelper.CreateParameter("@group", group));

                //and now we add them again...
                foreach (var tag in tags.Split(','))
                {
                    var cleanedtag = tag.Replace("<", "");
                    cleanedtag = cleanedtag.Replace("'", "");
                    cleanedtag = cleanedtag.Replace("\"", "");
                    cleanedtag = cleanedtag.Replace(">", "");

                    if (cleanedtag.Length <= 0)
                        continue;

                    try
                    {
                        tagId = umbraco.editorControls.tags.library.AddTag(cleanedtag, @group);

                        if (tagId > 0)
                        {
                            sqlHelper.ExecuteNonQuery(
                                "INSERT INTO cmsTagRelationShip(nodeId,tagId) VALUES (@nodeId, @tagId)",
                                sqlHelper.CreateParameter("@nodeId", nodeId),
                                sqlHelper.CreateParameter("@tagId", tagId)
                            );

                            tagId = 0;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        [HttpGet]
        public string ChangeCollabStatus(int projectId, bool status)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var currentMember = memberShipHelper.GetCurrentMemberId();

            if (currentMember <= 0)
                return "false";

            var contentService = ApplicationContext.Services.ContentService;
            var content = contentService.GetById(projectId);

            if (content.GetValue<int>("owner") != currentMember)
                return "false";

            content.SetValue("openForCollab", status);
            var result = contentService.PublishWithStatus(content);
            return result.Success.ToString();
        }

        [HttpGet]
        public string RemoveContributor(int projectId, int memberId)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var currentMember = memberShipHelper.GetCurrentMemberId();

            if (currentMember <= 0)
                return "false";

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.Content(projectId);

            if (content.GetPropertyValue<int>("owner") != currentMember)
                return "false";

            var projectContributor = new ProjectContributor(projectId, memberId);
            projectContributor.Delete();

            return "true";
        }


        // Avatar upload
        [HttpPost]
        public HttpResponseMessage ImageUpload()
        {
            var httpRequest = HttpContext.Current.Request;
            var allowedSuffixes = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            dynamic result = new ExpandoObject();
            if (httpRequest.Files.Count > 0)
            {
                var filename = string.Empty;

                var guid = Guid.NewGuid();

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    if(postedFile == null)
                        continue;

                    // only allow files with certain extensions
                    if(allowedSuffixes.Contains(postedFile.FileName.Substring(postedFile.FileName.LastIndexOf(".", StringComparison.Ordinal))) == false)
                        continue;

                    var updir = new DirectoryInfo(HttpContext.Current.Server.MapPath("/media/upload/" + guid));

                    if (!updir.Exists)
                        updir.Create();

                    var filePath = string.Format("{0}/{1}", updir.FullName, postedFile.FileName);
                    postedFile.SaveAs(filePath);
                    filename = postedFile.FileName;
                }

                result.success = true;
                result.imagePath = string.Format("/media/upload/{0}/{1}", guid, filename);
            }
            else
            {
                result.success = false;
                result.message = "No images found";
            }

            //jquery ajax file uploader expects html, it parses to json client side
            var response = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(result)) };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
