using System.Linq;
using System.Web.Mvc;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class BreadcrumbController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render(bool linkToCurrent)
        {
            var model = CurrentPage.Ancestors()
                .Where(a => a.Level > 1 && a.GetPropertyValue<bool>("umbracoNaviHide") == false)
                .OrderBy(a => a.Level)
                .ToList();

            if(linkToCurrent) 
                model.Add(CurrentPage);

            return PartialView("~/Views/Partials/Global/Breadcrumb.cshtml", model);
        }
    }
}