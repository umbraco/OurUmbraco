using System;
using System.Web.Mvc;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class TermsAndConditionsController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult RenderTerms(bool showBanner)
        {
            var model = new TermsAndConditionsModel { ShowTermsAndConditionsBanner = showBanner };
            return PartialView("~/Views/Partials/Community/TermsAndConditions.cshtml", model);
        }

        public ActionResult AcceptTerms()
        {
            var currentMember = Members.GetCurrentMember();
            if (currentMember != null)
            {
                var memberService = Services.MemberService;
                var member = memberService.GetById(currentMember.Id);
                member.SetValue("tos", DateTime.Now);
                memberService.Save(member);
                ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + member.Username);
            }

            return RedirectToCurrentUmbracoPage();
        }
    }
}
