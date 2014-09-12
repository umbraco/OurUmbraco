using System;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using umbraco.cms.businesslogic.member;
using System.IO;
using System.Drawing;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using our.Businesslogic;
using System.Web.Security;
using Umbraco.Web.BaseRest;

namespace our.Rest
{
    /// <summary>
    /// Our mighty utill classes for looking stuff up quickly...
    /// </summary>
    /// 

    public class Stats
    {
        public static void UpdateProjectDownloadCount(int fileId)
        {
            //uWiki.Businesslogic.WikiFile.UpdateDownloadCount(fileId);
        }
    }


    public class Tagger
    {
        public static void AddTag(int nodeId, string group)
        {
            string tag = HttpContext.Current.Request["tag"];

            string cleanedtag = tag.Replace("<", "");
            cleanedtag = cleanedtag.Replace("'", "");
            cleanedtag = cleanedtag.Replace("\"", "");
            cleanedtag = cleanedtag.Replace(">", "");

            umbraco.editorControls.tags.library.addTagsToNode(nodeId, tag, group);
        }

        public static void RemoveTag(int nodeId, string group)
        {
            string tag = HttpContext.Current.Request["tag"];

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


    }


    public class Twitter
    {

        public static XPathNodeIterator Search(string searchString)
        {
            string twitterUrl = "http://search.twitter.com/search.atom?q=" + searchString;
            return umbraco.library.GetXmlDocumentByUrl(twitterUrl, 2000);
        }

        public static XPathNodeIterator Profile(string alias)
        {
            string twitterUrl = "http://twitter.com/users/show/" + alias + ".xml";
            return umbraco.library.GetXmlDocumentByUrl(twitterUrl, 0);
        }
    }

    public class Validation
    {

        public static string IsEmailUnique(string email)
        {
            string e = email;

            //if user is already logged in and tries to re-enter his own email...
            umbraco.cms.businesslogic.member.Member mem = Member.GetCurrentMember();
            if (mem != null && mem.Email == email)
                return "true";
            else
                return (umbraco.cms.businesslogic.member.Member.GetMemberFromEmail(e) == null).ToString().ToLower();
        }

    }

    public class BuddyIcon
    {

        public static string SetAvatar(int mId, string service)
        {

            string retval = "";
            Member m = new Member(mId);

            if (m != null)
            {
                switch (service)
                {

                    case "twitter":
                        if (m.getProperty("twitter") != null && m.getProperty("twitter").Value.ToString() != "")
                        {
                            XPathNodeIterator twitData = Twitter.Profile(m.getProperty("twitter").Value.ToString());
                            if (twitData.MoveNext())
                            {
                                string imgUrl = twitData.Current.SelectSingleNode("//profile_image_url").Value;
                                return saveUrlAsBuddyIcon(imgUrl, m);
                            }
                        }
                        break;
                    case "gravatar":
                        string gUrl = "http://www.gravatar.com/avatar/" + umbraco.library.md5(m.Email) + "?s=48&d=monsterid";
                        return saveUrlAsBuddyIcon(gUrl, m);
                    default:
                        break;
                }
            }

            return retval;

        }

        public static string SetServiceAsBuddyIcon(string service)
        {

            int id = Member.GetCurrentMember().Id;

            return SetAvatar(id, service);
        }

        private static string saveUrlAsBuddyIcon(string url, Member m)
        {
            string _file = m.Id.ToString();
            string _path = HttpContext.Current.Server.MapPath("/media/avatar/" + _file + ".jpg");
            string _currentFile = m.getProperty("avatar").Value.ToString();

            if (System.IO.File.Exists(_path))
                System.IO.File.Delete(_path);

            System.Net.WebClient wc = new System.Net.WebClient();
            wc.DownloadFile(url, _path);

            m.getProperty("avatar").Value = "/media/avatar/" + _file + ".jpg";
            m.XmlGenerate(new XmlDocument());
            m.Save();

            Member.RemoveMemberFromCache(m);
            Member.AddMemberToCache(m);


            return "/media/avatar/" + _file + ".jpg";
        }


        private static void deleteFile(string vPath)
        {
            try
            {
                string _currentFile = HttpContext.Current.Server.MapPath(vPath);
                if (System.IO.File.Exists(_currentFile))
                    System.IO.File.Delete(_currentFile);
            }
            catch { }
        }

