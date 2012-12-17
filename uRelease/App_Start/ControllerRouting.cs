using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: PreApplicationStartMethod(typeof(uRelease.App_Start.ControllerRouting), "Setup")]
namespace uRelease.App_Start
{
    public class ControllerRouting
    {
        public static void Setup()
        {
            RouteTable.Routes.MapRoute(
                "ApiRoute", // Route name
                "api/{action}/{ids}", // URL with parameters
                new { controller = "Api", action = "Aggregate", ids = UrlParameter.Optional } // Parameter defaults
                );
        }
    }
}