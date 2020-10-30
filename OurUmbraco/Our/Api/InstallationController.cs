using System.Web;
using System.Web.Http;
using OurUmbraco.Our.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class InstallationController : UmbracoApiController
    {
        [HttpPost]
        public IHttpActionResult Install(InstallationModel model)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));

            var hasError = !string.IsNullOrEmpty(model.Error);
            var ipAddress = HttpContext.Current.Request.UserHostAddress;
            var domainName = HttpContext.Current.Request.UserHostName;

            if (string.IsNullOrWhiteSpace(model.VersionComment) == false)
            {
                // V8 nightlies are reporting versions like alpha.58.1927 - which is too long,
                // the column in the database is only 10 chars. Abbreviating the version comment here will result in fewer 
                // errors logged in people's installs and we can see how many people actually 
                // try the nightlies
                if (model.VersionComment.Length > 10 && model.VersionComment.Contains("alpha"))
                {
                    // V8 nightlies all start with "alpha"
                    model.VersionComment = model.VersionComment.Replace("alpha", "a");
                }
                else
                {
                    if (model.VersionComment.Length > 0 && model.VersionComment.Length > 10)
                        // truncate to 10 chars, the max size
                        model.VersionComment = model.VersionComment.Substring(0, 10);
                }
            }

            if (!model.InstallCompleted)
            {
                using (var umbracoUpdateDb = new Database("umbracoUpdate"))
                {
                    var insertCmd = @"INSERT INTO [installs]
           ([completed]
           ,[isUpgrade]
           ,[errorLogged]
           ,[installId]
           ,[installStart]
           ,[versionMajor]
           ,[versionMinor]
           ,[versionPatch]
           ,[versionComment]
           ,[ip]
           ,[domain]
           ,[userAgent])
     VALUES
           (@completed
           ,@isUpgrade
           ,@errorLogged
           ,@installId
           ,@installStart
           ,@versionMajor
           ,@versionMinor
           ,@versionPatch
           ,@versionComment
           ,@ip
           ,@domain
           ,@userAgent)";

                    umbracoUpdateDb.Execute(insertCmd, new
                    {
                        completed = model.InstallCompleted,
                        isUpgrade = model.IsUpgrade,
                        errorLogged = hasError,
                        installId = model.InstallId,
                        installStart = model.Timestamp,
                        versionMajor = model.VersionMajor,
                        versionMinor = model.VersionMinor,
                        versionPatch = model.VersionPatch,
                        versionComment = model.VersionComment,
                        ip = ipAddress,
                        domain = domainName,
                        userAgent = model.UserAgent
                    });
                }
            }
            else
            {
                using (var umbracoUpdateDb = new Database("umbracoUpdate"))
                {
                    var updateCmd = @"UPDATE [installs]
           SET [completed] = 1, [installEnd] = @installEnd, [dbProvider] = @dbProvider 
            WHERE installId = @installId";

                    umbracoUpdateDb.Execute(updateCmd, new
                    {
                        installId = model.InstallId,
                        installEnd = model.Timestamp,
                        dbProvider = model.DbProvider
                    });
                }
            }

            return Ok();
        }
    }
}
