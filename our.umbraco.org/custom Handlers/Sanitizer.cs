using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace our.custom_Handlers {
    public class SanitizerHandler : umbraco.BusinessLogic.ApplicationBase {

        public SanitizerHandler() {
            uForum.Businesslogic.Topic.BeforeCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Topic_BeforeCreate);
            uForum.Businesslogic.Topic.BeforeUpdate += new EventHandler<uForum.Businesslogic.UpdateEventArgs>(Topic_BeforeUpdate);

            //uForum.Businesslogic.Comment.BeforeCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Comment_BeforeCreate);
            //uForum.Businesslogic.Comment.BeforeUpdate += new EventHandler<uForum.Businesslogic.UpdateEventArgs>(Comment_BeforeUpdate);

            uWiki.Businesslogic.WikiPage.BeforeCreate += new EventHandler<uWiki.Businesslogic.CreateEventArgs>(WikiPage_BeforeCreate);
            uWiki.Businesslogic.WikiPage.BeforeUpdate += new EventHandler<uWiki.Businesslogic.UpdateEventArgs>(WikiPage_BeforeUpdate);
        }

        void WikiPage_BeforeUpdate(object sender, uWiki.Businesslogic.UpdateEventArgs e) {
            SanitizeWiki((uWiki.Businesslogic.WikiPage)sender);
        }

        void WikiPage_BeforeCreate(object sender, uWiki.Businesslogic.CreateEventArgs e) {
            SanitizeWiki((uWiki.Businesslogic.WikiPage)sender);
        }

        void Comment_BeforeUpdate(object sender, uForum.Businesslogic.UpdateEventArgs e) {
            SanitizeComment((uForum.Businesslogic.Comment)sender);
        }

        void Comment_BeforeCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            SanitizeComment((uForum.Businesslogic.Comment)sender);
        }

        void Topic_BeforeUpdate(object sender, uForum.Businesslogic.UpdateEventArgs e) {
            SanitizeTopic((uForum.Businesslogic.Topic)sender);
        }

        void Topic_BeforeCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            SanitizeTopic((uForum.Businesslogic.Topic)sender);
        }

        private void SanitizeTopic(uForum.Businesslogic.Topic t) {
            //t.Body = our.Utills.Sanitize(t.Body);
            t.Title = our.Utills.Sanitize(t.Title);
        }

        private void SanitizeComment(uForum.Businesslogic.Comment c) {
            c.Body = our.Utills.Sanitize(c.Body);
        }

        private void SanitizeWiki(uWiki.Businesslogic.WikiPage wp) {
            wp.Body = our.Utills.Sanitize(wp.Body);
            wp.Title = our.Utills.Sanitize(wp.Title);
        }


    }
}
