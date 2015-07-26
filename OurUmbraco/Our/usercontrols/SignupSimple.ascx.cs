using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;

namespace our.usercontrols
{
    public partial class SignupSimple : System.Web.UI.UserControl
    {
        public string Group { get; set; }
        public string memberType { get; set; }
        public int NextPage { get; set; }

        private Member m = umbraco.cms.businesslogic.member.Member.GetCurrentMember();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && m != null)
                Response.Redirect(umbraco.library.NiceUrl(NextPage));
        }

        protected void createMember(object sender, EventArgs e)
        {
                if (tb_email.Text != "")
                {
                    m = Member.GetMemberFromEmail(tb_email.Text);
                    if (m == null)
                    {
                        MemberType mt = MemberType.GetByAlias(memberType);
                        m = Member.MakeNew(tb_name.Text, mt, new umbraco.BusinessLogic.User(0));
                        m.Email = tb_email.Text;
                        m.Password = tb_password.Text;
                        m.LoginName = tb_email.Text;


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
                        Api.CommunityController.SetAvatar(m.Id, "gravatar");
                            
                        m.Save();
                        m.XmlGenerate(new System.Xml.XmlDocument());
                        Member.AddMemberToCache(m);
                        Response.Redirect(umbraco.library.NiceUrl(NextPage));
                    }
                }
            }
        }
    }
