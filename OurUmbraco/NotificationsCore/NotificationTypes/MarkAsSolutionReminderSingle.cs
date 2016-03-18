using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;

namespace NotificationsCore.NotificationTypes
{
    public class MarkAsSolutionReminderSingle : Notification
    {
        public MarkAsSolutionReminderSingle()
        {

        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            try
            {
                SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);
                c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);

                MailAddress from = new MailAddress(
                    details.SelectSingleNode("//from/email").InnerText,
                    details.SelectSingleNode("//from/name").InnerText);

                string subject = details.SelectSingleNode("//subject").InnerText;
                string body = details.SelectSingleNode("//body").InnerText;

                string domain = details.SelectSingleNode("//domain").InnerText;

                int topicId = int.Parse(args[0].ToString());
                int memberId = int.Parse(args[1].ToString());

                uForum.Businesslogic.Topic t = uForum.Businesslogic.Topic.GetTopic(topicId);


                Member m = new Member(memberId);

                body = string.Format(body,
                    t.Title,
                   "https://" + domain + args[2].ToString());


                if (m.getProperty("bugMeNot").Value.ToString() != "1")
                {
                    MailMessage mm = new MailMessage();
                    mm.Subject = subject;
                    mm.Body = body;

                    mm.To.Add(m.Email);
                    mm.From = from;

                    c.Send(mm);
                }

                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["umbracoDbDSN"]);


                conn.Open();



                string insert =
                       "Insert into notificationMarkAsSolution(topicId, memberID, timestamp) values(@topicId, @memberID, getdate())";

                SqlCommand icomm = new SqlCommand(insert, conn);
                icomm.Parameters.AddWithValue("@topicId", topicId);
                icomm.Parameters.AddWithValue("@memberID", m.Id);
                icomm.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception e)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications]" + e.Message);
            }
            return true;
        }
    }
}
