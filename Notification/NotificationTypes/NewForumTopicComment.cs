using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using uForum.Businesslogic;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;
using System.Web;

namespace NotificationsCore.NotificationTypes
{
    public class NewForumTopicComment: Notification
    {
        public NewForumTopicComment()
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

                Comment com = (Comment)args[0];
                Topic t = new Topic(com.TopicId);
                subject = string.Format(subject, t.Title);

                Member s = (Member)args[2];

                string domain = details.SelectSingleNode("//domain").InnerText;

                body = string.Format(body, 
                    t.Title,
                    "http://" + domain + args[1].ToString(), s.Text,  HttpUtility.HtmlDecode(umbraco.library.StripHtml(com.Body)));


                SqlConnection conn = new SqlConnection(details.SelectSingleNode("//conn").InnerText);



                SqlCommand comm = new SqlCommand("Select memberId from forumTopicSubscribers where topicId = @topicId", conn);
                comm.Parameters.AddWithValue("@topicId", t.Id);

                conn.Open();

                SqlDataReader dr = comm.ExecuteReader();
                
                while (dr.Read())
                {

                    int mid = dr.GetInt32(0);
                    try
                    {
                        Member m = new Member(mid);

                        if (m.Id != com.MemberId
                            && m.getProperty("bugMeNot").Value.ToString() != "1")
                        {
                            MailMessage mm = new MailMessage();
                            mm.Subject = subject;
                            mm.Body = body;

                            mm.To.Add(m.Email);
                            mm.From = from;

                            c.Send(mm);
                        }
                    }
                    catch (Exception e)
                    {
                        umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, 
                            "[Notifications] Error sending mail to " +  mid.ToString() + " " + e.Message);
                    }
                }

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
