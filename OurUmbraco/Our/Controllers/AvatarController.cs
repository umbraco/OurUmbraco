using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Controllers
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
