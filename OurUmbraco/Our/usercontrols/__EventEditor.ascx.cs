using System;
using System.Configuration;
using System.Net.Mail;
using System.Web.UI;
using OurUmbraco.Events.Relations;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Member = umbraco.cms.businesslogic.member.Member;
using UserControl = System.Web.UI.UserControl;

namespace OurUmbraco.Our.usercontrols
{
    public partial class EventEditor : UserControl
    {
        private readonly Member _member = Member.GetCurrentMember();
        public int EventsRoot { get; set; }

        protected override void OnInit(EventArgs e)
        {
            ((umbraco.UmbracoDefault)Page).ValidateRequest = false;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");

            //edit?
            if (Page.IsPostBack == false && string.IsNullOrEmpty(Request.QueryString["id"]) == false)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var content = umbracoHelper.TypedContent(int.Parse(Request.QueryString["id"]));

                //allowed?
                if (content.DocumentTypeAlias == "Event" && content.GetPropertyValue<int>("owner") == _member.Id)
                {
                    tb_name.Text = content.Name;
                    tb_desc.Text = content.GetPropertyValue<string>("description");

                    tb_venue.Text = content.GetPropertyValue<string>("venue");
                    tb_capacity.Text = content.GetPropertyValue<string>("capacity");

                    tb_lat.Value = content.GetPropertyValue<string>("latitude");
                    tb_lng.Value = content.GetPropertyValue<string>("longitude");

                    dp_startdate.Text = DateTime.Parse(content.GetPropertyValue<string>("start")).ToString("MM/dd/yyyy H:mm");
                    dp_enddate.Text = DateTime.Parse(content.GetPropertyValue<string>("start")).ToString("MM/dd/yyyy H:mm");
                }
            }
        }

        protected void createEvent(object sender, EventArgs e)
        {
            var hasAnchors = false;
            var hasLowKarma = false;

            var karma = int.Parse(_member.getProperty("reputationTotal").Value.ToString());
            if (karma < 50)
            {
                hasLowKarma = true;
            }

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var contentService = ApplicationContext.Current.Services.ContentService;

            IContent content;

            //edit?
            if (string.IsNullOrEmpty(Request.QueryString["id"]) == false)
            {
                content = contentService.GetById(int.Parse(Request.QueryString["id"]));
                var publishedContent = umbracoHelper.TypedContent(Request.QueryString["id"]);

                //allowed?
                if (publishedContent.DocumentTypeAlias == "Event" && publishedContent.GetPropertyValue<int>("owner") == _member.Id)
                {
                    content = SetDescription(content, hasLowKarma, ref hasAnchors);

                    content.Name = tb_name.Text;

                    content.SetValue("venue", tb_venue.Text);
                    content.SetValue("latitude", tb_lat.Value);
                    content.SetValue("longitude", tb_lng.Value);

                    content.SetValue("capacity", tb_capacity.Text);

                    var startDate = GetProperDate(dp_startdate.Text);
                    var endDate = GetProperDate(dp_enddate.Text);

                    content.SetValue("start", startDate);
                    content.SetValue("end", endDate);

                    var sync = tb_capacity.Text != publishedContent.GetPropertyValue<string>("capacity");

                    contentService.SaveAndPublishWithStatus(content);

                    if (sync)
                    {
                        var ev = new Event(publishedContent);
                        ev.syncCapacity();
                    }
                }
            }
            else
            {
                content = contentService.CreateContent(tb_name.Text, EventsRoot, "Event");

                content = SetDescription(content, hasLowKarma, ref hasAnchors);

                content.SetValue("venue", tb_venue.Text);
                content.SetValue("latitude", tb_lat.Value);
                content.SetValue("longitude", tb_lng.Value);

                content.SetValue("capacity", tb_capacity.Text);

                var startDate = GetProperDate(dp_startdate.Text);
                var endDate = GetProperDate(dp_enddate.Text);

                content.SetValue("start", startDate);
                content.SetValue("end", endDate);

                content.SetValue("owner", _member.Id);

                content.SetValue("signedup", 0);
                contentService.SaveAndPublishWithStatus(content);
            }

            var redirectUrl = umbraco.library.NiceUrl(content.Id);

            if (hasLowKarma && hasAnchors)
                SendPotentialSpamNotification(tb_name.Text, redirectUrl, _member.Id);

            Response.Redirect(redirectUrl);
        }

        private DateTime GetProperDate(string date)
        {
            var dateSplit = date.Split(' ');
            var dateTimeSplit = dateSplit[1].Split(':');

            var dateHour = int.Parse(dateTimeSplit[0]);
            var dateMinute = int.Parse(dateTimeSplit[1]);

            var dateDateSplit = dateSplit[0].Split('/');

            var dateDay = int.Parse(dateDateSplit[1]);
            var dateMonth = int.Parse(dateDateSplit[0]);
            var dateYear = int.Parse(dateDateSplit[2]);
            var startDate = new DateTime(dateYear, dateMonth, dateDay, dateHour, dateMinute, 0, 0);
            return startDate;
        }

        private IContent SetDescription(IContent content, bool hasLowKarma, ref bool hasAnchors)
        {
            content.SetValue("description", tb_desc.Text);

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

                content.SetValue("description", doc.DocumentNode.OuterHtml);
            }

            return content;
        }

        private static void SendPotentialSpamNotification(string eventName, string eventUrl, int memberId)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];

                var post = string.Format("Event: {0} - link: <a href=\"http://{1}\">http://our.umbraco.org/{1}</a><br />By member:<a href=\"http://our.umbraco.org/member/{2}\">http://our.umbraco.org/member/{2}</a>", eventName, eventUrl, memberId);

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