namespace uRelease.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string AuthorFullName { get; set; }
        public string IssueId { get; set; }
        public object ParentId { get; set; }
        public bool Deleted { get; set; }
        public object JiraId { get; set; }
        public string Text { get; set; }
        public bool ShownForIssueAuthor { get; set; }
        public long Created { get; set; }
        public long? Updated { get; set; }
        public object PermittedGroup { get; set; }
        public object[] Replies { get; set; }
    }
}