namespace OurUmbraco.Auth
{
    public class ProjectAuthConstants
    {
        public const string ProjectIdClaim = "http://our.umbraco/projects/projectid";
        public const string MemberIdClaim = "http://our.umbraco/projects/memberid";
        public const string BearerTokenClaimType = "http://our.umbraco/projects";
        public const string BearerTokenClaimValue = "yes";
        public const string BearerTokenAuthenticationType = "OurProjects";

        public const string ProjectIdHeader = "OurUmbraco-ProjectId";
        public const string MemberIdHeader = "OurUmbraco-MemberId";
    }
}
