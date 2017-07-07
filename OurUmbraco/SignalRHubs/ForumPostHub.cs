using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;
using Umbraco.Core.Models;
using Microsoft.AspNet.SignalR;

namespace OurUmbraco.SignalRHubs
{
    public class ForumPostHub : Hub
    {

        public void SomeoneIsTyping(int messageId)
        {
            Clients.Others.someoneIsTypingResponse(messageId);
        }


        public void SomeonePosted(dynamic comment)
        {  
            Clients.Others.returnLatestComment(comment);
        }
        
    }
}