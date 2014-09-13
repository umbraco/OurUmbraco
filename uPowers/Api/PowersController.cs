using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace uPowers.Api
{
    public class PowersController : UmbracoApiController
    {
        private static readonly MembershipHelper MemberShipHelper = new MembershipHelper(UmbracoContext.Current);
        private static readonly IPublishedContent CurrentMember = MemberShipHelper.GetCurrentMember();

        [HttpGet]
        public static string Action(string alias, int pageId)
        {
            var comment = HttpContext.Current.Request["comment"] + "";

            if (CurrentMember.Id > 0)
            {
                var action = new BusinessLogic.Action(alias);
                return action.Perform(CurrentMember.Id, pageId, comment).ToString();
            }

            return "notLoggedIn";
        }
    }
}
