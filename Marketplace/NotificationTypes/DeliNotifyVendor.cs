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
    public class DeliNotifyVendor : Notification
    {
        public DeliNotifyVendor()
        {
        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {

            IListingProvider listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            ILicenseProvider licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            try
            {
                SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);
                c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);


                MailAddress from = new MailAddress(
                    details.SelectSingleNode("//from/email").InnerText,
                    details.SelectSingleNode("//from/name").InnerText);

                string subject = details.SelectSingleNode("//subject").InnerText;
                string body = details.SelectSingleNode("//body").InnerText;

                // deli ordered item
                IOrderItem dOrder = (IOrderItem)args[0];

                // deli listing
                IListingItem dListing = listingProvider.GetListing(dOrder.ListingItemId);

                // deli member
                IMember dMember = (IMember)args[1];

                string domain = details.SelectSingleNode("//domain").InnerText;

                body = string.Format(body,
                    dMember.Name,
                    dOrder.Id.ToString(),
                    dListing.Name,
                    licenseProvider.GetLicense(dOrder.LicenseId).LicenseType.ToString(),
                    dOrder.Quantity,
                    dMember.Name,
                    dMember.Email);

                MailMessage mm = new MailMessage();
                mm.Subject = subject;
                mm.Body = body;

                mm.To.Add(dListing.Vendor.Member.Email);
                mm.From = from;

                c.Send(mm);

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] Deli order vendor notification message sent, order item id: " + dOrder.Id.ToString());
            }
            catch (Exception e)
            {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications]" + e.Message);

            }

            return true;
        }
    }
}