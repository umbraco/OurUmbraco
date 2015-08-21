using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Net.Mail;

namespace NotificationsCore.NotificationTypes
{
    public class MarkAsSolutionReminder: Notification
    {
        public MarkAsSolutionReminder()
        {
        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {

           

            SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);
            c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);

            MailAddress from = new MailAddress(
                details.SelectSingleNode("//from/email").InnerText,
                details.SelectSingleNode("//from/name").InnerText);

            string subject = details.SelectSingleNode("//subject").InnerText;
            string body = details.SelectSingleNode("//body").InnerText;

            SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["umbracoDbDSN"]);

            string select = @"select id, memberId from forumTopics where answer = 0
                            and created < getdate() - 7
                            and created > '2010-06-10 00:00:00'
                            and id not in (select topicId from notificationMarkAsSolution)
                            order by created desc;";


            SqlCommand comm = new SqlCommand(
                select, conn);
            conn.Open();
            SqlDataReader dr = comm.ExecuteReader();

            string domain = details.SelectSingleNode("//domain").InnerText;


            while (dr.Read())
            {
                int topicId = dr.GetInt32(0);

                string mbody = string.Format(body,
                        t.Title,
                        "https://" + domain + args[1].ToString());


                Member m = new Member(dr.GetInt32(1));


                if (m.getProperty("bugMeNot") != null || m.getProperty("bugMeNot").Value.ToString() != "1")
                {
                    MailMessage mm = new MailMessage();
                    mm.Subject = subject;
                    mm.Body = mbody;

                    mm.To.Add(m.Email);
                    mm.From = from;

                    c.Send(mm);
                }

                string insert =
                    "Insert into notificationMarkAsSolution(topicId, memberID, timestamp) values(@topicId, @memberID, getdate())";

                SqlCommand icomm = new SqlCommand(insert, conn);
                icomm.Parameters.AddWithValue("@topicId", topicId);
                icomm.Parameters.AddWithValue("@memberID", m.Id);
                icomm.ExecuteNonQuery();

            }
            conn.Close();
       
            return true;

        }
    }
}
