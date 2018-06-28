using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class MembersController : UmbracoAuthorizedApiController
    {
        //[System.Web.Http.HttpGet]
        //public IMember GetMemberGuid(int memberId)
        //{
        //    var memberService = ApplicationContext.Services.MemberService;
        //    var member = memberService.GetById(memberId);
        //    return member;
        //}

        [System.Web.Http.HttpGet]
        public List<IMember> SearchMembers(string searchTerm)
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var criteria = searcher.CreateSearchCriteria();
            var rawQuery = $"__NodeId:{searchTerm} OR nodeName:{searchTerm} OR _searchEmail:{searchTerm} OR company:{searchTerm} OR github:{searchTerm} OR twitter:{searchTerm}";
            var query = criteria.RawQuery(rawQuery);
            var results = searcher.Search(query);

            var members = new HashSet<IMember>();
            foreach (var searchResult in results)
            {
                var memberService = ApplicationContext.Services.MemberService;
                var member = memberService.GetById(searchResult.Id);
                members.Add(member);
            }
            return members.ToList();
        }

        [System.Web.Http.HttpGet]
        public bool AddContribBadgeToMember(int memberId)
        {
            var memberService = ApplicationContext.Services.MemberService;
            var member = memberService.GetById(memberId);
            if (member == null)
                return false;

            memberService.AssignRole(member.Id, "CoreContrib");
            return true;
        }
    }
}
