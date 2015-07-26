using System;
using System.Linq;
using OurUmbraco.MarketPlace.NodeListing;
using Umbraco.Web.UI.Controls;

namespace uProject.usercontrols.Deli.Profile
{
    public partial class MyProjects : UmbracoUserControl
    {
        public int edit { get; set; }
        public int forum { get; set; }
        public int licenses { get; set; }
        public int team { get; set; }

        protected string editUrl;
        protected string forumUrl;
        protected string licenseUrl;
        protected string teamUrl;

        protected void Page_Load(object sender, EventArgs e)
        {
            editUrl = umbraco.library.NiceUrl(edit);
            forumUrl = umbraco.library.NiceUrl(forum);
            licenseUrl = umbraco.library.NiceUrl(licenses);
            teamUrl = umbraco.library.NiceUrl(team);
            
            var nodeListingProvider = new NodeListingProvider();
            var memberId = Members.GetCurrentMemberId();

            var projects = nodeListingProvider.GetListingsByVendor(memberId, true, true).OrderBy(x=> x.Name);
            myProjects.DataSource = projects;
            myProjects.DataBind();

            var contribProjects = nodeListingProvider.GetListingsForContributor(memberId);
            myTeamProjects.DataSource = contribProjects;
            myTeamProjects.DataBind();
        }

        protected string GetIcon(string defaultIcon)
        {
            if (!string.IsNullOrEmpty(defaultIcon))
            {
                return defaultIcon;
            }
            return "/css/img/package2.png";
        }
    }
}