﻿using System;
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

            //The Room ID we want to listen for events from
            var roomId = ConfigurationManager.AppSettings["GitterRoomId"];

            //Register & setup/connect to Gitter Realtime API
            var realtimeGitterService = new RealtimeGitterService(apiToken);

            var gitter = GlobalHost.ConnectionManager.GetHubContext<GitterHub>();
            
            //Let's connect & listen...
            realtimeGitterService.Connect();

            //User presence
            realtimeGitterService.SubscribeToUserPresence(roomId)
                .Subscribe(x =>
                {
                    //Proxy the request with SignalR Hub
                    gitter.Clients.All.prescenceEvent(x);
                }, onError:OnError);


            //Room Events
            realtimeGitterService.SubscribeToRoomEvents(roomId)
                .Subscribe(x =>
                {
                    gitter.Clients.All.roomEvent(x);
                }, onError:OnError);


            //Users in room
            realtimeGitterService.SubscribeToRoomUsers(roomId)
                .Subscribe(x =>
                {
                    gitter.Clients.All.userEvent(x);
                }, onError:OnError);
            

            //Chat messages
            realtimeGitterService.SubscribeToChatMessages(roomId)
                .Subscribe(x =>
                {
                    gitter.Clients.All.chatMessage(x);
                }, onError:OnError);
        }

        private void OnError(Exception exception)
        {
            //TODO: Log exception/warning
            //Maybe connection issues?!

            throw new NotImplementedException();
        }
    }
}
