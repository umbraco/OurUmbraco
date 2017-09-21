using System.Linq;
using OurUmbraco.Forum.Extensions;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;

namespace OurUmbraco.Powers.Library
{
    public class Utils
    {
        public static IPublishedContent GetMember(int id)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
            var member = memberShipHelper.GetById(id);
            return member;
        }

        public static bool IsMemberInGroup(string groupName, int memberid)
        {
            var member = GetMember(memberid);
            return member != null && member.GetRoles().Any(memberGroup => memberGroup == groupName);
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
