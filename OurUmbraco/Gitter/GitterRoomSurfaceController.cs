using System.Threading.Tasks;
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
        public async Task<ActionResult> RenderGitterRoom(string roomName)
        {
            //Use Gitter API to get info about the room such as it's ID, name & other info
            var room = await _gitterService.GetRoomInfo(roomName);

            var foo = room.Id;

            return PartialView("~/views/partials/community/gitter.cshtml", room);
        }

    }
}
