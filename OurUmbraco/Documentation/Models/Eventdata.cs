namespace OurUmbraco.Documentation.Models
{
    public class Eventdata
    {
        public bool passed { get; set; }
        public bool failed { get; set; }
        public string status { get; set; }
        public string started { get; set; }
        public string finished { get; set; }
        public string duration { get; set; }
        public int projectId { get; set; }
        public string projectName { get; set; }
        public int buildId { get; set; }
        public int buildNumber { get; set; }
        public string buildVersion { get; set; }
        public string repositoryProvider { get; set; }
        public string repositoryScm { get; set; }
        public string repositoryName { get; set; }
        public string branch { get; set; }
        public string commitId { get; set; }
        public string commitAuthor { get; set; }
        public string commitAuthorEmail { get; set; }
        public string commitDate { get; set; }
        public string commitMessage { get; set; }
        public string committerName { get; set; }
        public string committerEmail { get; set; }
        public bool isPullRequest { get; set; }
        public int pullRequestId { get; set; }
        public string buildUrl { get; set; }
        public string notificationSettingsUrl { get; set; }
        public object[] messages { get; set; }
        public Job[] jobs { get; set; }
    }
}