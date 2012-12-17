using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace uRelease.Controllers
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Web.UI;
    using RestSharp;
    using RestSharp.Deserializers;
    using uRelease.Models;
    using YouTrackSharp.Infrastructure;
    using YouTrackSharp.Issues;
    using YouTrackSharp.Projects;
    using Issue = uRelease.Models.Issue;
using System.Configuration;

    public class ApiController : Controller
    {
        private static string VersionBundleUrl = "admin/customfield/versionBundle/Umbraco v4 Versions";
        private static string IssuesUrl = "issue/byproject/{0}?filter={1}&max=100";

        private static string ProjectId = ConfigurationManager.AppSettings["uReleaseProjectId"];

        private static string login = ConfigurationManager.AppSettings["uReleaseUsername"];
        private static string password = ConfigurationManager.AppSettings["uReleasePassword"];


        [OutputCache(Duration = 30, Location = OutputCacheLocation.ServerAndClient)]
        public JsonResult Aggregate(string ids)
        {

             ArrayList idArray = new ArrayList();
            idArray.AddRange(ids.Replace("all","").TrimEnd(',').Split(','));
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
            

            //figure out which is the latest release
            var latestRelease = orderedVersions.Where(x => x.Released).OrderByDescending(x => x.ReleaseDate).FirstOrDefault();
            var inprogressRelease = orderedVersions.Where(x => !x.Released).OrderBy(x => x.ReleaseDate).FirstOrDefault();

            // Just used to make sure we don't make repeated API requests for keys
            var versionCache = new ConcurrentDictionary<string, RestResponse<IssuesWrapper>>();
            var changesCache = new ConcurrentDictionary<string, RestResponse<Changes>>();

            foreach (var version in orderedVersions)
            {
                var item = new AggregateView();
                item.latestRelease = (latestRelease != null && version.Value == latestRelease.Value);
                item.inProgressRelease = (inprogressRelease != null && version.Value == inprogressRelease.Value);
                item.version = version.Value;
                item.releaseDescription = version.Description ?? string.Empty;
                item.released = version.Released;
                item.releaseDate = new DateTime(1970,1,1).AddMilliseconds(version.ReleaseDate).ToString();
                // /rest/issue/byproject/{project}?{filter}
                var issues = versionCache.GetOrAdd(version.Value, key => GetResponse<IssuesWrapper>(string.Format(IssuesUrl, ProjectId, "Due+in+version%3A+" + key)));
                var issueView = new List<IssueView>();
                var activityView = new List<ActivityView>();

                Parallel.ForEach(
                    issues.Data.Issues,
                    issue =>
                    {
                        var changes = changesCache.GetOrAdd(issue.Id, key => GetResponse<Changes>("/issue/" + key + "/changes"));

                        var allChanges = changes.Data;

                        foreach (var allChange in allChanges)
                        {
                            var updaterName = GetChanges(allChange.Fields, "updaterName");
                            var updaterDate = GetChanges(allChange.Fields, "updated");
                            long updaterDateLength = 0;
                            var hasDate = long.TryParse(updaterDate, out updaterDateLength);

                            var allUsableChanges = allChange.Fields.Where(x => x.NewValue != null);

                            var activity = new ActivityView()
                            {
                                id = issue.Id,
                                username = updaterName,
                                date = updaterDateLength,
                                changes =
                                    new List<ChangeView>(
                                    allUsableChanges.Where(x => x.Name != "numberInProject").Select(
                                        x =>
                                        new ChangeView() { fieldName = x.Name, newValue = x.NewValue, oldValue = x.OldValue }))
                            };
                            activityView.Add(activity);
                        }



                        var view = new IssueView()
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
                    .Concat(issueView.Where(y => !activitiesDateDesc.Select(z => z.id).Contains(y.id)).Select(y => y.id)); // Add issues for which there is no activity

                item.issues = issueIdsFromActivities.Select(x => issueView.Single(y => y.id == x)).OrderBy(x => x.id);
                item.activities = activitiesDateDesc.Take(5);


                toReturn.Add(item);
            }

            return new JsonResult() { Data = toReturn, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static string GetChanges(IEnumerable<Change.Field> allChanges, string fieldName)
        {
            var firstOrDefault = allChanges.FirstOrDefault(x => x.Name == fieldName);
            return firstOrDefault != null ? firstOrDefault.Value : string.Empty;
        }

        private static string GetFieldFromIssue(Issue issue, string fieldName)
        {
            var findField = issue.Fields.FirstOrDefault(x => x.Name == fieldName);
            return findField != null ? findField.Value : string.Empty;
        }

        public JsonResult AllVersions()
        {
            var gotBundle = GetVersionBundle();
            return new JsonResult() { Data = gotBundle.Data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static RestResponse<VersionBundle> GetVersionBundle()
        {
            return GetResponse<VersionBundle>(VersionBundleUrl);
        }

        private static RestResponse<T> GetResponse<T>(string restUri)
            where T : new()
        {
            var ctor = new DefaultUriConstructor("http", "issues.umbraco.org", 80, "");

            var auth = new RestSharp.RestClient();
            auth.RemoveHandler("application/json");

            var req = new RestSharp.RestRequest(ctor.ConstructBaseUri("user/login"), Method.POST);
            req.AddParameter("login", login);
            req.AddParameter("password", password);

            var resp = auth.Execute(req);
            var cookie = resp.Cookies.ToArray();

            var getVBundle = new RestSharp.RestRequest(ctor.ConstructBaseUri(restUri));
            foreach (var restResponseCookie in cookie)
            {
                getVBundle.AddCookie(restResponseCookie.Name, restResponseCookie.Value);
            }
            var gotBundle = auth.Execute<T>(getVBundle);
            return (RestResponse<T>) gotBundle;
        }
    }
}
