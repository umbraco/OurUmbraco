using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace our.custom_Handlers {
    public class memberSave : umbraco.BusinessLogic.ApplicationBase {

        public memberSave() {
            umbraco.cms.businesslogic.member.Member.AfterSave += new umbraco.cms.businesslogic.member.Member.SaveEventHandler(Member_AfterSave);
        }


        void Member_AfterSave(umbraco.cms.businesslogic.member.Member sender, umbraco.cms.businesslogic.SaveEventArgs e) {

            string groups = "";
            foreach (MemberGroup mg in sender.Groups.Values) { 
                groups += mg.Text + ",";
            }

            sender.getProperty("groups").Value = groups.Trim().Trim(','); ;
            sender.XmlGenerate(new System.Xml.XmlDocument());
        }
    }
}
