using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using NotificationsCore;
using uForum.Businesslogic;
using umbraco.cms.businesslogic.web;

namespace NotificationsWeb.Pages
{
    public partial class SheduledTaskTriggerTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] sheduled trigger");
            SendMarkAsSolutionReminders();
            SendProjectVoteReminders();
        }


        private void SendMarkAsSolutionReminders()
        {
            // Mark As Solution Reminder
            using (SqlConnection conn = new SqlConnection(umbraco.GlobalSettings.DbDSN))
            {
                try
                {
                    string select = @"select id, memberId from forumTopics where answer = 0
                                and created < getdate() - 7
                                and created > '2012-09-16 00:00:00'
                                and replies > 0
                                and id not in (select topicId from notificationMarkAsSolution)
                                order by created desc;";


                    SqlCommand comm = new SqlCommand(
                        select, conn);

                    conn.Open();

                    SqlDataReader dr = comm.ExecuteReader();

                    while (dr.Read())
                    {
                        InstantNotification not = new InstantNotification();

                        int topicId = dr.GetInt32(0);
                        int memberId = dr.GetInt32(1);

                        Topic t = Topic.GetTopic(topicId);


                        not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "MarkAsSolutionReminderSingle", topicId, memberId, NiceTopicUrl(t));
                    }

                }
                catch (Exception ex)
                {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] " + ex.Message);
                   
                }
                finally
                {
                    if(conn != null)
                        conn.Close();
                }
            }
        }

        private void SendProjectVoteReminders()
        {
            //Project vote

            using (SqlConnection conn = new SqlConnection(umbraco.GlobalSettings.DbDSN))
            {
                try
                {
                    string selectp = @"SELECT projectId, memberId
                                FROM [projectDownload] d
                                where memberId not in 
                                (select memberId from powersProject
                                where id = d.projectId and memberId = d.memberId)
                                and memberId != 0
                                and memberId not in
                                (select memberId from notificationVoteForProject
                                where projectId = d.projectId and memberId = d.memberId)
                                and timestamp < getdate() - 1
                                and timestamp > '2012-09-22 00:00:00'";


                    SqlCommand commp = new SqlCommand(
                        selectp, conn);

                    conn.Open();

                    SqlDataReader drp = commp.ExecuteReader();

                    while (drp.Read())
                    {
                        InstantNotification not = new InstantNotification();

                        int projectId = drp.GetInt32(0);
                        int memberId = drp.GetInt32(1);

                        Document d = new Document(projectId);

                        not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "VoteForProjectReminderSingle", projectId, memberId, umbraco.library.NiceUrl(d.Id));
                    }

                }
                catch (Exception ex)
                {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[Notifications] " + ex.Message);

                }
                finally
                {
                    if(conn != null)
                        conn.Close();
                }

            }
        }

        private static string NiceTopicUrl(Topic t)
        {

            if (t.Exists)
            {
                string _url = umbraco.library.NiceUrl(t.ParentId);

                if (umbraco.GlobalSettings.UseDirectoryUrls)
                {
                    return "/" + _url.Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName;
                }
                else
                {
                    return "/" + _url.Substring(0, _url.LastIndexOf('.')).Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName + ".aspx";
                }
            }
            else
            {
                return "";
            }
        }
    }
}