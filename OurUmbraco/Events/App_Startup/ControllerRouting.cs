using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OurUmbraco.Events.App_Startup;

[assembly: PreApplicationStartMethod(typeof(ControllerRouting), "Setup")]
namespace OurUmbraco.Events.App_Startup
{
    public class ControllerRouting
    {
        public static void Setup()
        {
            RouteTable.Routes.MapRoute(
                "MeetupApiRoute", // Route name
                "meetup/api/{action}/{ids}", // URL with parameters
                new { controller = "Meetup", action = "GetAllFromFile", ids = UrlParameter.Optional } // Parameter defaults
                );
        }
    }
}