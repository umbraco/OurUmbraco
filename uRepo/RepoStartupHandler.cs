using System.Web;
using System.Web.Http;
using System.Web.Routing;
using uRepo;

[assembly: PreApplicationStartMethod(typeof(AppStart), "PreStart")]
namespace uRepo
{
    public static class AppStart
    {
        public static void PreStart()
        {
            RouteTable.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "webapi/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}