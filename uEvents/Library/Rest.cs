using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.relation;
using System.Web.Security;
using System.Web;

namespace uEvents.Library
{
    public class Rest
    {
        public static string Toggle(int eventId)
        {  
            Event e = new Event(eventId);
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                if (!Relation.IsRelated(eventId, _currentMember))
                    e.SignUp(_currentMember, "no comment");
                else
                    e.Cancel(_currentMember, "no comment");
            }

            return "true";
        }

        public static string Sync(int eventId)
        {
                Event e = new Event(eventId);
                e.syncCapacity();
                return "true";
        }

        public static string SignUp(int eventId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                Event e = new Event(eventId);
                e.SignUp(_currentMember, "no comment");
            }
            return "true";
        }

        public static string Cancel(int eventId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
            Event e = new Event(eventId);
            e.Cancel(_currentMember, "no comment");
            }
            return "true";
        }

        public static XPathNodeIterator UpcomingEvents()
        {
             object eventCacheSyncLock = new object();
             return Cache.GetCacheItem<XPathNodeIterator>("ourEvents", eventCacheSyncLock, new TimeSpan(1,0,0),
                 delegate
                 {
                     return Rest.UpcomingEvents();
                 });
        }
    }
}
