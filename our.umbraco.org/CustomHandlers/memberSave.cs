using umbraco.cms.businesslogic.member;
using Umbraco.Core;

namespace our.CustomHandlers
{
    public class MemberSave : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Member.AfterSave += Member_AfterSave;
        }


        void Member_AfterSave(Member sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {

            string groups = "";
            foreach (MemberGroup mg in sender.Groups.Values)
            {
                groups += mg.Text + ",";
            }

            sender.getProperty("groups").Value = groups.Trim().Trim(','); ;
            sender.XmlGenerate(new System.Xml.XmlDocument());
        }
    }
}
