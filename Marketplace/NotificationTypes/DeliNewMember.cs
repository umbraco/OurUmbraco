using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

using NotificationsCore;

using umbraco.cms.businesslogic.member;

using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.NotificationTypes
{
    public class DeliNewMember : Notification
    {

        public DeliNewMember()
        {

        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            IMemberProvider MemberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];

            try
            {
                SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);
                c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);

                MailAddress from = new MailAddress(
                    details.SelectSingleNode("//from/email").InnerText,
                    details.SelectSingleNode("//from/name").InnerText);

                string subject = details.SelectSingleNode("//subject").InnerText;
                string body = details.SelectSingleNode("//body").InnerText;

                //Member m = (Member)args[0];
                // deli member
                IMember dMember = (IMember)args[0];

                string domain = details.SelectSingleNode("//domain").InnerText;

                body = string.Format(body,
                    dMember.Name,
                    dMember.Email,
                    args[1].ToString());

                MailMessage mm = new MailMessage();
                mm.Subject = subject;
                mm.Body = body;

                mm.To.Add(dMember.Email);
                mm.From = from;

                c.Send(mm);

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] New deli member message sent" + dMember.Name);
            }
            catch (Exception e)
            {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications]" + e.Message);

            }

            return true;
        }
    }
}