using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.People.Controllers.Http
{
    public class MvpController : UmbracoApiController
    {
        private readonly PeopleService _peopleSerivce;

        public MvpController()
        {
            _peopleSerivce = new PeopleService();
        }

        public IHttpActionResult GetAll()
        {
            var mvps = _peopleSerivce.GetMvps();

            return Ok(mvps);
        }
    }
}
