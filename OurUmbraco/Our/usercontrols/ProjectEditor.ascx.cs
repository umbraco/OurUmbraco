using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using OurUmbraco.Our.Api;
using OurUmbraco.Repository;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.presentation.nodeFactory;

namespace OurUmbraco.Our.usercontrols
{
    public partial class ProjectEditor : UserControl
    {

        public int GotoOnSave { get; set; }
        public int RootId { get; set; }
        public int TypeId { get; set; }
        public int CategoryRoot { get; set; }

        protected override void OnInit(EventArgs e)
        {
            ((UmbracoDefault)this.Page).ValidateRequest = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");

            if (!Page.IsPostBack)
            {

                int pId = 0;

                Member m = Member.GetCurrentMember();
                var memberHasEnoughReputation = MemberHasEnoughReputation(m);
                if (memberHasEnoughReputation == false)
                {
                    holder.Visible = false;
                    notallowed.Visible = true;
                }
                else
                {
                    if (m.Groups.ContainsKey(MemberGroup.GetByName("Vendor").Id))
                    {
                        p_purchaseUrl.Visible = true;
                        d_notice.Visible = false;
                    }


                    string taglist = string.Empty;
                    XPathNodeIterator tags =
                        umbraco.editorControls.tags.library.getAllTagsInGroup("project").Current.Select("./tags/tag");

                    while (tags.MoveNext())
                    {
                        taglist += "\"" + tags.Current.Value + "\",";
                    }

                    bool hideHq = true;
                    if (m.Groups.ContainsKey(MemberGroup.GetByName("HQ").Id))
                    {
                        hideHq = false;
                    }

                    List<Category> categories = Packages.Categories(false, hideHq);
                    dd_category.Items.Add(new ListItem("Please select...", ""));

                    foreach (Category c in categories)
                    {
                        dd_category.Items.Add(new ListItem(c.Text, c.Id.ToString()));
                    }

                    ScriptManager.RegisterStartupScript(
                        this,
                        this.GetType(),
                        "inittagsuggest",
                        " $(document).ready(function() { $('#projecttagger').autocomplete([" + taglist +
                        "],{max: 8,scroll: true,scrollHeight: 300}); enableTagger();});",
                        true);


                    if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) &&
                        library.IsLoggedOn())
                    {

                        Document d = new Document(pId);

                        if ((int)d.getProperty("owner").Value == m.Id || Utils.IsProjectContributor(m.Id, d.Id))
                        {

                            lt_title.Text = "Edit project";

                            bt_submit.CommandName = "save";
                            bt_submit.CommandArgument = d.Id.ToString();

                            tb_name.Text = d.Text;
                            tb_version.Text = d.getProperty("version").Value.ToString();
                            tb_desc.Text = d.getProperty("description").Value.ToString();

                            cb_stable.Checked = (d.getProperty("stable").Value.ToString() == "1");
                            tb_status.Text = d.getProperty("status").Value.ToString();

                            tb_demoUrl.Text = d.getProperty("demoUrl").Value.ToString();
                            tb_sourceUrl.Text = d.getProperty("sourceUrl").Value.ToString();
                            tb_websiteUrl.Text = d.getProperty("websiteUrl").Value.ToString();

                            tb_licenseUrl.Text = d.getProperty("licenseUrl").Value.ToString();
                            tb_license.Text = d.getProperty("licenseName").Value.ToString();

                            tb_purchaseUrl.Text = d.getProperty("vendorUrl").Value.ToString();

                            dd_category.SelectedValue = d.Parent.Id.ToString();

                            List<WikiFile> Files = WikiFile.CurrentFiles(d.Id);


                            bool hasScreenshots = false;
                            if (Files.Count > 0)
                            {
                                foreach (WikiFile f in Files)
                                {

                                    if (f.FileType != "screenshot")
                                        dd_package.Items.Add(new ListItem(f.Name, f.Id.ToString()));
                                    else
                                    {
                                        dd_screenshot.Items.Add(new ListItem(f.Name, f.Id.ToString()));
                                        hasScreenshots = true;
                                    }

                                }

                                dd_package.SelectedValue = d.getProperty("file").Value.ToString();

                                p_file.Visible = true;
                            }
                            else
                            {
                                p_file.Visible = false;
                            }

                            p_screenshot.Visible = false;

                            if (hasScreenshots)
                            {
                                p_screenshot.Visible = true;
                                dd_screenshot.SelectedValue = d.getProperty("defaultScreenshot").Value.ToString();
                            }
                            else
                            {
                                p_screenshot.Visible = false;
                            }




                            List<ITag> projecttags = umbraco.editorControls.tags.library.GetTagsFromNodeAsITags(pId);

                            if (projecttags.Count > 0)
                            {
                                string stags = string.Empty;
                                foreach (ITag tag in projecttags)
                                {
                                    stags += tag.TagCaption + ",";
                                }

                                stags = stags.Substring(0, stags.Length - 1);

                                ScriptManager.RegisterStartupScript(
                                    this,
                                    this.GetType(),
                                    "inittags",
                                    " $(document).ready(function() {$('#projecttagger').addTag('" + stags + "');});",
                                    true);
                            }
                        }

                    }
                    else
                    {


                        p_screenshot.Visible = false;
                        p_file.Visible = false;
                        lt_title.Text = "Create new project";

                    }
                }
            }
        }

        protected void saveProject(object sender, CommandEventArgs e)
        {

            Member m = Member.GetCurrentMember();
            Document d;

            var memberHasEnoughReputation = MemberHasEnoughReputation(m);
            if (memberHasEnoughReputation == false)
            {
                holder.Visible = false;
                notallowed.Visible = true;
            }
            else
            {
                if (e.CommandName == "save")
                {

                    int pId = int.Parse(e.CommandArgument.ToString());

                    d = new Document(pId);

                    if ((int)d.getProperty("owner").Value == m.Id || Utils.IsProjectContributor(m.Id, d.Id))
                    {
                        d.Text = tb_name.Text;
                        d.getProperty("version").Value = tb_version.Text;
                        d.getProperty("description").Value = tb_desc.Text;

                        d.getProperty("stable").Value = cb_stable.Checked;
                        d.getProperty("status").Value = tb_status.Text;

                        d.getProperty("demoUrl").Value = tb_demoUrl.Text;
                        d.getProperty("sourceUrl").Value = tb_sourceUrl.Text;
                        d.getProperty("websiteUrl").Value = tb_websiteUrl.Text;

                        d.getProperty("vendorUrl").Value = tb_purchaseUrl.Text;

                        d.getProperty("licenseUrl").Value = tb_licenseUrl.Text;
                        d.getProperty("licenseName").Value = tb_license.Text;

                        d.getProperty("file").Value = dd_package.SelectedValue;
                        d.getProperty("defaultScreenshot").Value = dd_screenshot.SelectedValue;


                        if (dd_screenshot.SelectedIndex > -1)
                        {
                            d.getProperty("defaultScreenshotPath").Value =
                                new WikiFile(int.Parse(dd_screenshot.SelectedValue)).Path;
                        }
                        else
                        {
                            d.getProperty("defaultScreenshotPath").Value = "";
                        }

                        if (Request["projecttags[]"] != null)
                        {
                            CommunityController.SetTags(d.Id.ToString(), "project",
                                Request["projecttags[]"].ToString());
                        }


                        Node category = new Node(int.Parse(dd_category.SelectedValue));

                        //if we have a proper category, move the package
                        if (category != null && category.NodeTypeAlias == "ProductGroup") ;
                        {
                            if (d.Parent.Id != category.Id)
                            {
                                d.Move(category.Id);
                            }
                        }


                        if (d.getProperty("packageGuid") == null ||
                            string.IsNullOrEmpty(d.getProperty("packageGuid").Value.ToString()))
                            d.getProperty("packageGuid").Value = Guid.NewGuid().ToString();

                        d.Save();
                        d.Publish(new User(0));

                        library.UpdateDocumentCache(d.Id);
                        library.RefreshContent();
                    }

                }
                else
                {

                    d = Document.MakeNew(tb_name.Text, new DocumentType(TypeId), new User(0),
                        RootId);

                    d.getProperty("version").Value = tb_version.Text;
                    d.getProperty("description").Value = tb_desc.Text;

                    d.getProperty("stable").Value = cb_stable.Checked;

                    d.getProperty("demoUrl").Value = tb_demoUrl.Text;
                    d.getProperty("sourceUrl").Value = tb_sourceUrl.Text;
                    d.getProperty("websiteUrl").Value = tb_websiteUrl.Text;

                    d.getProperty("licenseUrl").Value = tb_licenseUrl.Text;
                    d.getProperty("licenseName").Value = tb_license.Text;

                    d.getProperty("vendorUrl").Value = tb_purchaseUrl.Text;

                    //d.getProperty("file").Value = dd_package.SelectedValue;
                    d.getProperty("owner").Value = m.Id;
                    d.getProperty("packageGuid").Value = Guid.NewGuid().ToString();

                    if (Request["projecttags[]"] != null)
                    {
                        CommunityController.SetTags(d.Id.ToString(), "project", Request["projecttags[]"].ToString());
                        d.getProperty("tags").Value = Request["projecttags[]"].ToString();
                    }

                    Node category = new Node(int.Parse(dd_category.SelectedValue));

                    //if we have a proper category, move the package
                    if (category != null && category.NodeTypeAlias == "ProductGroup") ;
                    {
                        if (d.Parent.Id != category.Id)
                        {
                            d.Move(category.Id);
                        }
                    }

                    d.Save();

                    d.Publish(new User(0));
                    library.UpdateDocumentCache(d.Id);

                    library.RefreshContent();
                }
                Response.Redirect(library.NiceUrl(GotoOnSave));
            }
        }

        private static bool MemberHasEnoughReputation(Member m)
        {
            var reputation = string.Empty;
            if (m.getProperty("reputationTotal") != null && m.getProperty("reputationTotal").Value != null)
                reputation = m.getProperty("reputationTotal").Value.ToString();

            int reputationTotal;
            var enoughReputation = int.TryParse(reputation, out reputationTotal) && reputationTotal > 30;
            return enoughReputation;
        }
    }
}