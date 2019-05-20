using System.Net;
using Skybrud.Social.Http;

namespace OurUmbraco.Community.GitHub.Models.Comments
{

    /// <summary>
    /// Class representing the result for when a comment was made (or failed) to a GitHub issue.
    /// </summary>
    public class AddCommentResult
    {

        /// <summary>
        /// Gets whether adding the comment succeeded
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets a reference to the response from the GitHub API.
        /// </summary>
        public SocialHttpResponse Response { get; }

        public AddCommentResult(SocialHttpResponse response)
        {
            Response = response;
            Success = response.StatusCode == HttpStatusCode.Created;
        }

    }

}