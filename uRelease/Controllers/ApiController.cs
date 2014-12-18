using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using RestSharp;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using uRelease.Models;
using YouTrackSharp.Infrastructure;
using umbraco.NodeFactory;
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

        public JsonResult VersionBundle(string ids, bool cached)
        {
            var idArray = new ArrayList();
            idArray.AddRange(ids.Replace("all", "").TrimEnd(',').Split(','));
            idArray.Remove("");

            // For each version in the bundle, go get the issues 
            var toReturn = new List<AggregateView>();

            Version[] orderedVersions;

            if (cached == false)
            {
                var gotBundle = GetVersionBundle();

                orderedVersions = idArray.Count > 0
                    ? gotBundle.Data.Versions.Where(x => idArray.Contains(x.Value)).OrderBy(x => x.Value.AsFullVersion()).ToArray()
                    : gotBundle.Data.Versions.OrderBy(x => x.Value.AsFullVersion()).ToArray();
            }
            else
            {
                orderedVersions = ids.Split(',').Select(versionId => new Version { Value = versionId }).ToArray();
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
                        plannedReleases.Add(version);

                    if (release.GetPropertyValue("releaseStatus") != "Released")
                        inProgressReleases.Add(version);
                }
            }

            // Just used to make sure we don't make repeated API requests for keys
            var versionCache = new ConcurrentDictionary<string, RestResponse<IssuesWrapper>>();

            // Make sure all versions exist on Our 
            foreach (var orderedVersion in orderedVersions.Where(v => v.Value.AsFullVersion() > "6.0.0".AsFullVersion()))
            {
                var versionExistsInUmbraco = releasesNode.Children.Cast<Node>().Any(child => child.Name == orderedVersion.Value);

                if (versionExistsInUmbraco) 
                    continue;

                //make it
                var releaseDocumentType = DocumentType.GetByAlias("Release");
                var document = Document.MakeNew(orderedVersion.Value, releaseDocumentType, new User(0), releasesNode.Id);
                document.getProperty("bodyText").Value = string.Format("<p>{0}</p>", orderedVersion.Description);
                document.Publish(new User(0));
                library.UpdateDocumentCache(document.Id);
            }

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
                        releaseDate = version.ReleaseDate == 0 ? "" : ConvertDate(version.ReleaseDate).ToString(CultureInfo.InvariantCulture),
                        currentRelease = currentReleases.FirstOrDefault(x => x.Value == version.Value) != null,
                        plannedRelease = plannedReleases.FirstOrDefault(x => x.Value == version.Value) != null
                    };

                    var issuesList = new List<Issue>();

                    var issuesRest = versionCache.GetOrAdd(version.Value,
                        key => GetRestResponse<IssuesWrapper>(string.Format(IssuesUrl, ProjectId, "Due+in+version%3A+" + key), true));

                    var issues = JsonConvert.DeserializeObject<List<Issue>>(issuesRest.Content);

                    if (issues.Any())
                        issuesList.AddRange(issues);

                    Parallel.ForEach(
                        issuesList,
                        issue =>
                        {
                            var view = new IssueView
                                       {
                                           id = issue.id,
                                           state = GetFieldFromIssue(issue, "State"),
                                           title = GetFieldFromIssue(issue, "summary"),
                                           type = GetFieldFromIssue(issue, "Type"),
                                           breaking = (GetFieldFromIssue(issue, "Backwards compatible?") == "No")
                                       };

                            issueView.Add(view);
                        });
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

        public JsonResult GetAllFromFile()
        {
            if (System.IO.File.Exists(Server.MapPath(YouTrackJsonFile)) == false)
                SaveAllToFile();

            var allText = System.IO.File.ReadAllText(Server.MapPath(YouTrackJsonFile));

            return new JsonResult { Data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetVersionsFromFile()
        {
            var allText = System.IO.File.ReadAllText(Server.MapPath(YouTrackJsonFile));

            return new JsonResult { Data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
                    
                    using (var streamWriter = new StreamWriter(Server.MapPath(YouTrackJsonFile), false))
                    {
                        streamWriter.WriteLine(result);
                    }
                }
                else
                {
                    SendYouTrackErrorMail();
                    return string.Format("{0} There was no data to serialize so a new cache file hasn't been created", DateTime.Now);
                }
            }
            catch (Exception exception)
            {
                return string.Format("{0} {1}", exception.Message, exception.StackTrace);
            }

            return string.Format("Results succesfully written to {0} at {1}", Server.MapPath(YouTrackJsonFile), DateTime.Now);
        }

        private static void SendYouTrackErrorMail()
        {
            var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];

            var body = string.Format("<p>Creating YouTrack cache has failed to get any data, there might be something wrong with YouTrack.</p>");

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

        private static DateTime ConvertDate(long date)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(date);
        }

        private static string GetFieldFromIssue(Issue issue, string fieldName)
        {
            var findField = issue.field.FirstOrDefault(x => x.name == fieldName);
            return findField != null ? findField.value.ToString().Replace("[", string.Empty).Replace("]", string.Empty).Replace("\"", string.Empty).Trim() : string.Empty;
        }

        public JsonResult AllVersions()
        {
            var gotBundle = GetVersionBundle();
            return new JsonResult { Data = gotBundle.Data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static RestResponse<VersionBundle> GetVersionBundle()
        {
            return GetRestResponse<VersionBundle>(VersionBundleUrl, false);
        }

        private static RestResponse<T> GetRestResponse<T>(string restUri, bool requestJsonFormat)
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

            if (requestJsonFormat)
                getVBundle.AddHeader("Accept", "application/json");

            var gotBundle = auth.Execute<T>(getVBundle);
            return (RestResponse<T>)gotBundle;
        }
    }
    
    public class Issue
    {
        public string id { get; set; }
        public object jiraId { get; set; }
        public List<Field> field { get; set; }
        public List<Comment> comment { get; set; }
        public object[] tag { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public object value { get; set; }
    }

    public class Comment
    {
        public string id { get; set; }
        public string author { get; set; }
        public string authorFullName { get; set; }
        public string issueId { get; set; }
        public object parentId { get; set; }
        public bool deleted { get; set; }
        public object jiraId { get; set; }
        public string text { get; set; }
        public bool shownForIssueAuthor { get; set; }
        public long created { get; set; }
        public long? updated { get; set; }
        public object permittedGroup { get; set; }
        public object[] replies { get; set; }
    }
}
