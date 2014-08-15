using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using umbraco.BusinessLogic;


namespace our.usercontrols
{
    public partial class HeaderLogin : System.Web.UI.UserControl
    {
        public int ProfilePage { get; set; }
        public int LoginPage { get; set; }
        public int CreatePage { get; set; }
        public int BlockedPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            //kill member cookies with an invalid id-guid combo
            KillInvalidMemberCookies();

            if (Member.IsLoggedOn())
                uForum.Library.Utills.CheckForSpam(Member.GetCurrentMember());

            toggleControls(Member.IsLoggedOn());

            hl_login.NavigateUrl = umbraco.library.NiceUrl(LoginPage) + "?redirectUrl=" + Server.UrlEncode(Request.Url.ToString());
            hl_profile.NavigateUrl = umbraco.library.NiceUrl(ProfilePage);
            hl_create.NavigateUrl = umbraco.library.NiceUrl(CreatePage);

            RegisterIP();
        }

        private void KillInvalidMemberCookies()
        {
            //WB updated as in 4.7 RC the member cookie merged into one cookie now
            /*
            string UmbracoMemberIdCookieKey = "umbracoMemberId";
            string UmbracoMemberGuidCookieKey = "umbracoMemberGuid";
            string UmbracoMemberLoginCookieKey = "umbracoMemberLogin";
            */

            string umbracoMemberCookieKey           = "UMB_MEMBER";
            string umbracoMemberCookieValue         = StateHelper.GetCookieValue(umbracoMemberCookieKey);
          

            if (HasCookieValue(umbracoMemberCookieKey))
            {
                //WB split the cookie values with 4.7 RC cookie change
                string[] umbracoMemberCookieValueSplit  = umbracoMemberCookieValue.Split(new Char[] { '+' });

                string UmbracoMemberIdCookieValue       = umbracoMemberCookieValueSplit[0].ToString();
                string UmbracoMemberGuidCookieValue     = umbracoMemberCookieValueSplit[1].ToString();
                string UmbracoMemberLoginCookieValue    = umbracoMemberCookieValueSplit[2].ToString();


                int currentMemberId = 0;
                string currentGuid = "";

                /*
                int.TryParse(StateHelper.GetCookieValue(UmbracoMemberIdCookieKey), out currentMemberId);
                currentGuid = StateHelper.GetCookieValue(UmbracoMemberGuidCookieKey);
                */

                int.TryParse(UmbracoMemberIdCookieValue, out currentMemberId);
                currentGuid = UmbracoMemberGuidCookieValue;

                

                if (currentMemberId > 0 && !memberValid(currentMemberId, currentGuid))
                {
                    /*
                    //not valid
                    KillCookie(UmbracoMemberGuidCookieKey,"umbraco.com");
                    KillCookie(UmbracoMemberLoginCookieKey, "umbraco.com");
                    KillCookie(UmbracoMemberIdCookieKey,"umbraco.com");
                    */

                    //WB updated as in 4.7 RC the member cookie merged into one cookie now
                    KillCookie(umbracoMemberCookieKey, "umbraco.com");

                    KillCookie(FormsAuthentication.FormsCookieName, "umbraco.com");

                    FormsAuthentication.SignOut();

                    Response.Redirect("/", true);
                }
            }
        }


        public bool HasCookieValue(string name)
        {
            if(Request.Cookies[name] == null)
            {
                return false;
            }

            if(string.IsNullOrEmpty(Request.Cookies[name].Value))
            {
                return false;
            }

            return true;
        } 



        private void KillCookie(string name, string domain)
        {
            HttpCookie cookie = new HttpCookie(name, "0");
            cookie.Domain = domain;
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
        }

        private static bool memberValid(int id, string uniqueID)
        {
            if (HttpContext.Current.Session["validmember"] == null)
            {
                HttpContext.Current.Session["validmember"] = umbraco.BusinessLogic.Application.SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where id = @id and uniqueID = @uniqueID",
                    umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@id", id),
                    umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@uniqueID", uniqueID.ToUpper())) == 1;
            }

            return (bool)HttpContext.Current.Session["validmember"];

        }

