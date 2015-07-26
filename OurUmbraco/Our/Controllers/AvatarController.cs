using System.Web.Http;
using OurUmbraco.Our;
using Umbraco.Web.WebApi;

namespace our.Controllers
{
    public class AvatarController : UmbracoApiController
    {
        [HttpGet]
        public string GetMemberAvatar(int memberId, int size = 75)
        {
            var member = Members.GetById(memberId);
            return Utils.GetMemberAvatar(member, size);
        }
    }
}
