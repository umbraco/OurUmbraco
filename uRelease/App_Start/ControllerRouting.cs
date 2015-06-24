using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using uRelease;

[assembly: PreApplicationStartMethod(typeof(ControllerRouting), "Setup")]
namespace uRelease
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