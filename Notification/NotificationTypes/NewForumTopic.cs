using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using uForum.Businesslogic;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;
using System.Web;

namespace NotificationsCore.NotificationTypes
{
    public class NewForumTopic: Notification
    {
        public NewForumTopic()
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

              
                Topic t = (Topic)args[0];

                Member s = (Member)args[2];

                //currently using document api instead of nodefactory
                Document f = new Document(t.ParentId);

               
                subject = string.Format(subject, f.Text);

                string domain = details.SelectSingleNode("//domain").InnerText;

               

                body = string.Format(body,
                    f.Text,
                    "http://" + domain + args[1].ToString(), s.Text, t.Title, HttpUtility.HtmlDecode(umbraco.library.StripHtml(t.Body)));

              
                SqlConnection conn = new SqlConnection(details.SelectSingleNode("//conn").InnerText);

               
                SqlCommand comm = new SqlCommand("Select memberId from forumSubscribers where forumId = @forumId", conn);
                comm.Parameters.AddWithValue("@forumId", t.ParentId);

                conn.Open();

                SqlDataReader dr = comm.ExecuteReader();

               

                while (dr.Read())
                {

                    int mid = dr.GetInt32(0);
                    try
                    {
                       
                        Member m = new Member(mid);


                        if (m.Id != t.MemberId
                            && (m.getProperty("bugMeNot").Value.ToString() != "1"))
                        {
                            MailMessage mm = new MailMessage();
                            mm.Subject = subject;
                            mm.Body = body;

                            mm.To.Add(m.Email);
                            mm.From = from;

                            c.Send(mm);
                        }

                    }
                    catch(Exception e)
                    {
                        umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1,
                           "[Notifications] Error sending mail to " + mid.ToString() + " " + e.Message);
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
