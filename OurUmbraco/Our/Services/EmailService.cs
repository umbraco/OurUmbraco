using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OurUmbraco.Our.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace OurUmbraco.Our.Services
{
    public class EmailService
    {
        public void SendActivationMail(IMember member)
        {
            try
            {
                const string subject = "Activate your account on our.umbraco.com";

                var body = RenderPartial(
                    "Partials/Emails/ActivationEmailTemplate.cshtml",
                    new ActivationEmailModel(member));

                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = body.ToString(),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(member.Email);

                mailMessage.From = new MailAddress("robot@umbraco.org");

                var smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                var error =$"*ERROR* sending activation mail for member {member.Email} - {ex.Message} {ex.StackTrace} {ex.InnerException}";
                SendSlackNotification(error);
                LogHelper.Error<Forum.Library.Utils>(error, ex);
            }
        }

        internal class WebFormController : Controller { }

        internal static HtmlString RenderPartial(string partialName, object model)
        {
            // Get a wrapper for the legacy WebForm context
            var context = new HttpContextWrapper(HttpContext.Current);

            // Create a mock route that points to the empty controller
            var route = new RouteData();
            route.Values.Add("controller", "WebFormController");

            // Create a controller context for the route and http context
            var ctx = new ControllerContext(new RequestContext(context, route), new WebFormController());

            if (partialName.StartsWith("Partials/")) partialName = "~/Views/" + partialName;
            if (!partialName.EndsWith(".cshtml")) partialName += ".cshtml";

            // Find the partial view using the view engine
            var result = ViewEngines.Engines.FindPartialView(ctx, partialName);
            if (result?.View == null) throw new Exception($"Partial view \"{partialName}\" not found.");

            // Render the partial view to a StringBuilder
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                // Create a view context and assign the model
                var vdd = new ViewDataDictionary { Model = model };
                var vctx = new ViewContext(ctx, result.View, vdd, new TempDataDictionary(), writer);

                // Render the partial view
                result.View.Render(vctx, writer);
            }

            return new HtmlString(sb.ToString());
        }

        private void SendSlackNotification(string body)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    {"channel", ConfigurationManager.AppSettings["SlackChannel"]},
                    {"token", ConfigurationManager.AppSettings["SlackToken"]},
                    {"username", ConfigurationManager.AppSettings["SlackUsername"]},
                    {"icon_url", ConfigurationManager.AppSettings["SlackIconUrl"]},
                    {"text", body}
                };

                try
                {
                    client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Forum.Library.Utils>("Posting update to Slack failed", ex);
                }
            }
        }
    }
}
