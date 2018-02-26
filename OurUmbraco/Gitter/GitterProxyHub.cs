using System.Threading.Tasks;
using GitterSharp.Model.Realtime;
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

        //We only send messages from server to the client
        //And the client never calls back to the server with an update
        //This is a one way street

        public void SendRealtimePresenceEvent(RealtimeUserPresence userPresence)
        {
            Clients.All.prescenceEvent(userPresence);
        }

        public void SendRealtimeRoomEvent(RealtimeRoomEvent roomEvent)
        {
            Clients.All.roomEvent(roomEvent);
        }
        
        public void SendRealtimeUserEvent(RealtimeRoomUser roomUser)
        {
            Clients.All.userEvent(roomUser);
        }
        
        public void SendRealtimeChatMessageEvent(RealtimeChatMessage chatMessage)
        {
            Clients.All.chatMessage(chatMessage);
        }

        public async Task GetLatestChatMessage()
        {
            var latestMessage = await _gitterService.GetMessages(1);
            Clients.All.fetchedChatMessage(latestMessage);
        }
    }
}
