using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using NotificationsCore;

namespace NotificationsWeb.EventHandlers
{
    public class Test: ApplicationBase
    {

        public Test()
        {
            //Document.AfterPublish += new Document.PublishEventHandler(Document_AfterPublish); 
        }

        void Document_AfterPublish(Document sender, umbraco.cms.businesslogic.PublishEventArgs e)
        {
            InstantNotification not = new InstantNotification();

            not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "NewComment", sender);
        }
    }
}
