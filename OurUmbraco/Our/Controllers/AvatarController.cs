using System.Web.Http;
using OurUmbraco.Community.People;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Controllers
{
    public class AvatarController : UmbracoApiController
    {
        [HttpGet]
        public string GetMemberAvatar(int memberId, int size = 75)
        {
            var member = Members.GetById(memberId);
            var avatarService = new AvatarService();
            return avatarService.GetImgWithSrcSet(member, member.Name, size);
        }
    }
}
