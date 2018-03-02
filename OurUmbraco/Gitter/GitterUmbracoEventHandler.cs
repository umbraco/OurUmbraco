using System;
using System.Configuration;
using GitterSharp.Model;
using GitterSharp.Services;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Cache;

namespace OurUmbraco.Gitter
{
    public class GitterUmbracoEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Logger
            var logger = applicationContext.ProfilingLogger.Logger;

            //Gitter API token
            var apiToken = ConfigurationManager.AppSettings["GitterApiToken"];
            
            //Register & setup/connect to Gitter Realtime API
            var realtimeGitterService = new RealtimeGitterService(apiToken);

            //Used to communicate & fire events from server code to SignalR connected JS clients
            var gitter = GlobalHost.ConnectionManager.GetHubContext<GitterHub>();
            
            //Let's connect & listen...
            //This has to be done first before subscribe's
            realtimeGitterService.Connect();

            //Get the room names from the appsetting
            //'umbraco/playground,umbraco/some-other-room'

            //The Room ID we want to listen for events from
            //This appSetting contains a CSV of room IDs
            var roomNames = ConfigurationManager.AppSettings["GitterRooms"];


            if (string.IsNullOrEmpty(roomNames))
            {
                logger.Warn<GitterUmbracoEventHandler>("No Gitter Room Names are found in AppSetting key 'GitterRooms'");
                return;
            }

            //Gitter API
            var gitterService = new GitterService();

            //Do some AutoMapper - so we get our new dervived class with computed friendly date to use in the JSON
            AutoMapper.Mapper.CreateMap<Message, UmbracoMessage>();


            var rooms = roomNames.Split(',');

            //Setup the events for each room ID
            foreach (var roomName in rooms)
            {   
                //Call the API & get the Room ID
                //Store the topic & other info of the room object into the cache
                //Only at startup here will it ever get updated
                var room =
                    applicationContext.ApplicationCache.StaticCache.GetCacheItem<Room>("GitterRoom__" + roomName,
                        () =>
                        {
                            return gitterService.GetRoomInfo(roomName).Result;

                        });


                //User presence
                realtimeGitterService.SubscribeToUserPresence(room.Id)
                    .Subscribe(x =>
                    {
                        //Invoke signalR JS function prescenceEvent()
                        //Currently only fires a console.log with the data
                        gitter.Clients.Group(room.Id).prescenceEvent(new { prescenceEvent = x, room = room.Id });

                    }, onError:OnError);


                //Room Events
                realtimeGitterService.SubscribeToRoomEvents(room.Id)
                    .Subscribe(x =>
                    {
                        //Invoke signalR JS function roomEvent()
                        //Currently only fires a console.log with the data
                        gitter.Clients.Group(room.Id).roomEvent(new { roomEvent = x, room = room.Id });

                    }, onError:OnError);


                //Users in room
                realtimeGitterService.SubscribeToRoomUsers(room.Id)
                    .Subscribe(x =>
                    {
                        //Invoke signalR JS function userEvent()
                        //Currently only fires a console.log with the data
                        gitter.Clients.Group(room.Id).userEvent(new { userEvent = x, room = room.Id });

                    }, onError:OnError);
            

                //Chat messages
                realtimeGitterService.SubscribeToChatMessages(room.Id)
                    .Subscribe(x =>
                    {   
                        //Cast them with AutoMapper to our derivied class - with the computed friendly date on it
                        var umbracoMessage = AutoMapper.Mapper.Map<UmbracoMessage>(x.Model);

                        //Invoke signalR JS function chatMessage()
                        gitter.Clients.Group(room.Id).chatMessage(new { operation = x.Operation, message = umbracoMessage, room = room.Id });

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
