using System.Linq;
using System.Web.Mvc;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.Our.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class EditProjectController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render(RenderModel model)
        {
            var nodeListingProvider = new NodeListingProvider();
            var memberId = Members.GetCurrentMemberId();
            var myProjects = nodeListingProvider.GetListingsByVendor(memberId, true, true);
            var contribProjects = nodeListingProvider.GetListingsForContributor(memberId);

            var projectEditModel = new EditProjectModel
            {
                HasAccess = myProjects.Any(x => x.Id == model.Content.Id) || contribProjects.Any(x => x.Id == model.Content.Id)
            };

            if (projectEditModel.HasAccess)
                projectEditModel.Content = model.Content;

            return PartialView("~/Views/Partials/Projects/EditProject.cshtml", projectEditModel);
        }
    }
}
