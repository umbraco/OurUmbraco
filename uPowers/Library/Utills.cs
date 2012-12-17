using System;
using System.Collections.Generic;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace uPowers.Library {
    public class Utills {
        public static Member GetMember(int id) {
            Member m = Member.GetMemberFromCache(id);
            if (m == null)
                m = new Member(id);

            return m;
        }

        public static bool IsMemberInGroup(string GroupName, int memberid)
        {
            Member m = Utills.GetMember(memberid);
            foreach (MemberGroup mg in m.Groups.Values)
            {
                if (mg.Text == GroupName)
                    return true;
            }
            return false;
        }
    }
}
