using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Core.Logging;

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
            try
            {
                var room = UmbracoContext.Application.ApplicationCache.StaticCache.GetCacheItem("GitterRoom__" + roomName);
                return PartialView("~/views/partials/community/gitter.cshtml", room);
            }
            catch (KeyNotFoundException e)
            {
                //Try to render out a room with the partial that has not been added to the cache at bootup
                //From the appSettings - rather than throw a YSOD
                //Don't render a PartialView & log the error
                var err = string.Format("Trying to render Gitter Room that does not exist in the cache & AppSetting key at bootup'{0}'", roomName);
                Logger.Error<GitterRoomSurfaceController>(err, e);

                return null;
            }
        }

    }
}
