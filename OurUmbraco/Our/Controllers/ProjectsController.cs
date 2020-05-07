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
                MyLiveProjects = myProjects.Where(project => project.Live && !project.IsRetired),
                MyRetiredProjects = myProjects.Where(project => project.Live && project.IsRetired),
                MyDraftProjects = myProjects.Where(project => !project.Live),
                ContribLiveProjects = contribProjects.Where(project => project.Live && !project.IsRetired),
                ContribRetiredProjects = contribProjects.Where(project => project.Live && project.IsRetired),
                ContribDraftProjects = contribProjects.Where(project => !project.Live),
            };

            return PartialView("~/Views/Partials/Projects/MyProjects.cshtml", model);
        }
    }
}
