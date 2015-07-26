using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Providers;
using OurUmbraco.MarketPlace.Interfaces;

namespace uProject.usercontrols.Deli.Profile
{
    public partial class MyProjects : System.Web.UI.UserControl
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


            var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            var member = memberProvider.GetCurrentMember();
            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];


            var projects = provider.GetListingsByVendor(member.Id, true, true).OrderBy(x=> x.Name);
            myProjects.DataSource = projects;
            myProjects.DataBind();

            var contribProjects = provider.GetListingsForContributor(member.Id);
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