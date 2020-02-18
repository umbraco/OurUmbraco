using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Newtonsoft.Json;
using OurUmbraco.Our.Models;
using OurUmbraco.Our.Services;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using static OurUmbraco.Our.Models.UpgradeResult;

namespace OurUmbraco.Our.Api
{
    public class UpgradeCheckController : UmbracoApiController
    {
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
                return Json(new UpgradeResult(UpgradeType.None, "", ""));

            // Persist the update check for our statistics, don't remove!
            var caller = HttpContext.Current.Request.UserHostName + "_" +
                         HttpContext.Current.Request.UserHostAddress;
            var callerHashed = FormsAuthentication.HashPasswordForStoringInConfigFile(caller, "sha1");
            PersistUpdateCheck(callerHashed, model.VersionMajor, model.VersionMinor, model.VersionPatch,
                model.VersionComment);
            // End of persisting the update check for our statistics, don't remove!

            // Special case for 4.0.4.2, apperently we never recommended them to upgrade
            if (version == new System.Version(4, 0, 4, 2) || version == latestVersion)
                return Json(new UpgradeResult(UpgradeType.None, "", ""));

            if (version.Major == 4)
            {
                // We had some special cases in the old updatechecker so I left them here
                if (version == new System.Version(4, 0, 4, 1))
                    return Json(new UpgradeResult(UpgradeType.Minor, "4.0.4.2 is out with fixes for load-balanced sites.", "http://our.umbraco.org/download"));

                if (version == new System.Version(4, 0, 4, 0))
                    return Json(new UpgradeResult(UpgradeType.Critical, "4.0.4.1 is out fixing a serious macro bug. Please upgrade!", "http://our.umbraco.org/download"));


                if (version == new System.Version(4, 7, 0, 0))
                    return Json(new UpgradeResult(UpgradeType.Critical, "This Umbraco installation needs to be upgraded. It may contain a potential security issue!", "http://our.umbraco.org/download"));


                if (version >= new System.Version(4, 10, 0, 0) && version < latestVersion)
                    return Json(new UpgradeResult(UpgradeType.Major, $"{latestVersion} is released. Upgrade today - it's free!", "http://our.umbraco.org/download"));
            }

            if (version.Major == 6)
            {
                if ((version < latestVersion))
                    return Json(new UpgradeResult(UpgradeType.Major, $"{latestVersion} is released. Upgrade today - it's free!", "http://our.umbraco.org/download"));
            }

            if (version.Major == 7 || version.Major == 8 || version.Major > 8)
            {
                if (version < latestVersion)
                    return Json(new UpgradeResult(UpgradeType.Minor, $"{latestVersion} is released. Upgrade today - it's free!", $"http://our.umbraco.org/contribute/releases/{latestVersion.Major}{latestVersion.Minor}{latestVersion.Build}"));
            }

            // If nothing matches then it's probably a nightly or a very old version, no need to send upgrade message
            return Json(new UpgradeResult(UpgradeType.None, "", ""));
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
