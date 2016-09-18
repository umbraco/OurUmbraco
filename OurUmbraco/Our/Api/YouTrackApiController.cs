using System.Net;
using System.Net.Http;
using OurUmbraco.Release;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class YouTrackApiController : UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetData()
        {
            var import = new Import();
            var result = import.SaveAllToFile();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
