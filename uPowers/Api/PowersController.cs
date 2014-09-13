using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace uPowers.Api
{
    public class PowersController : UmbracoApiController
    {
        [HttpGet]
        public string Action(string alias, int pageId)
        {
            var comment = HttpContext.Current.Request["comment"] + "";

            if (Members.GetCurrentMember().Id > 0)
            {
                var action = new BusinessLogic.Action(alias);
                return action.Perform(Members.GetCurrentMember().Id, pageId, comment).ToString();
            }

            return "notLoggedIn";
        }
    }
}
