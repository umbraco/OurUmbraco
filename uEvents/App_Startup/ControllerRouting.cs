using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: PreApplicationStartMethod(typeof(uEvents.App_Start.ControllerRouting), "Setup")]
namespace uEvents.App_Start
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