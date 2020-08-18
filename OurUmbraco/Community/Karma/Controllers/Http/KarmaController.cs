using OurUmbraco.Community.Controllers;
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

        public KarmaStatistics GetKarmaStatistics()
        {
            var stats = _karmaService.GetCachedKarmaStatistics();

            return stats;
        }
    }
}
