using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GitterProxyHub : Hub
    {
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
        
    }
}
