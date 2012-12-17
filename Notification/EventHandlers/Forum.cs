using System;
using System.Collections.Generic;
using System.Text;
using uForum.Businesslogic;

namespace NotificationsCore.EventHandlers
{
    public class Forum : umbraco.BusinessLogic.ApplicationBase 
    {
        public Forum()
        {
            Comment.AfterCreate += new EventHandler<CreateEventArgs>(Comment_AfterCreate);
        }

        void Comment_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e)
        {
            Comment c = (Comment)sender;

            //InstantNotification.Execute("NewComment", c.Id);
        }
    }
}
