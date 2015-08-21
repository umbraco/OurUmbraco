using System.Collections.Generic;

namespace OurUmbraco.Release.Models
{
    public class YouTrackIssue
    {
        public string Id { get; set; }
        public object JiraId { get; set; }
        public List<YouTrackField> Field { get; set; }
        public List<YouTrackComment> Comment { get; set; }
        public object[] Tag { get; set; }
    }
}