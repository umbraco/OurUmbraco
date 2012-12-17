using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using umbraco.cms.businesslogic.member;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.web;

namespace NotificationsCore.NotificationTypes
{
    public class VoteForProjectReminder : Notification
    {
        public VoteForProjectReminder()
        {

        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            
            SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);

            MailAddress from = new MailAddress(
                details.SelectSingleNode("//from/email").InnerText,
                details.SelectSingleNode("//from/name").InnerText);

            string subject = details.SelectSingleNode("//subject").InnerText;
            string body = details.SelectSingleNode("//body").InnerText;

            SqlConnection conn = new SqlConnection(details.SelectSingleNode("//conn").InnerText);

            string select = @"SELECT projectId, memberId
                            FROM [projectDownload] d
                            where memberId not in 
                            (select memberId from powersProject
                            where id = d.projectId and memberId = d.memberId)
                            and memberId != 0
                            and memberId not in
                            (select memberId from notificationVoteForProject
                            where projectId = d.projectId and memberId = d.memberId)
                            and timestamp < getdate() - 1";


            SqlCommand comm = new SqlCommand(
                select, conn);

            conn.Open();

            SqlDataReader dr = comm.ExecuteReader();

            while (dr.Read())
            {
                int projectId = dr.GetInt32(0);
                Member m = new Member(dr.GetInt32(1));

                Document d = new Document(projectId);

                if (m.getProperty("bugMeNot").Value.ToString() != "1")
                {
                    MailMessage mm = new MailMessage();
                    mm.Subject = subject;
                    mm.Body = body;

                    mm.To.Add(m.Email);
                    mm.From = from;

                    c.Send(mm);
                }

                string insert =
                    @"Insert into notificationVoteForProject(projectId, memberID, timestamp) 
                    values(@projectId, @memberId, getdate())";

                SqlCommand icomm = new SqlCommand(insert, conn);
                icomm.Parameters.AddWithValue("@projectId", projectId);
                icomm.Parameters.AddWithValue("@memberId", m.Id);
                icomm.ExecuteNonQuery();

            }
            conn.Close();

            return true;
        }
    }
}
