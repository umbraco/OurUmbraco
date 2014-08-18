using System;
using System.Web.UI;
using System.Xml;
using our.Rest;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;

namespace our.usercontrols
{
    public partial class Signup : UserControl
    {   
        // Needs to be lower case, macro depends on that
        public string memberType { get; set; }

        public string Group { get; set; }
        public int NextPage { get; set; }

        private Member _member = Member.GetCurrentMember();

        protected void Page_Load(object sender, EventArgs e)
        {
            //lazyloading the needed javascript for validation. (addded it to the master template as our ahah forms need it aswel)
            //umbraco.library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");

            MemberExists.Visible = false;

            if (Page.IsPostBack == false && _member != null)
            {
                tb_name.Text = _member.Text;
                tb_email.Text = _member.Email;

                //make sure that it is not required to enter the password..
                tb_password.CssClass = "title";

                //hack on the save button...
                bt_submit.Text = "Save";

                //treshold and newsletter

                if (_member.getProperty("bugMeNot") != null)
                {
                    int checkbox;
                    int.TryParse(_member.getProperty("bugMeNot").Value.ToString(), out checkbox);

                    cb_bugMeNot.Checked = (checkbox > 0);
                }

                tb_treshold.Text = _member.getProperty("treshold").Value.ToString();

                //optional.. 
                tb_twitter.Text = _member.getProperty("twitter").Value.ToString();
                tb_flickr.Text = _member.getProperty("flickr").Value.ToString();
                tb_company.Text = _member.getProperty("company").Value.ToString();
                tb_bio.Text = _member.getProperty("profileText").Value.ToString();

                //Location
                tb_lat.Value = _member.getProperty("latitude").Value.ToString();
                tb_lng.Value = _member.getProperty("longitude").Value.ToString();
                tb_location.Text = _member.getProperty("location").Value.ToString();

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
            //Member is already logged in, and we just need to save his new data...
            if (_member != null)
            {
                _member.Text = tb_name.Text;
                _member.Email = tb_email.Text;
                _member.LoginName = tb_email.Text;

                if (tb_password.Text != "")
                    _member.Password = tb_password.Text;

                //optional.. 
                _member.getProperty("twitter").Value = tb_twitter.Text;
                _member.getProperty("flickr").Value = tb_flickr.Text;
                _member.getProperty("company").Value = tb_company.Text;
                _member.getProperty("profileText").Value = tb_bio.Text;

                //location
                _member.getProperty("location").Value = tb_location.Text;
                _member.getProperty("latitude").Value = tb_lat.Value;
                _member.getProperty("longitude").Value = tb_lng.Value;


                //treshold + newsletter
                _member.getProperty("treshold").Value = tb_treshold.Text;
                _member.getProperty("bugMeNot").Value = cb_bugMeNot.Checked;

                _member.XmlGenerate(new XmlDocument());
                _member.Save();
                
                //Refresh the member cache data
                Member.RemoveMemberFromCache(_member);
                Member.AddMemberToCache(_member);

                uForum.Library.Utills.CheckForSpam(_member);

                Response.Redirect(library.NiceUrl(NextPage));

            }
            else
            {
                if (tb_email.Text != string.Empty && Page.IsValid)
                {
                    _member = Member.GetMemberFromLoginName(tb_email.Text);
                    if (_member == null)
                    {
                        // If spammer then this will stop account creation
                        var spamResult = uForum.Library.Utills.CheckForSpam(tb_email.Text, tb_name.Text, true);
                        if(spamResult.Blocked)
                            return;

                        var mt = MemberType.GetByAlias(memberType);

                        // Adding " Temp" is a hack - bizarrely, when you create a member using MakeNew and 
                        // the name does not have a space in it (like: Ben) you'll get a YSOD saying the 
                        // username already exists. However, create it with a space in it and everything is 
                        // fine and dandy! So now we just force the last name to be "Temp" during creation 
                        // and then update the member's name immediately after that... -SJ
                        _member = Member.MakeNew(tb_name.Text + " Temp", mt, new User(0));
                        _member.Text = tb_name.Text;

                        _member.Email = tb_email.Text;
                        _member.Password = tb_password.Text;
                        _member.LoginName = tb_email.Text;

                        //Location
                        _member.getProperty("location").Value = tb_location.Text;
                        _member.getProperty("latitude").Value = tb_lat.Value;
                        _member.getProperty("longitude").Value = tb_lng.Value;

                        //optional.. 
                        _member.getProperty("twitter").Value = tb_twitter.Text;
                        _member.getProperty("flickr").Value = tb_flickr.Text;
                        _member.getProperty("company").Value = tb_company.Text;
                        _member.getProperty("profileText").Value = tb_bio.Text;

                        //treshold + newsletter
                        _member.getProperty("treshold").Value = tb_treshold.Text;
                        _member.getProperty("bugMeNot").Value = cb_bugMeNot.Checked;

                        //Standard values
                        _member.getProperty("reputationTotal").Value = 20;
                        _member.getProperty("reputationCurrent").Value = 20;
                        _member.getProperty("forumPosts").Value = 0;

                        if (string.IsNullOrEmpty(Group) == false)
                        {
                            var memberGroup = MemberGroup.GetByName(Group);
                            if (memberGroup != null)
                                _member.AddGroup(memberGroup.Id);
                        }

                        //set a default avatar
                        BuddyIcon.SetAvatar(_member.Id, "gravatar");

                        _member.Save();
                        _member.XmlGenerate(new XmlDocument());
                        Member.AddMemberToCache(_member);
                        
                        uForum.Library.Utills.CheckForSpam(_member);

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