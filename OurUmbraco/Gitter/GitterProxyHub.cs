using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace OurUmbraco.Gitter
{
    /// <summary>
    /// This is a Simple SignalR Hub
    /// That proxy's the Realtime events from Gitter's API to anyone connected to the our.umb site
    /// Where we can display a total count of users online currently or similar
    /// </summary>
    public class GitterHub : Hub
    {
        private GitterService _gitterService;

        public GitterHub()
        {
            _gitterService = new GitterService();
        }

        public async Task GetLatestChatMessages(string roomId, int numberOfMessages)
        {
            //Enfore a hard limit incase people try to request a large set from gitter
            numberOfMessages = numberOfMessages > 10 ? 10 : numberOfMessages;

            var latestMessages = await _gitterService.GetMessages(roomId, numberOfMessages);

            //Cast them with AutoMapper to our derivied class - with the computed friendly date on it
            var umbracoMessages = latestMessages.Select(AutoMapper.Mapper.Map<UmbracoMessage>).ToList();

            //Add the current SignalR connection to a group (So they only get messages for this room)
            await Groups.Add(Context.ConnectionId, roomId);

            //Call only the SignalR clients who are part of this room/group
            //Otherwise we will send messages from other rooms
            Clients.Group(roomId).fetchedChatMessage(new { messages = umbracoMessages, room = roomId });
        }
    }
}
