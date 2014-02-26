using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RestSharp;
using uRelease.Models;
using YouTrackSharp.Infrastructure;
using umbraco.NodeFactory;
using Issue = uRelease.Models.Issue;
using System.Configuration;
using Version = uRelease.Models.Version;

namespace uRelease.Controllers
{
    public class ApiController : Controller
    {
        private const string VersionBundleUrl = "admin/customfield/versionBundle/Umbraco Versions";
        private const string IssuesUrl = "issue/byproject/{0}?filter={1}&max=200";

        private static readonly string ProjectId = ConfigurationManager.AppSettings["uReleaseProjectId"];

        private static readonly string Login = ConfigurationManager.AppSettings["uReleaseUsername"];
        private static readonly string Password = ConfigurationManager.AppSettings["uReleasePassword"];
        private static readonly int ReleasesPageNodeId = int.Parse(ConfigurationManager.AppSettings["uReleaseParentNodeId"]);
        private const string YouTrackJsonFile = "~/App_Data/YouTrack/all.json";

        public JsonResult Aggregate(string ids)
        {
            var idArray = new ArrayList();
            idArray.AddRange(ids.Replace("all", "").TrimEnd(',').Split(','));
            idArray.Remove("");

            var gotBundle = GetVersionBundle();

            // For each version in the bundle, go get the issues 
            var toReturn = new List<AggregateView>();

            Version[] orderedVersions;

            if (idArray.Count > 0)
            {
                //get version specific data
                orderedVersions =
                    gotBundle.Data.Versions
                    .Where(x => idArray.Contains(x.Value))
                    .OrderBy(x => x.Value.AsFullVersion())
                    .ToArray();
            }
            else
            {
                //get all versions with projectID
                orderedVersions =
                gotBundle.Data.Versions
                .OrderBy(x => x.Value.AsFullVersion())
                .ToArray();
            }

            var currentReleases = new List<Version>();
            var plannedReleases = new List<Version>();
            var inProgressReleases = new List<Version>();

            var releasesNode = new Node(ReleasesPageNodeId);

            foreach (Node release in releasesNode.Children)
            {
                var version = orderedVersions.SingleOrDefault(x => x.Value == release.Name);
                if (version != null)
                {
                    if (release.GetPropertyValue("recommendedRelease") == "1")
                    {
                        var status = release.GetProperty("releaseStatus");
                        if (status != null && status.Value != "Released")
                            version.ReleaseStatus = status.Value;

                        currentReleases.Add(version);
                    }


                    if (release.GetPropertyValue("releaseStatus") == "Planning")
                    {
                        plannedReleases.Add(version);
                    }

                    if (release.GetPropertyValue("releaseStatus") != "Released")
                        inProgressReleases.Add(version);
                }
            }

            // Just used to make sure we don't make repeated API requests for keys
            var versionCache = new ConcurrentDictionary<string, RestResponse<IssuesWrapper>>();

            foreach (var version in orderedVersions)
            {
                var item = new AggregateView
                               {
                                   inProgressRelease = inProgressReleases.FirstOrDefault(x => x.Value == version.Value) != null,
                                   version = version.Value,
                                   isPatch = version.Value.AsFullVersion().Build != 0,
                                   releaseDescription = version.Description ?? string.Empty,
                                   releaseStatus = version.ReleaseStatus,
                                   released = version.Released,
                                   releaseDate = version.ReleaseDate == 0 ? "" : ConvertDate(version.ReleaseDate).ToString(CultureInfo.InvariantCulture),
                                   currentRelease = currentReleases.FirstOrDefault(x => x.Value == version.Value) != null,
                                   plannedRelease = plannedReleases.FirstOrDefault(x => x.Value == version.Value) != null
                               };

                // /rest/issue/byproject/{project}?{filter}
                var issues = versionCache.GetOrAdd(version.Value, key => GetRestResponse<IssuesWrapper>(string.Format(IssuesUrl, ProjectId, "Due+in+version%3A+" + key)));
                var issueView = new List<IssueView>();
                var activityView = new List<ActivityView>();

                Parallel.ForEach(
                    issues.Data.Issues,
                    issue =>
                    {
                        var view = new IssueView
                        {
                            id = issue.Id,
                            state = GetFieldFromIssue(issue, "State"),
                            title = GetFieldFromIssue(issue, "summary"),
                            type = GetFieldFromIssue(issue, "Type"),
                            breaking = (GetFieldFromIssue(issue, "Backwards compatible?") == "No")
                        };
                        issueView.Add(view);
                    });

                var activitiesDateDesc = activityView.Where(x => x.changes.Any()).OrderByDescending(x => x.date);
                var issueIdsFromActivities = activitiesDateDesc.Select(x => x.id).Distinct()
                    .Concat(issueView.Where(y => y != null && activitiesDateDesc.Select(z => z.id).Contains(y.id) == false).Select(y => y.id)); // Add issues for which there is no activity

                item.issues = issueIdsFromActivities.Select(x => issueView.Single(y => y != null && y.id == x)).OrderBy(x => x.id);
                item.activities = activitiesDateDesc.Take(5);

                toReturn.Add(item);
            }

            return new JsonResult { Data = toReturn, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetAllFromFile()
        {
            if (System.IO.File.Exists(Server.MapPath(YouTrackJsonFile)) == false)
                SaveAllToFile();

            var allText = System.IO.File.ReadAllText(Server.MapPath(YouTrackJsonFile));

            return new JsonResult { Data = new JavaScriptSerializer().DeserializeObject(allText), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string SaveAllToFile()
        {
            try
            {
                var data = Aggregate("all").Data;
                var result = new JavaScriptSerializer().Serialize(data);
                using (var streamWriter = new StreamWriter(Server.MapPath(YouTrackJsonFile), false))
                {
                    streamWriter.WriteLine(result);
                }
            }
            catch (Exception exception)
            {
                return string.Format("{0} {1}", exception.Message, exception.StackTrace);
            }

            return string.Format("Results succesfully written to {0} at {1}", Server.MapPath(YouTrackJsonFile), DateTime.Now);
        }

        private static DateTime ConvertDate(long date)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(date);
        }

        private static string GetFieldFromIssue(Issue issue, string fieldName)
        {
            var findField = issue.Fields.FirstOrDefault(x => x.Name == fieldName);
            return findField != null ? findField.Value : string.Empty;
        }

        public JsonResult AllVersions()
        {
            var gotBundle = GetVersionBundle();
            return new JsonResult { Data = gotBundle.Data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static RestResponse<VersionBundle> GetVersionBundle()
        {
            return GetRestResponse<VersionBundle>(VersionBundleUrl);
        }

        private static RestResponse<T> GetRestResponse<T>(string restUri)
            where T : new()
        {
            var ctor = new DefaultUriConstructor("http", "issues.umbraco.org", 80, "");

            var auth = new RestClient();
            auth.RemoveHandler("application/json");

            var req = new RestRequest(ctor.ConstructBaseUri("user/login"), Method.POST);
            req.AddParameter("login", Login);
            req.AddParameter("password", Password);

            var resp = auth.Execute(req);
            var cookie = resp.Cookies.ToArray();

            var getVBundle = new RestRequest(ctor.ConstructBaseUri(restUri));
            foreach (var restResponseCookie in cookie)
                getVBundle.AddCookie(restResponseCookie.Name, restResponseCookie.Value);

            var gotBundle = auth.Execute<T>(getVBundle);
            return (RestResponse<T>)gotBundle;
        }
    }
}
