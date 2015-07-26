using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OurUmbraco.Our;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;

namespace our.usercontrols {
    public partial class ProjectFileUpload : System.Web.UI.UserControl {
       
        public string MemberGuid = "";
        public string VersionGuid = "";
        private int pageId = 0;

        private void RebindFiles() {
         List<WikiFile> files = WikiFile.CurrentFiles(pageId);
         rp_files.DataSource = files;
         rp_files.Visible = (files.Count > 0);
         rp_files.DataBind();  
        }


        protected void DeleteFile(object sender, CommandEventArgs e) {
            WikiFile wf = new WikiFile( int.Parse(e.CommandArgument.ToString()) );
            Member mem = Member.GetCurrentMember();
            
            if(wf.CreatedBy == mem.Id || Utils.IsProjectContributor(mem.Id,pageId))
                wf.Delete();

            RebindFiles();
        }

        protected void ArchiveFile(object sender, CommandEventArgs e)
        {
            WikiFile wf = new WikiFile(int.Parse(e.CommandArgument.ToString()));

            if (e.CommandName == "Unarchive")
            {
                wf.Archived = false;
            }
            else
            {
                wf.Archived = true;
            }

            wf.Save();
            RebindFiles();
        }

        protected void OnFileBound(object sender, RepeaterItemEventArgs e){
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) {
                WikiFile wf = (WikiFile)e.Item.DataItem;

                Literal _name = (Literal)e.Item.FindControl("lt_name");
                Literal _date = (Literal)e.Item.FindControl("lt_date");
                Button _delete = (Button)e.Item.FindControl("bt_delete");
                Literal _type = (Literal)e.Item.FindControl("lt_type");
                Literal _version = (Literal)e.Item.FindControl("lt_version");

                Button _archive = (Button)e.Item.FindControl("bt_archive");

                _archive.CommandArgument = wf.Id.ToString();

                if (wf.Archived)
                {
                    _archive.Text = "Unarchive";
                    _archive.CommandName = "Unarchive";
                }
                else
                {
                    _archive.Text = "Archive";
                    _archive.CommandName = "Archive";
                }

                if (wf.FileType.Trim().ToLower()== "screenshot")
                {
                    _archive.Visible = false;

                }

                if(wf.Versions != null)
                    _version.Text = WikiFile.ToVersionString(wf.Versions);

                _type.Text = wf.FileType;
                _name.Text = "<a href='" + wf.Path + "'>" + wf.Name + "</a>";
                _date.Text = wf.CreateDate.ToShortDateString() + " - " + wf.CreateDate.ToShortTimeString();
                _delete.CommandArgument = wf.Id.ToString();
                
            }
        }

        protected void Page_Load(object sender, EventArgs e) {

           
            if (umbraco.library.IsLoggedOn() && int.TryParse(Request.QueryString["id"], out pageId)) {

                Member mem = Member.GetCurrentMember();
                Document d = new Document(pageId);

                if((d.getProperty("owner") != null && d.getProperty("owner").Value.ToString() == mem.Id.ToString()) ||
                    Utils.IsProjectContributor(mem.Id,pageId)){
                    holder.Visible = true;
                    RebindFiles();

                    umbraco.library.RegisterJavaScriptFile("swfUpload", "/scripts/swfupload/SWFUpload.js");
                    umbraco.library.RegisterJavaScriptFile("swfUpload_cb", "/scripts/swfupload/callbacks.js");
                    umbraco.library.RegisterJavaScriptFile("swfUpload_progress", "/scripts/swfupload/fileprogress.js");

                    MemberGuid = mem.UniqueId.ToString();
                    VersionGuid = d.Version.ToString();

                    string defaultVersion = UmbracoVersion.DefaultVersion().Version;
                    string options = "";

                    foreach (UmbracoVersion uv in UmbracoVersion.AvailableVersions().Values)
                    {
                        string selected = "selected='true'";
                        if (uv.Version != defaultVersion)
                            selected = "";
                        options += string.Format("<option value='{0}' {2}>{1}</option>", uv.Version, uv.Name, selected);
                    }

                    lt_versions.Text = options;
                
                }
            }
        }
        
    }
}