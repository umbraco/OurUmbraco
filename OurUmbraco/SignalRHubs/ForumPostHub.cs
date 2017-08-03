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

        public void SomeoneIsTyping(int messageId, int memberId, string name)
        {
            this.Clients.Others.someoneIsTyping(messageId, memberId, name);
        }


        public void SomeonePosted(dynamic comment)
        {  
            Clients.Others.returnLatestComment(comment);
        }

        public void SomeoneEdited(dynamic comment)
        {
            Clients.Others.returnEditedComment(comment);
        }


        public void CommentDeleted(int threadId,int commentId)
        {
            Clients.Others.DeleteComment(threadId, commentId);
        }


    }
}