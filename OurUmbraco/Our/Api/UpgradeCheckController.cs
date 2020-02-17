using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Newtonsoft.Json;
using OurUmbraco.Our.Models;
using OurUmbraco.Our.Services;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class UpgradeCheckController : UmbracoApiController
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

        [HttpPost]
        public IHttpActionResult CheckUpgrade(UpgradeModel model)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));

            int revision;
            int.TryParse(model.VersionComment, out revision);
            var version = new System.Version(model.VersionMajor, model.VersionMinor, model.VersionPatch, revision);

            var latestVersion = GetLatestVersionFromMajor(model.VersionMajor);
            if (latestVersion == null)
                return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.None, "", "")));

            // Persist the update check for our statistics, don't remove!
            var caller = HttpContext.Current.Request.UserHostName + "_" +
                         HttpContext.Current.Request.UserHostAddress;
            var callerHashed = FormsAuthentication.HashPasswordForStoringInConfigFile(caller, "sha1");
            PersistUpdateCheck(callerHashed, model.VersionMajor, model.VersionMinor, model.VersionPatch,
                model.VersionComment);
            // End of persisting the update check for our statistics, don't remove!

            // Special case for 4.0.4.2, apperently we never recommended them to upgrade
            if (version == new System.Version(4, 0, 4, 2) || version == latestVersion)
                return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.None, "", "")));

            if (version.Major == 4)
            {
                // We had some special cases in the old updatechecker so I left them here
                if (version == new System.Version(4, 0, 4, 1))
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Minor, "4.0.4.2 is out with fixes for load-balanced sites.", "http://our.umbraco.org/download")));

                if (version == new System.Version(4, 0, 4, 0))
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Critical, "4.0.4.1 is out fixing a serious macro bug. Please upgrade!", "http://our.umbraco.org/download")));


                if (version == new System.Version(4, 7, 0, 0))
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Critical, "This Umbraco installation needs to be upgraded. It may contain a potential security issue!", "http://our.umbraco.org/download")));


                if (version >= new System.Version(4, 10, 0, 0) && version < latestVersion)
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Major, $"{latestVersion} is released. Upgrade today - it's free!", "http://our.umbraco.org/download")));
            }

            if (version.Major == 6)
            {
                if ((version < latestVersion))
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Major, $"{latestVersion} is released. Upgrade today - it's free!", "http://our.umbraco.org/download")));
            }

            if (version.Major == 7 || version.Major == 8 || version.Major > 8)
            {
                if (version < latestVersion)
                    return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.Minor, $"{latestVersion} is released. Upgrade today - it's free!", $"http://our.umbraco.org/contribute/releases/{latestVersion.Major}{latestVersion.Minor}{latestVersion.Build}")));
            }

            // If nothing matches then it's probably a nightly or a very old version, no need to send upgrade message
            return Ok(JsonConvert.SerializeObject(new UpgradeResultModel(UpgradeType.None, "", "")));
        }

        private void PersistUpdateCheck(string server, int majorVersion, int minorVersion, int patchVersion, string commentVersion)
        {
            if (commentVersion.Length > 49)
            {
                commentVersion = commentVersion.Substring(0, 47) + "...";
            }

            using (var umbracoUpdateDb = new Database("umbracoUpdate"))
            {
                var insertCmd = @"EXEC [UpdatePing]
           @pinger, @major, @minor, @patch, @comment";

                umbracoUpdateDb.Execute(insertCmd, new
                {
                    pinger = server,
                    major = majorVersion,
                    minor = minorVersion,
                    patch = patchVersion,
                    comment = commentVersion
                });
            }
        }

        private System.Version GetLatestVersionFromMajor(int major)
        {
            var releasesService = new ReleasesService();
            var releases = releasesService.GetReleasesCache().Where(x => x.Released).OrderByDescending(x => x.FullVersion).ToList();
            var majorVersionGroups = releases.GroupBy(x => x.FullVersion.Major).OrderByDescending(x => x.Key);

            foreach (var majorVersion in majorVersionGroups)
            {
                if (majorVersion.Key == major)
                {
                    var releasesInGroup = releases.Where(x => x.FullVersion.Major == majorVersion.Key).ToList();
                    var newestReleaseInGroup = releasesInGroup.First().FullVersion;
                    var revision = (newestReleaseInGroup.Revision <= 0) ? 0 : newestReleaseInGroup.Revision;

                    return new System.Version(newestReleaseInGroup.Major, newestReleaseInGroup.Minor,
                        newestReleaseInGroup.Build, revision);
                }
            }

            return null;
        }
    }
}
