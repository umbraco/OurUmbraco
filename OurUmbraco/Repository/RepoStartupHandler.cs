using System.Web.Http;
using System.Web.Routing;
using Umbraco.Core;

namespace OurUmbraco.Repository
{
    public class AppStart : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            RouteTable.Routes.MapHttpRoute(
                name: "PackageRepositoryApi",
                routeTemplate: "webapi/packages/v1/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "PackageRepository" },
                constraints: new { }
            );

            RouteTable.Routes.MapHttpRoute(
                name: "RepoApi",
                routeTemplate: "webapi/starterkit/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "StarterKit" },
                constraints: new { }
            );
            
            RouteTable.Routes.MapHttpRoute(
                name: "ReleasesApi",
                routeTemplate: "webapi/releases/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "Releases" },
                constraints: new { }
            );

            GlobalConfiguration.Configuration.EnableCors();
        }
    }
}