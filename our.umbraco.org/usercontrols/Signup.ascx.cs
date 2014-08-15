using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using our.Rest;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;

namespace our.usercontrols
{
    public partial class Signup : System.Web.UI.UserControl
    {

        public string Group { get; set; }
        public string memberType { get; set; }
        public int NextPage { get; set; }

        private Member m = umbraco.cms.businesslogic.member.Member.GetCurrentMember();

        protected void Page_Load(object sender, EventArgs e)
        {
            //lazyloading the needed javascript for validation. (addded it to the master template as our ahah forms need it aswel)
            //umbraco.library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");
            
            MemberExists.Visible = false;

            if (!Page.IsPostBack && m != null)
            {
                tb_name.Text = m.Text;
                tb_email.Text = m.Email;

                //make sure that it is not required to enter the password..
                tb_password.CssClass = "title";

                //hack on the save button...
                bt_submit.Text = "Save";

                //treshold and newsletter

                if (m.getProperty("bugMeNot") != null)
                {
                    int c = 0;
                    int.TryParse(m.getProperty("bugMeNot").Value.ToString(), out c);

                    cb_bugMeNot.Checked = (c > 0);
                }

                tb_treshold.Text = m.getProperty("treshold").Value.ToString();

                //optional.. 
                tb_twitter.Text = m.getProperty("twitter").Value.ToString();
                tb_flickr.Text = m.getProperty("flickr").Value.ToString();
                tb_company.Text = m.getProperty("company").Value.ToString();
                tb_bio.Text = m.getProperty("profileText").Value.ToString();

                //Location
                tb_lat.Value = m.getProperty("latitude").Value.ToString();
                tb_lng.Value = m.getProperty("longitude").Value.ToString();
                tb_location.Text = m.getProperty("location").Value.ToString();

            }
            // ReCaptcha is wrong when Page.IsValid is false
            Page.Validate();
            if (Page.IsPostBack && Page.IsValid == false)
            {
                tb_password.Attributes["value"] = tb_password.Text;
            }
            else
            {
                
            }
        }

        internal static void SendPotentialSpamMemberMail(int memberId, bool memberIsNew)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];
                var body = string.Format("Go to member <a href=\"http://our.umbraco.org/member/{0}\">http://our.umbraco.org/member/{0}</a>", memberId);
                body = body + string.Format("<br />Member is new: {0}", memberIsNew);

                var mailMessage = new MailMessage
                {
                    Subject = string.Format("Umbraco community: member flagged as potential spammer"),
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in notify.Split(','))
                    mailMessage.To.Add(email);

                mailMessage.From = new MailAddress("our@umbraco.org");

                var smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, new User(0), -1, "Error sending potential spam member notification: " + ex.Message + " " + ex.StackTrace);
            }
        }

        protected void createMember(object sender, EventArgs e)
        {

            //Member is already logged in, and we just need to save his new data...
            if (m != null)
            {
                m.Text = tb_name.Text;
                m.Email = tb_email.Text;
                m.LoginName = tb_email.Text;

                if (tb_password.Text != "")
                    m.Password = tb_password.Text;

                //optional.. 
                m.getProperty("twitter").Value = tb_twitter.Text;
                m.getProperty("flickr").Value = tb_flickr.Text;
                m.getProperty("company").Value = tb_company.Text;
                m.getProperty("profileText").Value = tb_bio.Text;

                //location
                m.getProperty("location").Value = tb_location.Text;
                m.getProperty("latitude").Value = tb_lat.Value;
                m.getProperty("longitude").Value = tb_lng.Value;


                //treshold + newsletter
                m.getProperty("treshold").Value = tb_treshold.Text;
                m.getProperty("bugMeNot").Value = cb_bugMeNot.Checked;

                m.XmlGenerate(new System.Xml.XmlDocument());
                m.Save();




                //Refresh the member cache data
                Member.RemoveMemberFromCache(m);
                Member.AddMemberToCache(m);

                if (CommentBody.Text != string.Empty)
                {
                    SendPotentialSpamMemberMail(m.Id, false);
                }

                Response.Redirect(umbraco.library.NiceUrl(NextPage));

            }
            else
            {
                if (tb_email.Text != "" && Page.IsValid)
                {
                    m = Member.GetMemberFromLoginName(tb_email.Text);
                    if (m == null)
                    {
                        MemberType mt = MemberType.GetByAlias(memberType);

                        // Adding " Temp" is a hack - bizarrely, when you create a member using MakeNew and 
                        // the name does not have a space in it (like: Ben) you'll get a YSOD saying the 
                        // username already exists. However, create it with a space in it and everything is 
                        // fine and dandy! So now we just force the last name to be "Temp" during creation 
                        // and then update the member's name immediately after that... -SJ
                        m = Member.MakeNew(tb_name.Text + " Temp", mt, new User(0));
                        m.Text = tb_name.Text;

                        m.Email = tb_email.Text;
                        m.Password = tb_password.Text;
                        m.LoginName = tb_email.Text;
                        
                        //Location
                        m.getProperty("location").Value = tb_location.Text;
                        m.getProperty("latitude").Value = tb_lat.Value;
                        m.getProperty("longitude").Value = tb_lng.Value;

                        //optional.. 
                        m.getProperty("twitter").Value = tb_twitter.Text;
                        m.getProperty("flickr").Value = tb_flickr.Text;
                        m.getProperty("company").Value = tb_company.Text;
                        m.getProperty("profileText").Value = tb_bio.Text;

                        //treshold + newsletter
                        m.getProperty("treshold").Value = tb_treshold.Text;
                        m.getProperty("bugMeNot").Value = cb_bugMeNot.Checked;

                        //Standard values
                        m.getProperty("reputationTotal").Value = 20;
                        m.getProperty("reputationCurrent").Value = 20;
                        m.getProperty("forumPosts").Value = 0;



                        if (!string.IsNullOrEmpty(Group))
                        {
                            MemberGroup mg = MemberGroup.GetByName(Group);
                            if (mg != null)
                                m.AddGroup(mg.Id);
                        }

                        //set a default avatar
                        BuddyIcon.SetAvatar(m.Id, "gravatar");

                        m.Save();
                        m.XmlGenerate(new XmlDocument());
                        Member.AddMemberToCache(m);

                        if (CommentBody.Text != string.Empty)
                        {
                            SendPotentialSpamMemberMail(m.Id, true);
                        }

                        Response.Redirect(library.NiceUrl(NextPage));
                    }
                    else
                    {
                        MemberExists.Visible = true;
                        Panel1.Visible = false;
                    }
                }
            }
        }

    }
}