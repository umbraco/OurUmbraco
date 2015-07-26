using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;
using Umbraco.Core;

namespace our.usercontrols {
    public partial class ProjectForums : System.Web.UI.UserControl
    {

        private readonly ForumService _forumService = new ForumService(ApplicationContext.Current.DatabaseContext);

        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {

                int pId = 0;

                if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn()) {
                    
                    Member m = Member.GetCurrentMember();
                    Document d = new Document(pId);

                    if ((int)d.getProperty("owner").Value == m.Id) {
                        holder.Visible = true;

                        rp_forums.DataSource = _forumService.GetForums(pId);
                        rp_forums.DataBind();



                        int fId = 0;

                        if (!string.IsNullOrEmpty(Request.QueryString["forum"]) && int.TryParse(Request.QueryString["forum"], out fId))
                        {
                            var f = _forumService.GetById(fId);
                            tb_desc.Text = f.Description;
                            tb_name.Text = f.Title;

                            bt_submit.CommandArgument = f.Id.ToString();
                            bt_delete.CommandArgument = f.Id.ToString();

                            bt_submit.CommandName = "edit";

                            ph_add.Visible = true;
                            ph_edit.Visible = true;


                        }
                        else if (!string.IsNullOrEmpty(Request.QueryString["add"]))
                        {
                            ph_add.Visible = true;
                            ph_edit.Visible = false;
                        }
                    }
                }

                
            }
        }

        protected void bindForum(object sender, RepeaterItemEventArgs e) {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item) {
                var f = (Forum)e.Item.DataItem;
                Literal _title = (Literal)e.Item.FindControl("lt_titel");
                Literal _desc = (Literal)e.Item.FindControl("lt_desc");
                Literal _link = (Literal)e.Item.FindControl("lt_link");

                _title.Text = f.Title;
                _desc.Text = f.Description;
                _link.Text = "<a href='?id=" + Request.QueryString["id"] + "&forum=" + f.Id.ToString() + "'>Edit</a>";
            }
        }
        
               
        protected void modifyForum(object sender, CommandEventArgs e) {

            int pId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn()) {

                Member m = Member.GetCurrentMember();
                Document d = new Document(pId);
                Document fnode = null;

                if (e.CommandName == "edit") {
                    int fId = int.Parse(e.CommandArgument.ToString());
                    fnode = new Document(fId);

                } else if(e.CommandName == "create") {
                    
                    fnode = Document.MakeNew(tb_name.Text, DocumentType.GetByAlias("Forum"), new umbraco.BusinessLogic.User(0), d.Id);
                

                } else if(e.CommandName == "delete"){
                    
                    int fId = int.Parse(e.CommandArgument.ToString());

                    if (Document.IsDocument(fId))
                    {
                        fnode = new Document(fId);
                    }



                    if (fnode != null)
                    {
                        if ((int)d.getProperty("owner").Value == m.Id && fnode.ParentId == d.Id)
                        {
                            fnode.delete();

                            //if still not dead it's because it's in the trashcan and should be deleted once more.
                            if (fnode.ParentId == -20)
                                fnode.delete();


                        }

                        if (fnode.ParentId == -20)
                            fnode.delete();

                        fnode = null;

                    }
                    var f = _forumService.GetById(fnode.Id);
                    if (f != null)
                        _forumService.Delete(f);
                    
                    

                   

                
                }
                                
                if (fnode != null && (int)d.getProperty("owner").Value == m.Id && fnode.ParentId == d.Id) {
                    fnode.Text = tb_name.Text;
                    fnode.getProperty("forumDescription").Value = tb_desc.Text;
                    fnode.getProperty("forumAllowNewTopics").Value = true;
                    fnode.Publish(new umbraco.BusinessLogic.User(0));
                    fnode.Save();
                    umbraco.library.UpdateDocumentCache(fnode.Id);

                    Forum f = _forumService.GetById(fnode.Id);

                    if (f == null)
                    {
                        f = new Forum();
                        f.Id = fnode.Id;
                        f.ParentId = fnode.ParentId;
                        f.SortOrder = 0;
                        f.TotalTopics = 0;
                        f.TotalComments = 0;
                        f.LatestPostDate = DateTime.Now;
                        _forumService.Save(f);
                    }
                }

                
                Response.Redirect(Node.GetCurrent().NiceUrl + "?id=" + pId.ToString());
            
            }
        }
    }
}