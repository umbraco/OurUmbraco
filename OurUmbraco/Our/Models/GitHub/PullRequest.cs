using Newtonsoft.Json.Linq;

namespace OurUmbraco.Our.Models.GitHub
{

    public class PullRequest : Issue
    {

        public PullRequest(JObject obj) : base(obj)
        {


        }

        public new static PullRequest Parse(JObject obj)
        {
            return obj == null ? null : new PullRequest(obj);
        }

    }

}