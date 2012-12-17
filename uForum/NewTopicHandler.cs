using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace uForum {
    public class NewTopicHandler : umbraco.BusinessLogic.ApplicationBase {

        public NewTopicHandler(){

            uForum.Businesslogic.Topic.AfterCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Topic_AfterCreate);

        }

        void Topic_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            uForum.Businesslogic.Topic t = (uForum.Businesslogic.Topic)sender;

            Member mem = new Member(t.MemberId);
            int posts = 0;
            int.TryParse(mem.getProperty("forumPosts").Value.ToString(), out posts);

            mem.getProperty("forumPosts").Value = posts++;
            mem.Save();

            mem.XmlGenerate(new System.Xml.XmlDocument());

            Member.RemoveMemberFromCache(mem.Id);
            Member.AddMemberToCache(mem);


        }

    }
}
