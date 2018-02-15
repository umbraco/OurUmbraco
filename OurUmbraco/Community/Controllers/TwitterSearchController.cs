using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.Twitter;
using OurUmbraco.Forum.Extensions;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{
    public class TwitterSearchController : SurfaceController
    {
        public ActionResult TwitterSearchResult(int numberOfResults = 6, bool adminOverview = false)
        {
            var twitterService = new TwitterService();
            var model = twitterService.GetTweets(numberOfResults, adminOverview);
            return PartialView("~/Views/Partials/Home/TwitterSearchUmbraco.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsSpam(string twitterHandle)
        {
            var redirectUrl = HttpContext.Request.UrlReferrer.AbsoluteUri;

            var member = Members.GetCurrentMember();
            var settingsNode = Umbraco.TypedContentAtRoot().FirstOrDefault();
            if (string.IsNullOrEmpty(twitterHandle) 
                || member == null
                || member.IsHq() == false 
                || ModelState.IsValid == false
                || settingsNode == null)
                return Redirect(redirectUrl);
            
            var contentService = ApplicationContext.Services.ContentService;
            var settingsContent = contentService.GetById(settingsNode.Id);
            var twitterFilterAccountsValue = settingsContent.GetValue<string>("twitterFilterAccounts");
            settingsContent.SetValue("twitterFilterAccounts", string.Format("{0},{1}", twitterFilterAccountsValue, twitterHandle));
            var publishStatus = contentService.PublishWithStatus(settingsContent);
            if (publishStatus.Exception == null)
                LogHelper.Info<TwitterSearchController>(string.Format("Twitter handle {0} marked as spam by {1} (id: {2})", twitterHandle, member.Name, member.Id));
            
            return Redirect(redirectUrl);
        }
    }
}
