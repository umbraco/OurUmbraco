using System.Linq;
using System.Web.Mvc;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ProjectsController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult RenderMyProjects()
        {
            var nodeListingProvider = new NodeListingProvider();
            var memberId = Members.GetCurrentMemberId();

            var myProjects = nodeListingProvider.GetListingsByVendor(memberId, false, true).OrderBy(x => x.Name);
            var contribProjects = nodeListingProvider.GetListingsForContributor(memberId).OrderBy(x => x.Name);
            var model = new MyProjectsModel
            {
                Projects = myProjects,
                ContribProjects = contribProjects
            };

            return PartialView("~/Views/Partials/Projects/MyProjects.cshtml", model);
        }
    }
}
