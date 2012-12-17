using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.relation;
using uEvents;
using uEvents.Relations;
using System.Net.Mail;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;

namespace our.usercontrols
{
    public partial class EventMailer : System.Web.UI.UserControl
    {
        public int MaxNumberOfEmails { get; set; }
        private Member m = Member.GetCurrentMember();

        private int EmailsSent(Node n)
        {
            int num = 0;
            foreach (Node node in n.Children)
            {
                if (node.NodeTypeAlias == "EventNews")
                {
                    num++;
                }
            }
            return num;
        }

        protected override void OnInit(EventArgs e)
        {
            ((UmbracoDefault)this.Page).ValidateRequest = false;
            this.bt_submit.Attributes.Add("onclick", "this.value = 'sending...'; this.disabled=true; " + this.Page.ClientScript.GetPostBackEventReference(this.bt_submit, "onclick").ToString());
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");
            Node n = new Node(int.Parse(base.Request.QueryString["id"]));
            if ((n.NodeTypeAlias == "Event") && ((int.Parse(n.GetProperty("owner").Value) == this.m.Id) || (this.m.Id == 0x4ae)))
            {
                this.ph_holder.Visible = true;
                this.MaxNumberOfEmails -= this.EmailsSent(n);
                if (this.MaxNumberOfEmails <= 0)
                {
                    this.bt_submit.Enabled = false;
                }
            }
        }

        private static List<Member> RelationToMember(List<Relation> relations)
        {
            List<Member> list = new List<Member>();
            foreach (Relation relation in relations)
            {
                list.Add(new Member(relation.Child.Id));
            }
            return list;
        }

        protected void SendEmail(object sender, EventArgs e)
        {
            Node n = new Node(int.Parse(base.Request.QueryString["id"]));
            if (((n.NodeTypeAlias == "Event") && ((int.Parse(n.GetProperty("owner").Value) == this.m.Id) || (this.m.Id == 0x4ae))) && (this.MaxNumberOfEmails > this.EmailsSent(n)))
            {
                Event event2 = new Event(n.Id);
                List<Member> list = new List<Member>();
                string selectedValue = this.rbl_receivers.SelectedValue;
                if (selectedValue != null)
                {
                    if (!(selectedValue == "coming"))
                    {
                        if (selectedValue == "waiting")
                        {
                            list.AddRange(RelationToMember(EventRelation.GetPeopleWaiting(n.Id, event2.OnWaitingList)));
                        }
                        else if (selectedValue == "both")
                        {
                            list.AddRange(RelationToMember(EventRelation.GetPeopleSignedUpLast(n.Id, event2.SignedUp)));
                            list.AddRange(RelationToMember(EventRelation.GetPeopleWaiting(n.Id, event2.OnWaitingList)));
                        }
                    }
                    else
                    {
                        list.AddRange(RelationToMember(EventRelation.GetPeopleSignedUpLast(n.Id, event2.SignedUp)));
                    }
                }

                MailMessage message = new MailMessage();
                string str = "<p>\r\n You receive this email because you have signed up \r\n                                            for an event on the umbraco community site http://our.umbraco.org</p>\r\n\r\n                                            <p>You can view the event page \r\n                                            <a href='http://our.umbraco.org" + library.NiceUrl(n.Id) + "'>here</a></p>";
                message.Subject = this.tb_subject.Text;
                message.Body = this.tb_body.Text + str;
                message.From = new MailAddress("robot@umbraco.org", "Our Umbraco - Events");
                message.ReplyToList.Add(new MailAddress(this.m.Email));
                message.IsBodyHtml = true;
                foreach (Member member in list)
                {
                    try
                    {
                        message.Bcc.Add(new MailAddress(member.Email, member.Text));
                    }
                    catch
                    {
                    }
                }
                new SmtpClient().Send(message);
                Document document = Document.MakeNew(this.tb_subject.Text, DocumentType.GetByAlias("EventNews"), new User(0), n.Id);
                document.getProperty("bodyText").Value = this.tb_body.Text;
                document.getProperty("subject").Value = this.tb_subject.Text;
                document.getProperty("sender").Value = this.m.Id;
                document.Publish(new User(0));
                library.UpdateDocumentCache(document.Id);
                document.Save();
                base.Response.Redirect(n.NiceUrl);
            }
        }

    }
}