using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Data.SqlClient;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;
using System.Web;
using OurUmbraco.Forum.Models;

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

                var topic = (Topic)args[0];
                string url = (string)args[1];
                var memberName = (string)args[2];

                if (topic.IsSpam)
                {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, string.Format("[Notifications] Topic ID {0} is marked as spam, no notification sent{0}", topic.Id));
                    return true;
                }
                    

                SmtpClient c = new SmtpClient();
                //c.Credentials = new System.Net.NetworkCredential(details.SelectSingleNode("//username").InnerText, details.SelectSingleNode("//password").InnerText);

                MailAddress from = new MailAddress(
                    details.SelectSingleNode("//from/email").InnerText,
                    details.SelectSingleNode("//from/name").InnerText);

                string subject = details.SelectSingleNode("//subject").InnerText;
                string body = details.SelectSingleNode("//body").InnerText;
              
               

                //currently using document api instead of nodefactory
                Document forum = new Document(topic.ParentId);

               
                subject = string.Format(subject, forum.Text);

                var domain = details.SelectSingleNode("//domain").InnerText;
                
                body = string.Format(body, forum.Text, "https://" + domain + url, memberName, topic.Title, HttpUtility.HtmlDecode(umbraco.library.StripHtml(topic.Body)));


                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString);

               
                SqlCommand comm = new SqlCommand("Select memberId from forumSubscribers where forumId = @forumId", conn);
                comm.Parameters.AddWithValue("@forumId", topic.ParentId);
                conn.Open();

                SqlDataReader dr = comm.ExecuteReader();
                
                while (dr.Read())
                {

                    int mid = dr.GetInt32(0);
                    try
                    {
                       
                        Member m = new Member(mid);


                        if (m.Id != topic.MemberId
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
