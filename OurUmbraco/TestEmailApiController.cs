using OurUmbraco.Our.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using Umbraco.Web.WebApi;
using static OurUmbraco.Our.Services.EmailService;
using System.Web.Http;

namespace OurUmbraco
{
    public class TestEmailApiController : UmbracoApiController
    {
        [System.Web.Http.HttpGet]
        public IHttpActionResult SendTestEmail(string email, string secret)
        {
            var bearerToken = ConfigurationManager.AppSettings["CollabBearerToken"];
            if (secret != bearerToken)
                return Ok("Not allowed");

            const string subject = "Activate your account on our.umbraco.com";
            
            var member = Services.MemberService.GetById(4576);

            var body = RenderPartial(
                "Partials/Emails/ActivationEmailTemplate.cshtml",
                new ActivationEmailModel(member));

            var mailMessage = new MailMessage
            {
                Subject = subject,
                Body = body.ToString(),
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            mailMessage.From = new MailAddress("robot@umbraco.com");

            var smtpClient = new SmtpClient();
            smtpClient.Send(mailMessage);

            return Ok("OK");
        }


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

    }
}
