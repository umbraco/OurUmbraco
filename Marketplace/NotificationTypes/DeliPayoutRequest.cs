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
    public class DeliPayoutRequest : Notification
    {
        public DeliPayoutRequest()
        {
        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            IPayoutProvider payoutProvider = (IPayoutProvider)MarketplaceProviderManager.Providers["PayoutProvider"];
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
                string notify = details.SelectSingleNode("//to").InnerText;

                // deli order
                IPayout dPayout = (IPayout)args[0];

                // deli member
                IMember dMember = (IMember)MemberProvider.GetMemberById(dPayout.VendorId);

                string domain = details.SelectSingleNode("//domain").InnerText;

                body = string.Format(body,
                    dMember.Name,
                    GetPayoutValue(dPayout.OrderItems).ToString());

                MailMessage mm = new MailMessage();
                mm.Subject = subject;
                mm.Body = body;

                mm.To.Add(notify);
                mm.From = from;

                c.Send(mm);

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] Deli payout message sent, payout id: " + dPayout.Id.ToString());
            }
            catch (Exception e)
            {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications]" + e.Message + "++++++++++++++++++++++" + e.Source + "-------" + e.StackTrace);

            }

            return true;
        }

        private double GetPayoutValue(IEnumerable<IOrderItem> items)
        {
            double val = 0.00;
            foreach (var i in items)
            {
                val += i.PayoutAmount;
            }

            return val;
        }
    }
}