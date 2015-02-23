using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using NotificationsCore;
using NotificationsWeb;

using umbraco.BusinessLogic;
using umbraco;
using umbraco.cms.businesslogic.member;

using Marketplace.Interfaces;


namespace Marketplace.Events
{
    public class DeliNotificationEvents : ApplicationBase
    {

        protected void NewMemberNotification()
        {

            //memberTasks.NewMember += new memberTasks.NewUIMemberEventHandler(memberTasks_NewMember);

        }

        void memberTasks_NewMember(Member sender, string unencryptedPassword, NewMemberUIEventArgs e)
        {

            if ((bool)sender.getProperty("createdByDeli").Value)
            {
                InstantNotification not = new InstantNotification();
                not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "DeliNewMember", sender, unencryptedPassword);
            }
        }

    }
}