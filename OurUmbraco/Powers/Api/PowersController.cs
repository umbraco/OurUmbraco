using System.Web;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Powers.Api
{
    public class PowersController : UmbracoApiController
    {
        [HttpGet]
        public string Action(string alias, int pageId)
        {
            var comment = HttpContext.Current.Request["comment"] + "";

            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                var action = new BusinessLogic.Action(alias);
                return action.Perform(currentMemberId, pageId, comment).ToString();
            }

            return "notLoggedIn";
        }
    }
}
