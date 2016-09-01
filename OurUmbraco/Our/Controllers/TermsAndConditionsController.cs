using System;
using System.Web.Mvc;
using OurUmbraco.Our.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class TermsAndConditionsController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult RenderTerms()
        {
            var model = new TermsAndConditionsModel();
            var currentMember = Members.GetCurrentMember();
            if (currentMember != null)
            {
                var tosAccepted = currentMember.GetPropertyValue<DateTime>("tos");
                var newTosDate = new DateTime(2016, 09, 01);
                if ((newTosDate - tosAccepted).TotalDays > 1)
                {
                    model.ShowTermsAndConditionsBanner = true;
                }
            }

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
            }

            return RedirectToCurrentUmbracoPage();
        }
    }
}
