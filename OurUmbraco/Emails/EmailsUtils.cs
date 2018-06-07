using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OurUmbraco.Emails
{
    public class EmailsUtils
    {
        public static class Mvc
        {
            public class WebFormController : Controller { }

            public static HtmlString RenderPartial(string partialName, object model)
            {
                // Get a wrapper for the legacy WebForm context
                var context = new HttpContextWrapper(HttpContext.Current);

                // Create a mock route that points to the empty controller
                var route = new RouteData();
                route.Values.Add("controller", "WebFormController");

                // Create a controller context for the route and http context
                var ctx = new ControllerContext(new RequestContext(context, route), new WebFormController());

                if (partialName.StartsWith("Partials/")) partialName = "~/Views/" + partialName;
                if (!partialName.EndsWith(".cshtml")) partialName += ".cshtml";

                // Find the partial view using the view engine
                var result = ViewEngines.Engines.FindPartialView(ctx, partialName);
                if (result?.View == null) throw new Exception($"Partial view \"{partialName}\" not found.");

                // Render the partial view to a StringBuilder
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    // Create a view context and assign the model
                    var vdd = new ViewDataDictionary { Model = model };
                    var vctx = new ViewContext(ctx, result.View, vdd, new TempDataDictionary(), writer);

                    // Render the partial view
                    result.View.Render(vctx, writer);
                }

                return new HtmlString(sb.ToString());
            }
        }
    }
}
