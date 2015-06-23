using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Xml;
using our.Businesslogic;
using our.Rest;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using Umbraco.Web.WebApi;
using System.Net.Http;
using System.Dynamic;
using Newtonsoft.Json;

namespace our.Api
{
    public class CommunityController : UmbracoApiController
    {
        [HttpGet]
        public string IsEmailUnique(string email)
        {
            //if user is already logged in and tries to re-enter his own email...
            Member mem = Member.GetCurrentMember();
            if (mem != null && mem.Email == email)
                return "true";

            return (Member.GetMemberFromEmail(email) == null).ToString().ToLower();
        }

        public static string SetAvatar(int mId, string service)
        {
            var member = new Member(mId);

            switch (service)
            {
                case "twitter":
                    if (member.getProperty("twitter") != null && member.getProperty("twitter").Value.ToString() != "")
                    {
                        var twitData = Twitter.Profile(member.getProperty("twitter").Value.ToString());
                        if (twitData.MoveNext())
                        {
                            var imgUrl = twitData.Current.SelectSingleNode("//profile_image_url").Value;
                            return SaveUrlAsBuddyIcon(imgUrl, member);
                        }
                    }
                    break;
                case "gravatar":
                    var gravatarUrl = "http://www.gravatar.com/avatar/" + umbraco.library.md5(member.Email) + "?s=48&d=monsterid";
                    return SaveUrlAsBuddyIcon(gravatarUrl, member);
            }

            return string.Empty;
        }

        [HttpGet]
        public string SetServiceAsBuddyIcon(string service)
        {
            var id = Member.GetCurrentMember().Id;
            return SetAvatar(id, service);
        }

        private static string SaveUrlAsBuddyIcon(string url, Member m)
        {
            var file = m.Id.ToString(CultureInfo.InvariantCulture);
            var path = HttpContext.Current.Server.MapPath("/media/avatar/" + file + ".jpg");

            if (File.Exists(path))
                File.Delete(path);

            var webClient = new System.Net.WebClient();
            webClient.DownloadFile(url, path);

            m.getProperty("avatar").Value = "/media/avatar/" + file + ".jpg";
            m.XmlGenerate(new XmlDocument());
            m.Save();

            Member.RemoveMemberFromCache(m);
            Member.AddMemberToCache(m);

            return "/media/avatar/" + file + ".jpg";
        }

        [HttpGet]
        public string SaveWebCamImage(string memberGuid)
        {
            var url = HttpContext.Current.Request["AvatarUrl"];
            if (string.IsNullOrEmpty(url) == false)
                return "true";

            Member m = Member.GetCurrentMember();
            if (m != null)
            {
                var imageBytes = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                var file = m.Id.ToString(CultureInfo.InvariantCulture);
                var path = HttpContext.Current.Server.MapPath("/media/avatar/" + file + ".jpg");

                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    ms.Write(imageBytes, 0, imageBytes.Length);

                    var newImage = Image.FromStream(ms, true).GetThumbnailImage(64, 48, ThumbnailCallback, new IntPtr());

                    if (File.Exists(path))
                        File.Delete(path);

                    newImage.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                m.getProperty("avatar").Value = string.Format("/media/avatar/{0}.jpg", file);
                m.XmlGenerate(new XmlDocument());
                m.Save();

                Member.RemoveMemberFromCache(m);
                Member.AddMemberToCache(m);

                return string.Format("/media/avatar/{0}.jpg", file);
            }

            return "error";
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
            Application.SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND ([group] = @group));",
                Application.SqlHelper.CreateParameter("@nodeId", nodeId),
                Application.SqlHelper.CreateParameter("@group", group));

            //and now we add them again...
            foreach (string tag in tags.Split(','))
            {
                string cleanedtag = tag.Replace("<", "");
                cleanedtag = cleanedtag.Replace("'", "");
                cleanedtag = cleanedtag.Replace("\"", "");
                cleanedtag = cleanedtag.Replace(">", "");

                if (cleanedtag.Length > 0)
                {
                    try
                    {
                        tagId = umbraco.editorControls.tags.library.AddTag(cleanedtag, group);


                        if (tagId > 0)
                        {

                            Application.SqlHelper.ExecuteNonQuery("INSERT INTO cmsTagRelationShip(nodeId,tagId) VALUES (@nodeId, @tagId)",
                                Application.SqlHelper.CreateParameter("@nodeId", nodeId),
                                 Application.SqlHelper.CreateParameter("@tagId", tagId)
                            );

                            tagId = 0;

                        }
                    }
                    catch { }
                }

            }
        }

        [HttpGet]
        public string ChangeCollabStatus(int projectId, bool status)
        {
            int _currentMember = Member.GetCurrentMember().Id;
            if (_currentMember > 0)
            {
                Document p = new Document(projectId);

                if ((int)p.getProperty("owner").Value == _currentMember)
                {
                    p.getProperty("openForCollab").Value = status;

                    p.Publish(new User(0));
                    umbraco.library.UpdateDocumentCache(p.Id);

                    return "true";
                }
                
                return "false";
            }

            return "false";
        }

        [HttpGet]
        public string RemoveContributor(int projectId, int memberId)
        {
            int _currentMember = Member.GetCurrentMember().Id;

            if (_currentMember > 0)
            {
                umbraco.presentation.nodeFactory.Node p = new umbraco.presentation.nodeFactory.Node(projectId);

                if (p.GetProperty("owner").Value == _currentMember.ToString())
                {

                    ProjectContributor pc = new ProjectContributor(projectId, memberId);
                    pc.Delete();
                    return "true";
                }
                
                return "false";
            }

            return "false";
        }


        // Avatar upload
        [HttpPost]
        public HttpResponseMessage ImageUpload()
        {
            dynamic result = new ExpandoObject();
            var httpRequest = System.Web.HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                string filename = string.Empty;

                Guid g = Guid.NewGuid();

                foreach (string file in httpRequest.Files)
                {

                    DirectoryInfo updir = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/media/upload/" + g));

                    if (!updir.Exists)
                        updir.Create();

                    var postedFile = httpRequest.Files[file];

                    var filePath = updir.FullName + "/" + postedFile.FileName;
                    postedFile.SaveAs(filePath);
                    filename = postedFile.FileName;

                }

                result.success = true;
                result.imagePath = "/media/upload/" + g + "/" + filename;
            }
            else
            {
                result.success = false;
                result.message = "No images found";
            }

            //jquery ajax file uploader expects html, it parses to json client side
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(result));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            return response;
        }
    }

   

}
