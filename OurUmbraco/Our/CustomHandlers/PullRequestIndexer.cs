using System.Linq;
using OurUmbraco.Our.Examine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace OurUmbraco.Our.CustomHandlers
{
    public class PullRequestIndexer : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MemberService.Saved += MemberService_Saved;
        }

        private static void MemberService_Saved(IMemberService sender, Umbraco.Core.Events.SaveEventArgs<IMember> e)
        {
            foreach (var item in e.SavedEntities.Where(x => string.IsNullOrWhiteSpace(x.GetValue<string>("github")) == false))
                UpdatePullRequestExamineIndex(item.Id, item.GetValue<string>("github"));
        }
        
        private static void UpdatePullRequestExamineIndex(int memberId, string githubLogin)
        {
            var pullRequestIndexDataService = new PullRequestIndexDataService();
            pullRequestIndexDataService.UpdateIndex(memberId, githubLogin);
        }
    }
}
