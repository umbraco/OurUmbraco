using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using OurUmbraco.Release.Controllers;
using OurUmbraco.Release.Models;
using RestSharp;
using umbraco.NodeFactory;
using Umbraco.Core.Logging;
using YouTrackSharp.Infrastructure;

namespace OurUmbraco.Release
{
    public class Import
    {
        private const string VersionBundleUrl = "admin/customfield/versionBundle/Umbraco Versions";
        private const string IssuesUrl = "issue/byproject/{0}?filter={1}&max=200";

        private readonly string _projectId = ConfigurationManager.AppSettings["uReleaseProjectId"];

        private readonly string _login = ConfigurationManager.AppSettings["uReleaseUsername"];
        private readonly string _password = ConfigurationManager.AppSettings["uReleasePassword"];
        private readonly int _releasesPageNodeId = int.Parse(ConfigurationManager.AppSettings["uReleaseParentNodeId"]);
        public readonly string YouTrackJsonFile = "~/App_Data/YouTrack/all.json";

        public JsonResult VersionBundle(string ids, bool cached)
        {
            var idArray = new ArrayList();
            idArray.AddRange(ids.Replace("all", "").TrimEnd(',').Split(','));
            idArray.Remove("");

            // For each version in the bundle, go get the issues 
            var toReturn = new List<AggregateView>();

            Models.Version[] orderedVersions;

            if (cached == false)
            {
                var gotBundle = GetVersionBundle();

                orderedVersions = idArray.Count > 0
                    ? gotBundle.Data.Versions.Where(x => idArray.Contains(x.Value)).OrderBy(x => x.Value.AsFullVersion()).ToArray()
                    : gotBundle.Data.Versions.OrderBy(x => x.Value.AsFullVersion()).ToArray();
            }
            else
            {
                orderedVersions = ids.Split(',').Select(versionId => new Models.Version { Value = versionId }).ToArray();
            }

            var currentReleases = new List<Models.Version>();
            var plannedReleases = new List<Models.Version>();
            var inProgressReleases = new List<Models.Version>();

            var releasesNode = new Node(_releasesPageNodeId);

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
                        plannedReleases.Add(version);

                    if (release.GetPropertyValue("releaseStatus") != "Released")
                        inProgressReleases.Add(version);
                }
            }

            // Just used to make sure we don't make repeated API requests for keys
            var versionCache = new ConcurrentDictionary<string, RestResponse<IssuesWrapper>>();

            foreach (var version in orderedVersions)
            {
                var issueView = new List<IssueView>();

                var activityView = new List<ActivityView>();

                var item = new AggregateView();

                if (cached == false)
                {
                    item = new AggregateView
                    {
                        inProgressRelease = inProgressReleases.FirstOrDefault(x => x.Value == version.Value) != null,
                        version = version.Value,
                        isPatch = version.Value.AsFullVersion().Build != 0,
                        releaseDescription = version.Description ?? string.Empty,
                        releaseStatus = version.ReleaseStatus,
                        released = version.Released,
                        releaseDate = version.ReleaseDate == 0 ? "" : version.ReleaseDate.ConvertFromUnixDate().ToString(CultureInfo.InvariantCulture),
                        currentRelease = currentReleases.FirstOrDefault(x => x.Value == version.Value) != null,
                        plannedRelease = plannedReleases.FirstOrDefault(x => x.Value == version.Value) != null
                    };

                    var issuesList = new List<YouTrackIssue>();

                    var issuesRest = versionCache.GetOrAdd(version.Value,
                        key => GetRestResponse<IssuesWrapper>(string.Format(IssuesUrl, _projectId, "Due+in+version%3A+" + key), true));

                    var issues = JsonConvert.DeserializeObject<List<YouTrackIssue>>(issuesRest.Content);

                    if (issues.Any())
                        issuesList.AddRange(issues);

                    foreach (var issue in issuesList)
                    {
                        var view = new IssueView
                        {
                            id = issue.Id,
                            state = issue.GetFieldValue("State"),
                            title = issue.GetFieldValue("summary"),
                            type = issue.GetFieldValue("Type"),
                            breaking = (issue.GetFieldValue("Backwards compatible?") == "No")
                        };

                        issueView.Add(view);
                    }
                }
                else
                {
                    var cache = (List<AggregateView>)GetVersionsFromFile().Data;
                    var cachedVersion = cache.FirstOrDefault(x => x.version == version.Value);
                    if (cachedVersion != null)
                    {
                        item = cachedVersion;
                        issueView = cachedVersion.issues.ToList();
                    }
                }

                var activitiesDateDesc = activityView.Where(x => x.changes.Any()).OrderByDescending(x => x.date);
                var issueIdsFromActivities = activitiesDateDesc.Select(x => x.id).Distinct()
                    .Concat(issueView.Where(y => y != null && activitiesDateDesc.Select(z => z.id).Contains(y.id) == false)
                    .Select(y => y.id)); // Add issues for which there is no activity

                item.issues = new List<IssueView>(issueIdsFromActivities.Select(x => issueView.Single(y => y != null && y.id == x)).OrderBy(x => x.id));
                item.activities = activitiesDateDesc.Take(5).ToList();

                toReturn.Add(item);
            }

