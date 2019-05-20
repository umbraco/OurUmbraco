namespace OurUmbraco.Our.Models.GitHub.AutoReplies
{

    public enum GitHubAutoReplyType
    {

        /// <summary>
        /// Indicates that a PR has merge conflicts 
        /// </summary>
        MergeConflict = 1,

        /// <summary>
        /// Indicates that a badge was added to the creator of the PR.
        /// </summary>
        ContributorsBadgeAdded = 2,

        /// <summary>
        /// Indicates that a creator of a PR wasn't found as a Our member.
        /// </summary>
        ContributorsBadgeMemberNotFound = 3,

        UpForGrabs = 4,

        AwaitingFeedback = 5,

        /// <summary>
        /// Indicates that an issue has been assigned a state/HqDiscussion label.
        /// </summary>
        HqDiscussion = 6

    }

}
