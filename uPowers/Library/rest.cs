using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Web;
using System.Web.Security;

namespace uPowers.Library {
    public class Rest {

        //public static int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
        //This should return points and minimum reputation for uses the actions on the site, to make it easier to
        //give correct user feedback...
        public static string ReputationPointsForJavascript() {   
            return "";
        }
        
        public static string Action(string alias, int pageID) {

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
            string comment = HttpContext.Current.Request["comment"] + "";

            if (_currentMember > 0) {
                BusinessLogic.Action a = new BusinessLogic.Action(alias);
                if (a != null) {
                    return a.Perform(_currentMember, pageID, comment).ToString();
                } else {
                    return "noAction";
                }
            }
        
            return "notLoggedIn";
        }
    }
}
