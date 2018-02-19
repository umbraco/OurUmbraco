using OurUmbraco.Forum.Extensions;
using Umbraco.Web;

namespace OurUmbraco.Our
{
    public class UpcomingFeaturesService
    {
        public bool MemberHasAccessToFeature()
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var member = umbracoHelper.MembershipHelper.GetCurrentMember();
            var allowed = member != null && member.IsAdmin();
            return allowed;
        }
    }
}
