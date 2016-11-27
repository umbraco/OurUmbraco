using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;

namespace OurUmbraco.Powers.Library
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
            using (var sqlHelper = Application.SqlHelper)
            {
                return sqlHelper.ExecuteScalar<int>(
                           "SELECT count(points) FROM " + dataBaseTable +
                           " WHERE (id = @id) AND (memberId = @memberId)",
                           sqlHelper.CreateParameter("@id", id),
                           sqlHelper.CreateParameter("@memberId", memberId)) > 0;
            }
        }
    }
}
