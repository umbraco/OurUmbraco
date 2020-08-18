using OurUmbraco.Community.People.Models;
using System.Collections.Generic;
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

        public List<MvpsPerYear> GetAll()
        {
            var mvps = _peopleSerivce.GetMvps();

            return mvps;
        }
    }
}
