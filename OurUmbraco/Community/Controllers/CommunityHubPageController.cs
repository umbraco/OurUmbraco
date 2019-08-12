using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Community.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{
    public class CommunityHubPageController : RenderMvcController
    {
        public async Task<ActionResult> CommunityHubUProfileBlogPosts(RenderModel model)
        {
            var vm = new CommunityBlogPostViewModel(model.Content);

            var service = new BlogPostsService();

            var result = await service.GetUprofileBlogPosts();
            if (result != null && result.Any())
            {
                vm.Posts = result;
            }

            return CurrentTemplate(vm);
        }
    }
}
