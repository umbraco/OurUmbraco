using System;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using OurUmbraco.Our.Businesslogic;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using File = System.IO.File;

namespace OurUmbraco.Our.Api
{
    [Authorize]
    public class CommunityController : UmbracoApiController
    {
        public static string SetAvatar(int memberId, string service)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var member = memberShipHelper.GetById(memberId);

            switch (service)
            {
                case "twitter":
                    if (string.IsNullOrWhiteSpace(member.GetPropertyValue<string>("twitter")) == false)
                    {
                        var twitData = Twitter.Profile(member.GetPropertyValue<string>("twitter"));
                        if (twitData.MoveNext())
                        {
                            var imgUrl = twitData.Current.SelectSingleNode("//profile_image_url").Value;
                            return SaveUrlAsBuddyIcon(imgUrl, member.Id);
                        }
                    }
                    break;
                case "gravatar":
                    var gravatarUrl = string.Format("https://www.gravatar.com/avatar/{0}?s=48&d=monsterid",
                        umbraco.library.md5(member.GetPropertyValue<string>("email")));
                    return SaveUrlAsBuddyIcon(gravatarUrl, member.Id);
            }

            return string.Empty;
        }

        [HttpGet]
        public string SetServiceAsBuddyIcon(string service)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var id = memberShipHelper.GetCurrentMemberId();
            return SetAvatar(id, service);
        }

        private static string SaveUrlAsBuddyIcon(string url, int memberId)
        {
            var file = memberId.ToString(CultureInfo.InvariantCulture);
            var avatar = string.Format("/media/avatar/{0}.jpg", file);
            var path = HttpContext.Current.Server.MapPath(avatar);

            if (File.Exists(path))
                File.Delete(path);

            var webClient = new System.Net.WebClient();
            webClient.DownloadFile(url, path);

            var memberService = ApplicationContext.Current.Services.MemberService;
            var member = memberService.GetById(memberId);

            member.SetValue("avatar", avatar);

            return avatar;
        }

        [HttpGet]
        public string SaveWebCamImage(string memberGuid)
        {
            var url = HttpContext.Current.Request["AvatarUrl"];
            if (string.IsNullOrEmpty(url) == false)
                return "true";

            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var member = memberShipHelper.GetCurrentMember();
            if (member == null)
                return "error";

            var imageBytes = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
            var file = member.Id.ToString(CultureInfo.InvariantCulture);
            var avatar = string.Format("/media/avatar/{0}.jpg", file);
            var path = HttpContext.Current.Server.MapPath(avatar);

            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);

                var newImage = Image.FromStream(ms, true).GetThumbnailImage(64, 48, ThumbnailCallback, new IntPtr());

                if (File.Exists(path))
                    File.Delete(path);

                newImage.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            var memberService = ApplicationContext.Current.Services.MemberService;
            var m = memberService.GetById(member.Id);
            m.SetValue("avatar", avatar);

            return avatar;
        }

        private bool ThumbnailCallback()
        {
            return true;
        }

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
            dynamic result = new ExpandoObject();
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var filename = string.Empty;

                var guid = Guid.NewGuid();

                foreach (string file in httpRequest.Files)
                {

                    var updir = new DirectoryInfo(HttpContext.Current.Server.MapPath("/media/upload/" + guid));

                    if (!updir.Exists)
                        updir.Create();

                    var postedFile = httpRequest.Files[file];

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
