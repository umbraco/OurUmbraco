using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using uForum.Models;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;
using System.Web;
using Umbraco.Core.Models;

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
                Comment com = (Comment)args[0];
                Topic topic = (Topic)args[1];
                IMember mem = (IMember)args[2];


                if (com.IsSpam)
                {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, string.Format("[Notifications] Comment Id {{0}} is marked as spam, no notification sent{0}", com.Id));
                    return true;
                }


                //SMTP SETTINGS
                SmtpClient c = new SmtpClient(details.SelectSingleNode("//smtp").InnerText);
                c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);

                //SENDER ADDRESS
                MailAddress from = new MailAddress(
                    details.SelectSingleNode("//from/email").InnerText,
                    details.SelectSingleNode("//from/name").InnerText);

                //Notification details
                var domain = details.SelectSingleNode("//domain").InnerText;
                var subject = string.Format(details.SelectSingleNode("//subject").InnerText, topic.Title);
                var body = details.SelectSingleNode("//body").InnerText;
                body = string.Format(body, topic.Title, "http://" + domain + args[1], mem.Name,  HttpUtility.HtmlDecode(umbraco.library.StripHtml(com.Body)));

                
                //connect to DB
                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["umbracoDbDSN"]);
                SqlCommand comm = new SqlCommand("Select memberId from forumTopicSubscribers where topicId = @topicId", conn);
                comm.Parameters.AddWithValue("@topicId", topic.Id);
                conn.Open();
                

                //shit this must be so fucking slow
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
