using System;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
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
            // Database no longer exists, skip
            return Ok();

            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));

            var hasError = string.IsNullOrEmpty(model.Error) == false;

            var request = HttpContext.Current.Request;
            var ipAddress = request.UserHostAddress;
            var forwardedFor = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(forwardedFor) == false)
                ipAddress = forwardedFor;
            var domainName = request.UserHostName;

            // CloudFlare identifies the country for us
            var country = request.Params["HTTP_CF_IPCOUNTRY"];

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

            if (hasError)
            {
                // insert error

                using (var umbracoUpdateDb = new Database("umbracoUpdate"))
                {
                    var cmd = @"INSERT INTO [installError]
       ([installId]
       ,[timestamp]
       ,[error])
 VALUES
       (@installId
       ,@timestamp
       ,@error)";

                    var args = new
                    {
                        installId = model.InstallId,
                        installEnd = DateTime.Now,
                        dbProvider = model.DbProvider,
                        error = model.Error
                    };
                    
                    try
                    {
                        umbracoUpdateDb.Execute(cmd, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(InstallationController),$"Couldn't log installer error ping, SQL insert failed, values: {JsonConvert.SerializeObject(args)}", ex);
                    }
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
       ,[userAgent]
       ,[country])
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
       ,@userAgent
       ,@country)";

                    var args = new
                    {
                        completed = model.InstallCompleted,
                        isUpgrade = model.IsUpgrade,
                        errorLogged = hasError,
                        installId = model.InstallId,
                        installStart = DateTime.Now,
                        versionMajor = model.VersionMajor,
                        versionMinor = model.VersionMinor,
                        versionPatch = model.VersionPatch,
                        versionComment = model.VersionComment,
                        ip = ipAddress,
                        domain = domainName,
                        userAgent = model.UserAgent,
                        country = country
                    };
                    try
                    {
                        umbracoUpdateDb.Execute(insertCmd, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(InstallationController),$"Couldn't log installer ping, SQL insert failed, values: {JsonConvert.SerializeObject(args)}", ex);
                    }
                }
            }
            else
            {
                using (var umbracoUpdateDb = new Database("umbracoUpdate"))
                {
                    var updateCmd = @"UPDATE [installs]
       SET [completed] = 1, [installEnd] = @installEnd, [dbProvider] = @dbProvider, [country] = @country 
        WHERE installId = @installId";

                    var args = new
                    {
                        installId = model.InstallId,
                        installEnd = DateTime.Now,
                        dbProvider = model.DbProvider,
                        country = country
                    };
                    try
                    {
                        umbracoUpdateDb.Execute(updateCmd, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(InstallationController),$"Couldn't log installer update ping, SQL insert failed, values: {JsonConvert.SerializeObject(args)}", ex);
                    }
                    
                }
            }


            return Ok();
        }
    }
}
