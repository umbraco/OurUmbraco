using System.Web.Http;
using Newtonsoft.Json;
using OurUmbraco.Our.Services;

namespace OurUmbraco.Our.Controllers
{
    public class ReleasesController : ApiController
    {
        [HttpGet]
        public string GetReleasesCache()
        {
            var releasesService = new ReleasesService();
            var releases = releasesService.GetReleasesCache();
            var releasesCache = JsonConvert.SerializeObject(releases, Formatting.None);
            return releasesCache;
        }
    }
}