using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;

namespace NotificationsCore.NotificationTypes
{
    public class VoteForProjectReminderSingle : Notification
    {
        public VoteForProjectReminderSingle()
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


                int projectId = int.Parse(args[0].ToString());
                int memberId = int.Parse(args[1].ToString());

                string domain = details.SelectSingleNode("//domain").InnerText;

                Member m = new Member(memberId);

                Document d = new Document(projectId);

                body = string.Format(body,
                    d.Text,
                    "http://" + domain + args[2].ToString());

                if (m.getProperty("bugMeNot").Value.ToString() != "1" && 
                    d.getProperty("owner").Value.ToString() != m.Id.ToString())
                {
                    MailMessage mm = new MailMessage();
                    mm.Subject = subject;
                    mm.Body = body;

                    mm.To.Add(m.Email);
                    mm.From = from;

                    c.Send(mm);
                }

                SqlConnection conn = new SqlConnection(details.SelectSingleNode("//conn").InnerText);


                conn.Open();

                string insert =
                       @"Insert into notificationVoteForProject(projectId, memberID, timestamp) 
                        values(@projectId, @memberId, getdate())";

                SqlCommand icomm = new SqlCommand(insert, conn);
                icomm.Parameters.AddWithValue("@projectId", projectId);
                icomm.Parameters.AddWithValue("@memberId", memberId);
                icomm.ExecuteNonQuery();

                conn.Close();
               
            }
            catch (Exception e)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1,"[Notifications]" + e.Message);
            }
            return true;
        }
        

    }
}
