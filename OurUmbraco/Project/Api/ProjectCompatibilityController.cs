using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using OurUmbraco.Project.Models;
using OurUmbraco.Project.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Project.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class ProjectCompatibilityController : UmbracoApiController
    {
        [HttpPost]
        public IEnumerable<VersionCompatibility> UpdateCompatibility(JObject model)
        {
            var projectId = model.Value<int>("projectId");
            var fileId = model.Value<int>("fileId");
            var report = model["report"].ToObject<Dictionary<string, bool>>();

            var cs = Services.ContentService;
            var project = cs.GetById(projectId);

            if (project == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var versionCompatService = new VersionCompatibilityService(DatabaseContext);

            versionCompatService.UpdateCompatibility(projectId, fileId, Members.GetCurrentMemberId(), report);

            return versionCompatService.GetCompatibilityReport(projectId);
        }
    }
}