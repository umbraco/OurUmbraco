using Umbraco.Core.Persistence;

namespace OurUmbraco.Forum.Models
{
    public class SimpleMember
    {
        [Column("id")]
        public int CommentId { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }

        [Column("memberName")]
        public string MemberName { get; set; }
    }
}