            return new JsonResult { Data = toReturn, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private RestResponse<VersionBundle> GetVersionBundle()
        {
            return GetRestResponse<VersionBundle>(VersionBundleUrl, false);
        }

        private RestResponse<T> GetRestResponse<T>(string restUri, bool requestJsonFormat)
            where T : new()
        {
            var ctor = new DefaultUriConstructor("http", "issues.umbraco.org", 80, "");

            var auth = new RestClient();
            auth.RemoveHandler("application/json");

            var req = new RestRequest(ctor.ConstructBaseUri("user/login"), Method.POST);
            req.AddParameter("login", _login);
            req.AddParameter("password", _password);

            var resp = auth.Execute(req);
            if (resp.StatusCode == HttpStatusCode.Forbidden)
                throw new Exception(resp.Content);

            var cookie = resp.Cookies.ToArray();

            var getVBundle = new RestRequest(ctor.ConstructBaseUri(restUri));
            foreach (var restResponseCookie in cookie)
                getVBundle.AddCookie(restResponseCookie.Name, restResponseCookie.Value);

            if (requestJsonFormat)
                getVBundle.AddHeader("Accept", "application/json");

            var gotBundle = auth.Execute<T>(getVBundle);
            return (RestResponse<T>)gotBundle;
        }
        
        public JsonResult GetVersionsFromFile()
        {
            var allText = File.ReadAllText(HttpContext.Current.Server.MapPath(YouTrackJsonFile));

            return new JsonResult { Data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        
        internal void SendYouTrackErrorMail()
        {
            var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];

            var body = "<p>Creating YouTrack cache has failed to get any data, there might be something wrong with YouTrack.</p>";

            var mailMessage = new MailMessage
            {
                Subject = "Our - Issue tracker cache creation failed",
                Body = body,
                IsBodyHtml = true
            };

            foreach (var email in notify.Split(','))
                mailMessage.To.Add(email);

            mailMessage.From = new MailAddress("our@umbraco.org");

            var smtpClient = new SmtpClient();
            smtpClient.Send(mailMessage);
        }
        
        public string SaveAllToFile()
        {
            try
            {
                var data = VersionBundle("all", false).Data;
                var typedData = (List<AggregateView>)data;
                if (typedData.Any())
                {
                    var result = new JavaScriptSerializer().Serialize(data);

                    using (var streamWriter = new StreamWriter(HostingEnvironment.MapPath(YouTrackJsonFile), false))
                    {
                        streamWriter.WriteLine(result);
                    }
                }
                else
                {

                    SendYouTrackErrorMail();
                    var message = string.Format("{0} There was no data to serialize so a new cache file hasn't been created", DateTime.Now);
                    LogHelper.Debug<Import>(message);
                    return message;
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<Import>("Error importing YouTrack issues", exception);
                return string.Format("{0} {1}", exception.Message, exception.StackTrace);
            }

            var succes = string.Format("Results succesfully written to {0} at {1}", HostingEnvironment.MapPath(YouTrackJsonFile), DateTime.Now);
            LogHelper.Debug<Import>(succes);
            return succes;
        }
    }
}