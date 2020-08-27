using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.BlogPosts.Controllers.Http
{
    public class BlogController : UmbracoApiController
    {
        private readonly BlogPostsService _blogPostService;

        public BlogController()
        {
            _blogPostService = new BlogPostsService();
        }

        public IHttpActionResult GetAll(int take = 60, int numberOfPostsPerBlog = 4)
        {
            var posts = _blogPostService.GetCachedBlogPosts(take, numberOfPostsPerBlog);

            return Ok(posts);
        }
    }
}
