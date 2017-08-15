using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using OurUmbraco.Announcements.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Announcements
{
    public class AnnouncementApiController : UmbracoApiController
    {
        private const string CookieReadAnnouncementsKey = "announcementRead";
        public async Task<Models.LocationDisplayModel> GetLocation(string ip)
        {
            // Call the Ecom API to get the location
            var location = await EcomApiService.GetLocationAsync(ip);
            return location;
        }

        [HttpGet]
        public HttpResponseMessage MarkAnnouncementAsRead(Guid key)
        {
            var resp = Request.CreateResponse();
            var announcements = AnnouncementsService.GetPublishedAnnouncements();
            if (announcements.Any(x => x.Id == key && x.Permanent == false))
            {
                // the announcement exist and is not a permanent one - we set a read cookie
                var readAnnouncements = string.Empty;
                var cookieValue = string.Empty;

                CookieHeaderValue readCookie = Request.Headers.GetCookies(CookieReadAnnouncementsKey).FirstOrDefault();
                if (readCookie != null)
                {
                    readAnnouncements = readCookie[CookieReadAnnouncementsKey].Value;
                }

                // add to cookie if not already present
                if (readAnnouncements.Contains(key.ToString()) == false)
                {
                    cookieValue = readAnnouncements + key + ",";
                }

                var newCookie =
                    new CookieHeaderValue(CookieReadAnnouncementsKey,
                        cookieValue)
                    {
                        Domain = Request.RequestUri.Host,
                        Path = "/",
                        Expires = DateTimeOffset.Now.AddYears(10)
                    };
                resp.Headers.AddCookies(new CookieHeaderValue[] { newCookie });
            }

            return resp;
        }

        public async Task<HttpResponseMessage> GetAnnouncement(string area)
        {
            if (EcomApiService.IsReady())
            {
                bool updateCookie = false;

                // find the location first, check for cookie?
                var ip = HttpContext.Current.Request.UserHostAddress;
                Models.LocationDisplayModel location;
                CookieHeaderValue cookie = Request.Headers.GetCookies("our-location").FirstOrDefault();
                if (cookie != null)
                {
                    location = JsonConvert.DeserializeObject<Models.LocationDisplayModel>(cookie["our-location"].Value);
                }
                else
                {
                    location = await EcomApiService.GetLocationAsync(ip);
                    updateCookie = true;
                }

                // find read announcements (to filter them out)
                string readAnnouncements = string.Empty;
                CookieHeaderValue readCookie = Request.Headers.GetCookies(CookieReadAnnouncementsKey).FirstOrDefault();
                if (readCookie != null)
                {
                    readAnnouncements = readCookie[CookieReadAnnouncementsKey].Value;
                }


                var resp = Request.CreateResponse(
                    HttpStatusCode.OK,
                    AnnouncementsService.GetAnnouncement(area, location.Country, readAnnouncements)
                );

                // set a cookie?
                if (updateCookie)
                {
                    var locationCookie =
                        new CookieHeaderValue("our-location", JsonConvert.SerializeObject(location))
                        {
                            Expires = DateTimeOffset.Now.AddDays(1),
                            Domain = Request.RequestUri.Host,
                            Path = "/"
                        };
                    resp.Headers.AddCookies(new[] {locationCookie});
                }

                return resp;
            }
            else
            {
                var resp = Request.CreateResponse(HttpStatusCode.NotImplemented);
                return resp;
            }


        }

    }
}