        public static string SaveWebCamImage(string memberGuid)
        {
            string url = HttpContext.Current.Request["AvatarUrl"];
            if (!string.IsNullOrEmpty(url))
            {
                return "true";
            }
            else
            {
                Member m = Member.GetCurrentMember();
                if (m != null)
                {
                    byte[] imageBytes = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                    string _file = m.Id.ToString();
                    string _path = HttpContext.Current.Server.MapPath("/media/avatar/" + _file + ".jpg");
                    string _currentFile = m.getProperty("avatar").Value.ToString();



                    System.Drawing.Image newImage;
                    using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                    {

                        ms.Write(imageBytes, 0, imageBytes.Length);

                        newImage = Image.FromStream(ms, true).GetThumbnailImage(64, 48, new Image.GetThumbnailImageAbort(ThumbnailCallback), new IntPtr());

                        if (System.IO.File.Exists(_path))
                            System.IO.File.Delete(_path);

                        newImage.Save(_path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                    m.getProperty("avatar").Value = "/media/avatar/" + _file + ".jpg";
                    m.XmlGenerate(new System.Xml.XmlDocument());
                    m.Save();

                    Member.RemoveMemberFromCache(m);
                    Member.AddMemberToCache(m);


                    return "/media/avatar/" + _file + ".jpg";

                }
                else
                {
                    return "error";
                }
            }
        }

        private static bool ThumbnailCallback()
        {
            return true;
        }



    }

    public class Projects
    {
        public static string ChangeCollabStatus(int projectId, bool status)
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
                else
                {
                    return "false";
                }
            }

            return "false";
        }

        public static string RemoveContributor(int projectId, int memberId)
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
                else
                {
                    return "false";
                }

            }

            return "false";
        }
    }

    [RestExtension("Community")]
    public class Community
    {
        /// <summary>
        /// WB Added: A way for members of the 'Admin' group to block a spammy member
        /// </summary>
        [RestExtensionMethod(AllowGroup = "admin", ReturnXml = false)]
        public static string BlockMember(int memberId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            //Check if member is an admin (in group 'admin')
            if (Utills.IsAdmin(_currentMember))
            {
                //Lets check the memberID of the member we are blocking passed into /base is a valid member..
                if (Utills.IsMember(memberId))
                {
                    //Yep - it's valid, lets get that member
                    Member MemberToBlock = Utills.GetMember(memberId);

                    //Now we have the member - lets update the 'blocked' property on the member
                    MemberToBlock.getProperty("blocked").Value = true;

                    //Save the changes
                    MemberToBlock.Save();

                    //It's all good...
                    return "true";
                }
            }

            //Member not authorised or memberID passed in is not valid
            return "false";
        }

        /// <summary>
        /// WB Added: A way for members of the 'Admin' group to un-block a spammy member
        /// </summary>
        [RestExtensionMethod(AllowGroup = "admin", ReturnXml = false)]
        public static string UnBlockMember(int memberId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            //Check if member is an admin (in group 'admin')
            if (Utills.IsAdmin(_currentMember))
            {
                //Lets check the memberID of the member we are blocking passed into /base is a valid member..
                if (Utills.IsMember(memberId))
                {
                    //Yep - it's valid, lets get that member
                    Member MemberToBlock = Utills.GetMember(memberId);

                    //Now we have the member - lets update the 'blocked' property on the member
                    MemberToBlock.getProperty("blocked").Value = false;

                    //Save the changes
                    MemberToBlock.Save();

                    //It's all good...
                    return "true";
                }
            }

            //Member not authorised or memberID passed in is not valid
            return "false";
        }

        [RestExtensionMethod(AllowGroup = "HQ", ReturnXml = false)]
        public static string DeleteMember(int memberId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            //Check if member is an admin (in group 'admin')
            if (Utills.IsHq(_currentMember))
            {
                //Lets check the memberID of the member we are blocking passed into /base is a valid member..
                if (Utills.IsMember(memberId))
                {
                    //Yep - it's valid, lets get that member
                    var member = Utills.GetMember(memberId);

                    Membership.DeleteUser(member.LoginName, true);

                    Application.SqlHelper.ExecuteNonQuery("UPDATE forumForums SET latestAuthor = 0 WHERE latestAuthor = @memberId",
                        Application.SqlHelper.CreateParameter("@memberId", memberId));

                    //It's all good...
                    return "true";
                }
            }

            //Member not authorised or memberID passed in is not valid
            return "false";
        }

        [RestExtensionMethod(AllowGroup = "HQ", ReturnXml = false)]
        public static string GetBlockedMembers()
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            //Check if member is an admin (in group 'admin')
            if (Utills.IsHq(_currentMember))
            {
                var returnValue = string.Empty;

                const string blockedMembersQuery = "SELECT contentNodeId FROM cmsPropertyData WHERE propertytypeid = (SELECT id FROM cmsPropertyType WHERE alias = 'blocked') AND dataInt = 1";

                var reader = Data.SqlHelper.ExecuteReader(blockedMembersQuery);

                while (reader.Read())
                {
                    var memberId = reader.GetInt("contentNodeId");
                    returnValue = returnValue + "<a href=\"/member/" + memberId + "\">" + memberId + "</a><br />";
                }
                
                //It's all good...
                return returnValue;
            }

            //Member not authorised or memberID passed in is not valid
            return "";
        }

    }
}
