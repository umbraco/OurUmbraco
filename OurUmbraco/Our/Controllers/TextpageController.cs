using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class TextpageController : RenderMvcController
    {
        public ActionResult EditProject(int id)
        {
            var projectContent = Umbraco.TypedContent(id);
            return CurrentTemplate(projectContent);
        }
    }
}