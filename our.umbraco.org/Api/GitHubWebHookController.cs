using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Octokit;
using our.Attributes;
using uForum;
using uForum.Extensions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Web.WebApi;

namespace our.Api
{
    public class GitHubWebHookController : UmbracoApiController
    {
       
        [HttpGet]
        public HttpResponseMessage Ping()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Pong");
        }

        
        [VerifyGitHubWebHook]
        [HttpPost]
        public HttpResponseMessage ReceiveWebHook()
        {
            //From the webhook JSON payloud we get POSTed to us
            //Deserialize it to a object - take from Octokit GitHub's .NET API client

            var payloadString = Request.Content.ReadAsStringAsync().Result;

            //Map JSON string to object
            //var payload = JsonConvert.DeserializeObject<PullRequestEventPayload>(payloadString);
            var payload = new Octokit.Internal.SimpleJsonSerializer().Deserialize<PullRequestEventPayload>(payloadString);
            
            //Check the state, is it closed & merged = true
            //Means was accepeted & put back into repo
            //Thus assign contributor badge to memebr
            if (payload.PullRequest.State == ItemState.Closed && payload.PullRequest.Merged)
            {
                //First lets see if we can find a member with the username
                //Who done the commit/PR as a prop found on the member

                //MemberService to talk to the API
                var memberService = Services.MemberService;

                //Try & find a member with this github username
                //Should only ever be one (As when saving profile we should check that no one else has it set already)
                var gitHubMember = memberService.GetMembersByPropertyValue("github", payload.PullRequest.User.Login, StringPropertyMatchType.Exact).FirstOrDefault();

                //Check we definately found someone (again if there was more than one, we picked first)
                if (gitHubMember != null)
                {
                    //As we found this member in a succesfull PR back to the Umbraco core
                    //Assign the member to the contributor group
                    memberService.AssignRole(gitHubMember.Id, "CoreContrib");

                    //Get appSetting value for karma amount
                    var karmaToAward = Convert.ToInt32(ConfigurationManager.AppSettings["gitHubPullRequestKarma"]);

                    //Award karma to member
                    //TODO: DO I NEED TO GIVE SOME CONTEXT WHERE THIS WAS ASSIGNED FROM
                    //SO KARMA LOG/HISTORY FOR USER CAN BE TRACED BACK?!
                    gitHubMember.IncreaseKarma(karmaToAward);

                    //Save member so karma is persisted to member
                    memberService.Save(gitHubMember);
                }

            }

            //Always need to return 200 OK back to GitHub otherwise they will stop sending data to us
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
