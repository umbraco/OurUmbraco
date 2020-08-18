namespace OurUmbraco.Community.People.Models
{
    public class PeopleKarmaResult
    {
        public string MemberName { get; set; }
        public string MemberId { get; set; }
        public string MemberAvatarUrl { get; set; }
        public int Performed { get; set; }
        public int Received { get; set; }
        public int TotalPointsInPeriod { get; set; }
        public int TotalKarma { get; set; }
    }
}
