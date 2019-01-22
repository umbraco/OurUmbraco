using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.Community.Banner
{
    public class BannerService
    {
        public BannerService() { }

        public IEnumerable<Models.Banner> GetBannersByPage(IPublishedContent page)
        {
            var banners = page.GetPropertyValue<IEnumerable<IPublishedContent>>("banners");
            if (banners != null && banners.Any())
            {
                return banners.Select(x => new Models.Banner(x)
                {
                    Link = x.GetPropertyValue<string>("link"),
                    Image = x.GetPropertyValue<IPublishedContent>("image"),
                    All = x.GetPropertyValue<bool>("all"),
                    Continents = x.GetPropertyValue<IEnumerable<string>>("continents"),
                    Countries = x.GetPropertyValue<IEnumerable<string>>("countries")
                })
                .OrderByDescending(x => x.CreateDate);
            }

            return null;
        }
    }
}
