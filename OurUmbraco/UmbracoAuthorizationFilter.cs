using System.Linq;
using System.Web;
using Hangfire.Dashboard;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco
{
    [UmbracoAuthorize]
    public class UmbracoAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var http = new HttpContextWrapper(HttpContext.Current);
            var ticket = http.GetUmbracoAuthTicket();
            http.AuthenticateCurrentRequest(ticket, true);

            var user = UmbracoContext.Current.Security.CurrentUser;

            return user != null && user.Groups.Any(g => g.Alias == "admin");
        }
    }
}
