using System.Web.Http;
using System.Web.Routing;
using Umbraco.Core;

namespace uRepo
{
    public class AppStart : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            RouteTable.Routes.MapHttpRoute(
                name: "RepoApi", 
                routeTemplate: "webapi/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = "StarterKit" }
            );
        }
    }
}