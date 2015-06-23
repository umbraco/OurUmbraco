using System;
using System.Collections.Generic;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace uPowers.Library
{
    public class Utils
    {
        public static Member GetMember(int id)
        {
            Member m = Member.GetMemberFromCache(id);
            if (m == null)
                m = new Member(id);

            return m;
        }

        public static bool IsMemberInGroup(string GroupName, int memberid)
        {
            Member m = Utils.GetMember(memberid);
            foreach (MemberGroup mg in m.Groups.Values)
            {
                if (mg.Text == GroupName)
                    return true;
            }
            return false;
        }

        public static bool HasVoted(int memberId, int id, string dataBaseTable)
        {
            return (BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT count(points) FROM " + dataBaseTable + " WHERE (id = @id) AND (memberId = @memberId)",
                BusinessLogic.Data.SqlHelper.CreateParameter("@id", id), BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0);
        }
    }
}
