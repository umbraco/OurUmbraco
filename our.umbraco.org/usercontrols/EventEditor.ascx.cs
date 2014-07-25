using System;
using System.Configuration;
using System.Linq;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;
using System.Net.Mail;
using umbraco.BusinessLogic;

namespace our.usercontrols
{
    public partial class EventEditor : System.Web.UI.UserControl
    {
        private readonly Member _member = Member.GetCurrentMember();
        public int EventsRoot { get; set; }

        protected override void OnInit(EventArgs e)
        {
            ((umbraco.UmbracoDefault)this.Page).ValidateRequest = false;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");

            //edit?
            if (Page.IsPostBack == false && string.IsNullOrEmpty(Request.QueryString["id"]) == false)
            {
                var node = new Node(int.Parse(Request.QueryString["id"]));
                //allowed?
                if (node.NodeTypeAlias == "Event" && int.Parse(node.GetProperty("owner").Value) == _member.Id)
                {
                    tb_name.Text = node.Name;
                    tb_desc.Text = node.GetProperty("description").Value;

                    tb_venue.Text = node.GetProperty("venue").Value;
                    tb_capacity.Text = node.GetProperty("capacity").Value;

                    tb_lat.Value = node.GetProperty("latitude").Value;
                    tb_lng.Value = node.GetProperty("longitude").Value;

                    dp_startdate.Text = DateTime.Parse(node.GetProperty("start").Value).ToString("MM/dd/yyyy H:mm");
                    dp_enddate.Text = DateTime.Parse(node.GetProperty("end").Value).ToString("MM/dd/yyyy H:mm");
                }
            }
        }

        protected void createEvent(object sender, EventArgs e)
        {
            var hasAnchors = false;
            var hasLowKarma = false;
            var documentId = 0;

            var karma = int.Parse(_member.getProperty("reputationTotal").Value.ToString());
            if (karma < 50)
            {
                hasLowKarma = true;
            }

            //edit?
            if (string.IsNullOrEmpty(Request.QueryString["id"]) == false)
            {
                var document = new Document(int.Parse(Request.QueryString["id"]));
                documentId = document.Id;

                //allowed?
                if (document.ContentType.Alias == "Event" && int.Parse(document.getProperty("owner").Value.ToString()) == _member.Id)
                {
                    SetDescription(document, hasLowKarma, ref hasAnchors);

                    document.Text = tb_name.Text;

                    document.getProperty("venue").Value = tb_venue.Text;
                    document.getProperty("latitude").Value = tb_lat.Value;
                    document.getProperty("longitude").Value = tb_lng.Value;

                    var sync = tb_capacity.Text != document.getProperty("capacity").Value.ToString();

                    document.getProperty("capacity").Value = tb_capacity.Text;

                    document.getProperty("start").Value = DateTime.Parse(dp_startdate.Text);
                    document.getProperty("end").Value = DateTime.Parse(dp_enddate.Text);

                    document.Save();
                    document.Publish(new User(0));

                    umbraco.library.UpdateDocumentCache(document.Id);

                    if (sync)
                    {
                        var ev = new uEvents.Event(document);
                        ev.syncCapacity();
                    }
                }
            }
            else
            {
                Document document = Document.MakeNew(tb_name.Text, DocumentType.GetByAlias("Event"), new User(0), EventsRoot);
                documentId = document.Id;

                SetDescription(document, hasLowKarma, ref hasAnchors);

                document.getProperty("venue").Value = tb_venue.Text;
                document.getProperty("latitude").Value = tb_lat.Value;
                document.getProperty("longitude").Value = tb_lng.Value;

                document.getProperty("capacity").Value = tb_capacity.Text;
                document.getProperty("signedup").Value = 0;

                document.getProperty("start").Value = DateTime.Parse(dp_startdate.Text);
                document.getProperty("end").Value = DateTime.Parse(dp_enddate.Text);

                document.getProperty("owner").Value = _member.Id;

                document.Save();
                document.Publish(new User(0));
                umbraco.library.UpdateDocumentCache(document.Id);
            }

            var redirectUrl = umbraco.library.NiceUrl(documentId);

            if (hasLowKarma && hasAnchors)
                SendPotentialSpamNotification(tb_name.Text, redirectUrl, _member.Id);

            Response.Redirect(redirectUrl);
        }

        private void SetDescription(Content content, bool hasLowKarma, ref bool hasAnchors)
        {
            content.getProperty("description").Value = tb_desc.Text;

            // Filter out links when karma is low, probably a spammer
            if (hasLowKarma)
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(tb_desc.Text);

                var anchorNodes = doc.DocumentNode.SelectNodes("//a");
                if (anchorNodes != null)
                {
                    hasAnchors = true;

                    foreach (var anchor in anchorNodes)
                        anchor.ParentNode.RemoveChild(anchor, true);
                }

                content.getProperty("description").Value = doc.DocumentNode.OuterHtml;
            }
        }

        private static void SendPotentialSpamNotification(string eventName, string eventUrl, int memberId)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];

                var post = string.Format("Event: {0} - link: <a href=\"http://{1}\">http://{1}</a><br />By member:<a href=\"http://our.umbraco.org/member/{2}\">http://our.umbraco.org/member/{2}</a>", eventName, eventUrl, memberId);

                var body = string.Format("<p>The following event could be spam as it was posted by someone with low karma and it includes anchor tags (which have been stripped)</p><hr />{0}", post);

                var mailMessage = new MailMessage
                {
                    Subject = "Umbraco community: Event is possible spam",
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
                Log.Add(LogTypes.Error, new User(0), -1, "Error sending spam notification: " + ex.Message + " " + ex.StackTrace);
            }
        }
    }
}