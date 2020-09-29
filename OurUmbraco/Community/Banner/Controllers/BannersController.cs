using OurUmbraco.Community.Banner.Models;
using OurUmbraco.Location;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Banner.Controllers
{
    public class BannersController : SurfaceController
    {
        private readonly BannerService _bannerService;
        private readonly LocationService _locationService;

        public BannersController()
        {
            _bannerService = new BannerService();
            _locationService = new LocationService();
        }

        public ActionResult Render(int id)
        {
            var page = Umbraco.TypedContent(id);
            if (page == null) return null;

            var banners = _bannerService.GetBannersByPage(page);
            if (banners == null) return null;

            var location = _locationService.GetLocationByIp(Request.UserHostAddress);

            var relevantBanners = banners.Where(x => x.All || x.Countries.InvariantContains(location?.Country) || x.Continents.InvariantContains(location?.Continent)).ToList();

            var vm = new BannersViewModel()
            {
                Collection = relevantBanners
            };

            return PartialView("Home/Banners", vm);
        }
    }
}
