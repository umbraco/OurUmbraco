using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;

namespace our.usercontrols {
    public partial class Signup : System.Web.UI.UserControl {

        public string Group { get; set; }
        public string memberType { get; set; }
        public int NextPage { get; set; }

        private Member m = umbraco.cms.businesslogic.member.Member.GetCurrentMember();

        protected void Page_Load(object sender, EventArgs e) {
            //lazyloading the needed javascript for validation. (addded it to the master template as our ahah forms need it aswel)
            //umbraco.library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");

            if (!Page.IsPostBack && m != null) {
                tb_name.Text = m.Text;
                tb_email.Text = m.Email;

                //make sure that it is not required to enter the password..
                tb_password.CssClass = "title";

                //hack on the save button...
                bt_submit.Text = "Save";

                //treshold and newsletter

                if (m.getProperty("bugMeNot") != null) {
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

        }

        protected void createMember(object sender, EventArgs e) {

            //Member is already logged in, and we just need to save his new data...
            if (m != null) {
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

                Response.Redirect(umbraco.library.NiceUrl(NextPage));

            } else {
                if (tb_email.Text != "") {
                    m = Member.GetMemberFromEmail(tb_email.Text);
                    if (m == null) {
                        MemberType mt = MemberType.GetByAlias(memberType);
                        m = Member.MakeNew(tb_name.Text, mt, new umbraco.BusinessLogic.User(0));
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
                        


                        if (!string.IsNullOrEmpty(Group)) {
                            MemberGroup mg = MemberGroup.GetByName(Group);
                            if (mg != null)
                                m.AddGroup(mg.Id);
                        }

                        //set a default avatar
                        Rest.BuddyIcon.SetAvatar(m.Id, "gravatar");

                        m.Save();
                        m.XmlGenerate(new System.Xml.XmlDocument());
                        Member.AddMemberToCache(m);

                        Response.Redirect(umbraco.library.NiceUrl(NextPage));
                    }
                }
            }
        }

    }
}