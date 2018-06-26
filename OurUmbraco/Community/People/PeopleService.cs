using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using OurUmbraco.Community.People.Models;
using OurUmbraco.Our;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.Community.People
{
    public class PeopleService
    {
        public List<PeopleKarmaResult> GetMostActiveDateRange(DateTime fromDate, DateTime toDate, int numberOfResults = 25)
        {
            var sqlQuery = GetMostActiveDateRangeQuery(fromDate, toDate, numberOfResults);
            var results = ApplicationContext.Current.DatabaseContext.Database.Fetch<PeopleKarmaResult>(sqlQuery);
            return results;
        }

        private static string GetMostActiveDateRangeQuery(DateTime fromDate, DateTime toDate, int numberOfResults)
        {
            var where = string.Format("where (date BETWEEN '{0}' AND '{1}')", fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
            var top = string.Format("TOP {0}", numberOfResults);

            var query = string.Format(@";with score as(
                          SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints) as received
                          FROM [powersTopic]
                          {0}
                          group by receiverId
                          
                        UNION ALL
                        
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                        FROM [powersProject]
                        {0}
                        GROUP BY receiverId  
                        
                        UNION ALL
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                          FROM [powersComment]
                          {0}
                          GROUP BY receiverId
                       
                        UNION ALL
                         SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM [powersComment]
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersProject
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersTopic
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersWiki
                          {0}
                          GROUP BY memberId
                         )
                         
                         select {1} text as memberName, memberId, sum(performed) as performed, SUM(received) as received, (sum(received) + sum(performed)) as totalPointsInPeriod from score
                           inner join umbracoNode ON memberId = id  
                        where memberId IS NOT NULL and memberId > 0
                         group by text, memberId order by totalPointsInPeriod DESC", @where, top);

            return query;
        }

        public int? GetMemberIdFromGithubName(string githubName)
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();
            criteria.Field("github", githubName);
            var searchResults = searcher.Search(criteria);
            var searchResult = searchResults.FirstOrDefault();
            return searchResult?.Id;
        }

        public List<MvpsPerYear> GetMvps()
        {
            var memberService = UmbracoContext.Current.Application.Services.MemberService;
            var ourMvps = UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<List<MvpsPerYear>>("OurMvpsPerYear",
                () =>
                {
                    var memberGroupService = UmbracoContext.Current.Application.Services.MemberGroupService;
                    var allMemberGroups = memberGroupService.GetAll().Where(x => x.Name.StartsWith("MVP "));

                    var mvpsPerYear = new List<MvpsPerYear>();
                    foreach (var memberGroup in allMemberGroups)
                    {
                        var members = memberService.GetMembersByGroup(memberGroup.Name).ToList();

                        var yearString = memberGroup.Name.Split(' ')[1];
                        var category = memberGroup.Name.Replace($"{memberGroup.Name.Split(' ')[0]} {yearString}", string.Empty);

                        int.TryParse(yearString, out var year);
                        var yearExists = mvpsPerYear.FirstOrDefault(x => x.Year == year);
                        if (yearExists != null)
                        {
                            foreach (var member in members)
                            {
                                var mvpMember = PopulateMemberData(member, category);
                                yearExists.Members.Add(mvpMember);
                            }
                        }
                        else
                        {
                            var yearMembers = new MvpsPerYear
                            {
                                Year = year,
                                Members = new List<MvpMember>()
                            };

                            foreach (var member in members)
                            {
                                var mvpMember = PopulateMemberData(member, category);
                                yearMembers.Members.Add(mvpMember);

                            }
                            mvpsPerYear.Add(yearMembers);
                        }
                    }

                    return mvpsPerYear;

                }, TimeSpan.FromHours(48));
            
            return ourMvps;
        }

        public List<BadgeMember> GetMembersInRole(string role)
        {
            var currentApplication = UmbracoContext.Current.Application;
            var memberService = currentApplication.Services.MemberService;

            var ourMembersInRole = currentApplication.ApplicationCache.RuntimeCache.GetCacheItem<List<BadgeMember>>("OurMembersInRole" + role,
                () =>
                {
                    var memberGroupService = currentApplication.Services.MemberGroupService;
                    var memberGroup = memberGroupService
                        .GetAll()
                        .FirstOrDefault(x => string.Equals(x.Name, role, StringComparison.InvariantCultureIgnoreCase));
                    if (memberGroup == null)
                        return null;

                    var badgeMembers = new List<BadgeMember>();
                    var members = memberService.GetMembersByGroup(memberGroup.Name).ToList();
                    
                            foreach (var member in members)
                            {
                                var badgeMember = PopulateBadgeMemberData(member);
                                badgeMembers.Add(badgeMember);
                            }

                    return badgeMembers;

                }, TimeSpan.FromHours(24));
            
            return ourMembersInRole;
        }

        private static MvpMember PopulateMemberData(IMember member, string category)
        {
            var membershipHelper = new MembershipHelper(UmbracoContext.Current);
            var m = membershipHelper.GetById(member.Id);

            var company = member.GetValue<string>("company");
            var twitter = (member.GetValue<string>("twitter") ?? "").Trim().TrimStart('@');
            var github = (member.GetValue<string>("github") ?? "").Trim().TrimStart('@');

            var avatarService = new AvatarService();
            var avatarHtml = avatarService.GetImgWithSrcSet(m, m.Name, 48);

            var isMvpRenewal = false;
            if (category.Contains("Renewal"))
            {
                isMvpRenewal = true;
                category = category.Replace("Renewal", string.Empty);
            }

            var mvpMember = new MvpMember
            {
                Id = member.Id,
                Name = member.Name,
                Avatar = avatarHtml,
                Company = company,
                Twitter = twitter,
                GitHub = github,
                Category = category,
                IsMvpRenewal = isMvpRenewal
            };
            return mvpMember;
        }

        private static BadgeMember PopulateBadgeMemberData(IMember member)
        {
            var membershipHelper = new MembershipHelper(UmbracoContext.Current);
            var m = membershipHelper.GetById(member.Id);

            var company = member.GetValue<string>("company");
            var twitter = (member.GetValue<string>("twitter") ?? "").Trim().TrimStart('@');
            var github = (member.GetValue<string>("github") ?? "").Trim().TrimStart('@');

            var avatarService = new AvatarService();
            var avatarHtml = avatarService.GetImgWithSrcSet(m, m.Name, 48);

            var badgeMember = new BadgeMember
            {
                Id = member.Id,
                Name = member.Name,
                Avatar = avatarHtml,
                Company = company,
                Twitter = twitter,
                GitHub = github
            };

            return badgeMember;
        }
    }
}