        protected void logout_click(object sender, EventArgs e)
        {

            if (umbraco.library.IsLoggedOn())
            {
                umbraco.cms.businesslogic.member.Member mem = umbraco.cms.businesslogic.member.Member.GetCurrentMember();
                umbraco.cms.businesslogic.member.Member.RemoveMemberFromCache(mem);
                umbraco.cms.businesslogic.member.Member.ClearMemberFromClient(mem);

                uForum.Businesslogic.ForumEditor.ClearEditorChoiceCookie();

                Response.Redirect(umbraco.presentation.nodeFactory.Node.GetCurrent().Url);
            }
        }

        private void RegisterIP()
        {
            if (umbraco.library.IsLoggedOn())
            {
                string ip = HttpContext.Current.Request.UserHostAddress;
                umbraco.cms.businesslogic.member.Member mem = umbraco.cms.businesslogic.member.Member.GetCurrentMember();

                if (mem != null)
                {

                    if (mem.getProperty("ip") != null && mem.getProperty("ip").Value.ToString() != ip)
                    {
                        mem.getProperty("ip").Value = ip;
                        mem.Save();
                    }

                    if (mem.getProperty("blocked") != null && mem.getProperty("blocked").Value.ToString() == "1")
                    {
                        umbraco.cms.businesslogic.member.Member.RemoveMemberFromCache(mem);
                        umbraco.cms.businesslogic.member.Member.ClearMemberFromClient(mem);
                        Response.Redirect(umbraco.library.NiceUrl(BlockedPage));
                    }


                    // if terms of service is not accepted and we're not on the Terms of Service page, redirect
                    if (!Request.Url.PathAndQuery.ToLower().StartsWith("/termsofservice") &&  mem.getProperty("tos") != null && mem.getProperty("tos").Value.ToString() == "")
                    {
                        Response.Redirect("/termsofservice");
                    }
                }
            }
        }

        public static bool IsMember(int id)
        {
            return (uForum.Businesslogic.Data.SqlHelper.ExecuteScalar<int>("select count(nodeid) from cmsMember where nodeid = '" + id + "'") > 0);
        }


        private void toggleControls(bool state)
        {

            bool _state = state;
            Member m = umbraco.cms.businesslogic.member.Member.GetCurrentMember();
            HttpCookie c = Request.Cookies["umbracoMemberId"];
            int cMid = 0;

            if (c != null && int.TryParse(c.Value, out cMid))
            {
                if (cMid > 0 && !IsMember(cMid))
                {
                    foreach (string name in Request.Cookies.AllKeys)
                    {
                        Request.Cookies[name].Expires = DateTime.Now;
                    }
                }
            }


            if (m == null)
            {
                _state = false;
                /*
                foreach (HttpCookie hc in Request.Cookies.) {
                    hc.Expires = DateTime.Now;
                }
                */

                //FormsAuthentication.SignOut();

            }
            else
            {
                //WB 17/4/11 - When I deployed latest DLL for logging events this line caused a YSOD
                //I dont think this cookie no longer exists, as in 4.7 the cookie member items seperated into different cookies
                //Page.Trace.Write(Request.Cookies["UMB_MEMBER"].Value);

                //WB 17/4/11 - replace with 3 indivudal calls
                //Page.Trace.Write(Request.Cookies["umbracoMemberId"].Value);
                //Page.Trace.Write(Request.Cookies["umbracoMemberGuid"].Value);
                //Page.Trace.Write(Request.Cookies["umbracoMemberLogin"].Value);
            }

            lb_logout.Visible = _state;
            hl_login.Visible = !_state;
            lt_LoggedInmsg.Visible = _state;
            lt_notLoggedInmsg.Visible = !_state;
            hl_profile.Visible = _state;
            hl_create.Visible = !_state;

            if (_state)
            {
                lt_LoggedInmsg.Text = lt_LoggedInmsg.Text.Replace("%name%", m.Text);
            }
        }
    }
}