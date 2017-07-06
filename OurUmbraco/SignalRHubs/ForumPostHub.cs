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
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using OurUmbraco.Forum.Api;
    using OurUmbraco.Forum.Services;

    using umbraco.NodeFactory;
    using Umbraco.Core;
    using Umbraco.Web;

    using Task = System.Threading.Tasks.Task;

    public class ForumPostHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.Others.addNewMessageToPage(name, message);
        }

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