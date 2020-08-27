using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.Karma.Controllers.Http
{
    public class KarmaController : UmbracoApiController
    {
        private readonly KarmaService _karmaService;

        public KarmaController()
        {
            _karmaService = new KarmaService();
        }

        public IHttpActionResult GetKarmaStatistics()
        {
            var stats = _karmaService.GetCachedKarmaStatistics();

            return Ok(stats);
        }
    }
}
