using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Gitter
{
    public class GitterRoomSurfaceController : SurfaceController
    {
        private GitterService _gitterService;

        public GitterRoomSurfaceController()
        {
            _gitterService = new GitterService();
        }

        [ChildActionOnly]
        public ActionResult RenderGitterRoom(string roomName)
        {
            //Use Gitter API to get info about the room such as it's ID, name & other info
            //Fetch from runtime cache - stored at appstartup event handler
            var room = UmbracoContext.Application.ApplicationCache.StaticCache.GetCacheItem("GitterRoom__" + roomName);
            return PartialView("~/views/partials/community/gitter.cshtml", room);
        }

    }
}
