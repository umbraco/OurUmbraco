using System.Web.Configuration;
using Microsoft.Owin;
using Owin;
using Umbraco.Web;
using OurUmbraco.Site;

[assembly: OwinStartup("UmbracoStandardOwinStartup", typeof(UmbracoStandardOwinStartup))]
namespace OurUmbraco.Site
{
    /// <summary>
    /// The standard way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
    /// </remarks>
    public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            //ensure the default options are configured
            base.Configuration(app);

            var clientId = WebConfigurationManager.AppSettings["GoogleOAuthClientID"];
            var secret = WebConfigurationManager.AppSettings["GoogleOAuthSecret"];
            app.ConfigureBackOfficeGoogleAuth(clientId, secret);
        }
    }
}
