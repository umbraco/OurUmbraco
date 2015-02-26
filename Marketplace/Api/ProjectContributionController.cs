using Marketplace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Umbraco.Web.WebApi;

namespace Marketplace.Api
{
    public class ProjectContributionController : UmbracoApiController
    {
        public HttpResponseMessage Delete(int projectId, int memberId)
        {
            using (var cs = new ContributionService())
            {
                cs.DeleteContributor(projectId, memberId);
            }

            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
