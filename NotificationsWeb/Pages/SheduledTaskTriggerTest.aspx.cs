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

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // Mark As Solution Reminder
            SqlConnection conn = new SqlConnection(umbraco.GlobalSettings.DbDSN);

            string select = @"select id, memberId from forumTopics where answer = 0
                                and created < getdate() - 7
                                and created > '2010-06-10 00:00:00'
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

                Topic t = new Topic(topicId);

                not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "MarkAsSolutionReminderSingle", topicId, memberId, NiceTopicUrl(t));
            }

            conn.Close();


            //Project vote

            string selectp = @"SELECT projectId, memberId
                                FROM [projectDownload] d
                                where memberId not in 
                                (select memberId from powersProject
                                where id = d.projectId and memberId = d.memberId)
                                and memberId != 0
                                and memberId not in
                                (select memberId from notificationVoteForProject
                                where projectId = d.projectId and memberId = d.memberId)
                                and timestamp < getdate() - 1";


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

            conn.Close();


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