using System;
using System.Configuration;
using GitterSharp.Services;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;

namespace OurUmbraco.Gitter
{
    public class GitterUmbracoEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Gitter API token
            var apiToken = ConfigurationManager.AppSettings["GitterApiToken"];
            
            //Register & setup/connect to Gitter Realtime API
            var realtimeGitterService = new RealtimeGitterService(apiToken);

            //Used to communicate & fire events from server code to SignalR connected JS clients
            var gitter = GlobalHost.ConnectionManager.GetHubContext<GitterHub>();
            
            //Let's connect & listen...
            //This has to be done first before subscribe's
            realtimeGitterService.Connect();

            //The Room ID we want to listen for events from
            //This appSetting contains a CSV of room IDs
            var roomIds = ConfigurationManager.AppSettings["GitterRoomIds"];
            if (string.IsNullOrEmpty(roomIds))
                return;

            var rooms = roomIds.Split(',');

            //Setup the events for each room ID
            foreach (var roomId in rooms)
            {
                //User presence
                realtimeGitterService.SubscribeToUserPresence(roomId)
                    .Subscribe(x =>
                    {
                        //Proxy the request with SignalR Hub
                        gitter.Clients.Group(roomId).prescenceEvent(x);
                    }, onError:OnError);


                //Room Events
                realtimeGitterService.SubscribeToRoomEvents(roomId)
                    .Subscribe(x =>
                    {
                        gitter.Clients.Group(roomId).roomEvent(x);
                    }, onError:OnError);


                //Users in room
                realtimeGitterService.SubscribeToRoomUsers(roomId)
                    .Subscribe(x =>
                    {
                        gitter.Clients.Group(roomId).userEvent(x);
                    }, onError:OnError);
            

                //Chat messages
                realtimeGitterService.SubscribeToChatMessages(roomId)
                    .Subscribe(x =>
                    {
                        gitter.Clients.Group(roomId).chatMessage(x);
                    }, onError:OnError);
            }
            
            

            
        }

        private void OnError(Exception exception)
        {
            //TODO: Log exception/warning
            //Maybe connection issues?!

            //Managed to get an ex
            //System.InvalidOperationException: Not connected to server.


            throw exception;
        }
    }
}
