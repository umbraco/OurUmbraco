using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NotificationsCore;
using Marketplace.Interfaces;
using Marketplace.Providers;
using System.Net.Mail;

namespace Marketplace.NotificationTypes
{
    public class DeliOrderComplete : Notification
    {
        public DeliOrderComplete()
        {
        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            IOrderProvider OrderProvider = (IOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
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

                // deli order
                IOrder dOrder = (IOrder)args[0];

                // deli member
                IMember dMember = (IMember)args[1];

                string domain = details.SelectSingleNode("//domain").InnerText;

                body = string.Format(body,
                    dMember.Name,
                    dOrder.Id.ToString(),
                    dOrder.ProcessedTotal.ToString(),
                    dOrder.Currency.ToString(),
                    dOrder.PaymentDate.ToString(),
                    args[2].ToString());

                MailMessage mm = new MailMessage();
                mm.Subject = subject;
                mm.Body = body;

                mm.To.Add(dMember.Email);
                mm.From = from;

                c.Send(mm);

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] Deli order complete message sent, order id: " + dOrder.Id.ToString());
            }
            catch (Exception e)
            {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications]" + e.Message);

            }

            return true;
        }
    }
}