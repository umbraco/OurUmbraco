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
