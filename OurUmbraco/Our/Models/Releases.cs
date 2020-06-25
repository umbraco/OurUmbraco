using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OurUmbraco.Our.Models
{
    public class Release
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonIgnore]
        public System.Version FullVersion { get; set; }

        [JsonProperty("released")]
        public bool Released { get; set; }

        [JsonProperty("latestRelease")]
        public bool LatestRelease { get; set; }

        [JsonProperty("inProgressRelease")]
        public bool InProgressRelease { get; set; }

        [JsonProperty("isPatch")]
        public bool IsPatch { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("releaseStatus")]
        public object ReleaseStatus { get; set; }

        [JsonProperty("releaseDescription")]
        public string ReleaseDescription { get; set; }

        [JsonProperty("issues")]
        public List<Issue>Issues { get; set; }

        [JsonProperty("activities")]
        public object[] Activities { get; set; }

        [JsonProperty("currentRelease")]
        public bool CurrentRelease { get; set; }

        [JsonProperty("plannedRelease")]
        public bool PlannedRelease { get; set; }

        [JsonIgnore]
        public int FeatureCount { get; set; }

        [JsonIgnore]
        public int TotalIssueCount { get; set; }

        [JsonIgnore]
        public IEnumerable<Issue> IssuesCompleted { get; set; }

        [JsonProperty("percentComplete")]
        public double PercentComplete { get; set; }

        [JsonIgnore]
        public IEnumerable<Issue> Bugs { get; set; }

        [JsonIgnore]
        public IEnumerable<Issue> Features { get; set; }

        [JsonProperty("categorizedIssues")]
        public List<Category> CategorizedIssues { get; set; }
        
        public class Issue
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("breaking")]
            public bool Breaking { get; set; }

            [JsonProperty("source")]
            public ReleaseSource Source { get; set; }

            [JsonIgnore]
            public string Resolved { get; set; }
            
            [JsonProperty("contrib")]
            public bool CommunityContribution { get; set; }
        }
        
        public class Category
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonIgnore]
            public List<string> MatchingLabels { get; set; }

            [JsonProperty("priority")]
            public int Priority { get; set; }

            [JsonProperty("issues")]
            public List<Issue> Issues { get; set; }
        }
    }


    public enum ReleaseSource
    {
        YouTrack,
        GitHub
    }
}
