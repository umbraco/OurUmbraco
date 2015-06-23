using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;

namespace uWiki.usercontrols {
    public partial class FileUpload : System.Web.UI.UserControl {

        public string MemberGuid = "";
        public string VersionGuid = "";
        private int pageId = 0;

        private void RebindFiles()
        {
            List<uWiki.Businesslogic.WikiFile> files = uWiki.Businesslogic.WikiFile.CurrentFiles(pageId);

            rp_files.DataSource = files;
            rp_files.Visible = (files.Count > 0);
            rp_files.DataBind();
        }


        protected void DeleteFile(object sender, CommandEventArgs e)
        {
            uWiki.Businesslogic.WikiFile wf = new uWiki.Businesslogic.WikiFile(int.Parse(e.CommandArgument.ToString()));
            //Member mem = Member.GetCurrentMember();

            //if (wf.CreatedBy == mem.Id)
                wf.Delete();

            RebindFiles();
        }

        protected void OnFileBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                uWiki.Businesslogic.WikiFile wf = (uWiki.Businesslogic.WikiFile)e.Item.DataItem;

                Literal _name = (Literal)e.Item.FindControl("lt_name");
                Literal _date = (Literal)e.Item.FindControl("lt_date");
                Button _delete = (Button)e.Item.FindControl("bt_delete");
                Literal _version = (Literal)e.Item.FindControl("lt_version");

                _name.Text = "<a href='" + wf.Path + "'>" + wf.Name + "</a>";
                _date.Text = wf.CreateDate.ToShortDateString() + " - " + wf.CreateDate.ToShortTimeString();
                _delete.CommandArgument = wf.Id.ToString();

                if (wf.Versions != null)
                    _version.Text = uWiki.Businesslogic.WikiFile.ToVersionString(wf.Versions);

                if (Member.GetCurrentMember().Id == wf.CreatedBy || uWiki.Library.Utils.IsInGroup("admin"))
                    _delete.Enabled = true;


            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            if (umbraco.library.IsLoggedOn())
            {
                pageId = umbraco.presentation.nodeFactory.Node.GetCurrent().Id;
                               

                Member mem = Member.GetCurrentMember();
                Document d = new Document(pageId);

                //if (n.GetProperty("owner") != null && n.GetProperty("owner").Value == mem.Id.ToString())
                //{
                holder.Visible = true;
                RebindFiles();

                umbraco.library.RegisterJavaScriptFile("swfUpload", "/scripts/swfupload/SWFUpload.js");
                umbraco.library.RegisterJavaScriptFile("swfUpload_cb", "/scripts/swfupload/callbacks.js");
                umbraco.library.RegisterJavaScriptFile("swfUpload_progress", "/scripts/swfupload/fileprogress.js");

                MemberGuid = mem.UniqueId.ToString();
                VersionGuid = d.Version.ToString();

                string defaultVersion = uWiki.Businesslogic.UmbracoVersion.DefaultVersion().Version;
                string options = "";

                foreach (uWiki.Businesslogic.UmbracoVersion uv in uWiki.Businesslogic.UmbracoVersion.AvailableVersions().Values)
                {
                    string selected = "selected='true'";
                    if (uv.Version != defaultVersion)
                        selected = "";
                    options += string.Format("<option value='{0}' {2}>{1}</option>", uv.Version, uv.Name, selected);
                }

                lt_versions.Text = options;
                //}
            }
            else
            {
                notLoggedIn.Visible = true;
            }
        }
    }
}