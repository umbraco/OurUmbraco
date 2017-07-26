using System.Collections.Generic;

namespace OurUmbraco.Community.GitHub.Models {
    
    /// <summary>
    /// Class wrapping the overall result of GitHub contributors. This is a class so we can add other properties than
    /// just the contributors - eg. a log for debugging purposes.
    /// </summary>
    public class GitHubContributorsResult
    {

        public List<GitHubGlobalContributorModel> Contributors { get; private set; }

        public string Log { get; private set; }

        public GitHubContributorsResult(List<GitHubGlobalContributorModel> contributors, string log = "")
        {
            Contributors = contributors;
            Log = log;
        }

    }

}