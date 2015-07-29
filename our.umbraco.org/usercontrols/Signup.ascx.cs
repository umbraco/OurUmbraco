using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using File = System.IO.File;
using UmbracoUserControl = Umbraco.Web.UI.Controls.UmbracoUserControl;

namespace our.usercontrols
{
    public partial class Signup : UmbracoUserControl
    {
        // Needs to be lower case, macro depends on that
        public string memberType { get; set; }

        public string Group { get; set; }
        public int NextPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var memberService = Services.MemberService;
            var currentMember = memberService.GetById(Members.GetCurrentMemberId());

            MemberExists.Visible = false;

            if (Page.IsPostBack == false && currentMember != null)
            {
                tb_name.Text = currentMember.Name;
                tb_email.Text = currentMember.Email;

                //make sure that it is not required to enter the password..
                tb_password.CssClass = "title";

                //hack on the save button...
                bt_submit.Text = "Save";

                //treshold and newsletter
                cb_bugMeNot.Checked = currentMember.GetValue<bool>("bugMeNot");

                tb_treshold.Text = currentMember.GetValue<string>("treshold");

                //optional.. 
                tb_twitter.Text = currentMember.GetValue<string>("twitter");
                tb_flickr.Text = currentMember.GetValue<string>("flickr");
                tb_company.Text = currentMember.GetValue<string>("company");
                tb_bio.Text = currentMember.GetValue<string>("profileText");

                //Location
                tb_lat.Value = currentMember.GetValue<string>("latitude");
                tb_lng.Value = currentMember.GetValue<string>("longitude");
                tb_location.Text = currentMember.GetValue<string>("location");
            }

            // ReCaptcha is wrong when Page.IsValid is false
            Page.Validate();
            if (Page.IsPostBack && Page.IsValid == false)
            {
                tb_password.Attributes["value"] = tb_password.Text;
            }

        }

        protected void CreateMember(object sender, EventArgs e)
        {
            var memberService = Services.MemberService;
            var currentMember = memberService.GetById(Members.GetCurrentMemberId());

            //Member is already logged in, and we just need to save his new data...
            if (currentMember != null)
            {
                currentMember.Name = tb_name.Text;
                currentMember.Email = tb_email.Text;
                currentMember.Username = tb_email.Text;
                currentMember.SetValue("twitter", tb_twitter.Text);
                currentMember.SetValue("flickr", tb_flickr.Text);
                currentMember.SetValue("company", tb_company.Text);
                currentMember.SetValue("profileText", tb_bio.Text);
                currentMember.SetValue("location", tb_location.Text);
                currentMember.SetValue("latitude", tb_lat.Value);
                currentMember.SetValue("longitude", tb_lng.Value);
                currentMember.SetValue("treshold", tb_treshold.Text);
                currentMember.SetValue("bugMeNot", cb_bugMeNot.Checked);

                memberService.Save(currentMember);
                memberService.SavePassword(currentMember, tb_password.Text);

                uForum.Library.Utils.CheckForSpam(currentMember);

                Response.Redirect(library.NiceUrl(NextPage));
            }
            else
            {
                if (tb_email.Text != string.Empty && Page.IsValid)
                {
                    var member = memberService.GetByEmail(tb_email.Text);
                    if (member == null)
                    {
                        // If spammer then this will stop account creation
                        var spamResult = uForum.Library.Utils.CheckForSpam(tb_email.Text, tb_name.Text, true);
                        if (spamResult != null && spamResult.Blocked)
                            return;

                        if (string.IsNullOrWhiteSpace(tb_flickr.Text) == false || string.IsNullOrWhiteSpace(tb_bio.Text) == false)
                        {
                            //These fields are hidden, only a bot will know to fill them in
                            //This honeypot catches them
                            return;
                        }

                        member = memberService.CreateMember(tb_email.Text, tb_email.Text, tb_name.Text, memberType);

                        member.Name = tb_name.Text;
                        member.Email = tb_email.Text;
                        member.Username = tb_email.Text;
                        member.SetValue("twitter", tb_twitter.Text);
                        member.SetValue("company", tb_company.Text);
                        member.SetValue("location", tb_location.Text);
                        member.SetValue("latitude", tb_lat.Value);
                        member.SetValue("longitude", tb_lng.Value);
                        member.SetValue("treshold", tb_treshold.Text);
                        member.SetValue("bugMeNot", cb_bugMeNot.Checked);

                        member.SetValue("reputationTotal", 20);
                        member.SetValue("reputationCurrent", 20);
                        member.SetValue("forumPosts", 0);
                        
                        member.IsApproved = false;
                        memberService.Save(member);

                        // Now that we have a memberId we can use it
                        var avatarPath = GetAvatarPath(member);
                        member.SetValue("avatar", avatarPath);
                        memberService.Save(member);

                        memberService.AssignRole(member.Id, Group);
                        memberService.SavePassword(member, tb_password.Text);

                        if (spamResult != null && spamResult.TotalScore >= int.Parse(ConfigurationManager.AppSettings["PotentialSpammerThreshold"]))
                        {
                            spamResult.MemberId = member.Id;

                            memberService.AssignRole(member.Id, "potentialspam");
                            uForum.Library.Utils.SendPotentialSpamMemberMail(spamResult);
                        }
                        else
                        {
                            uForum.Library.Utils.SendActivationMail(member);
                            uForum.Library.Utils.SendMemberSignupMail(member);
                        }
                        memberService.AssignRole(member.Id, "notactivated");

                        var redirectPage = "/";
                        var contentService = UmbracoContext.Current.Application.Services.ContentService;
                        var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");

                        var memberNode = rootNode.Children().FirstOrDefault(x => x.Name == "Member");
                        if (memberNode != null)
                        {
                            var pendingActivationPage = memberNode.Children().FirstOrDefault(x => x.Name == "Pending activation");
                            if (pendingActivationPage != null)
                                redirectPage = library.NiceUrl(pendingActivationPage.Id);
                        }

                        Response.Redirect(redirectPage);
                    }
                    else
                    {
                        MemberExists.Visible = true;
                        Panel1.Visible = false;
                    }
                }
            }
        }

        private static string GetAvatarPath(IMember member)
        {
            var url = "http://www.gravatar.com/avatar/" + member.Email.ToMd5() + "?s=400&d=retro";

            try
            {
                var avatarFileName = "/media/avatar/" + member.Id + ".jpg";
                var path = HostingEnvironment.MapPath(avatarFileName);

                if (path != null)
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(url, path);
                    }

                    return avatarFileName;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Signup>("Could not save gravatar locally", ex);
            }

            return url;
        }
    }
}