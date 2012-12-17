using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;

namespace our.usercontrols
{
    public partial class EventEditor : System.Web.UI.UserControl
    {
        private Member m = umbraco.cms.businesslogic.member.Member.GetCurrentMember();
        public int EventsRoot { get; set; }

        protected override void OnInit(EventArgs e)
        {
            ((umbraco.UmbracoDefault)this.Page).ValidateRequest = false;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");

            //edit?
            if (!Page.IsPostBack && !string.IsNullOrEmpty(Request.QueryString["id"]))
            {

                Node n = new Node(int.Parse(Request.QueryString["id"]));
                //allowed?
                if (n.NodeTypeAlias == "Event" && int.Parse(n.GetProperty("owner").Value) == m.Id)
                {
                    tb_name.Text = n.Name;
                    tb_desc.Text = n.GetProperty("description").Value;

                    tb_venue.Text = n.GetProperty("venue").Value;
                    tb_capacity.Text = n.GetProperty("capacity").Value;

                    tb_lat.Value = n.GetProperty("latitude").Value;
                    tb_lng.Value = n.GetProperty("longitude").Value;

                    dp_startdate.Text = DateTime.Parse(n.GetProperty("start").Value).ToString("MM/dd/yyyy H:mm");
                    dp_enddate.Text = DateTime.Parse(n.GetProperty("end").Value).ToString("MM/dd/yyyy H:mm");
                }
            }
        }

        protected void createEvent(object sender, EventArgs e)
        {
            //edit?
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {

                Document d = new Document(int.Parse(Request.QueryString["id"]));
                //allowed?
                if (d.ContentType.Alias == "Event" && int.Parse(d.getProperty("owner").Value.ToString()) == m.Id)
                {
                    d.Text = tb_name.Text;
                    d.getProperty("description").Value = tb_desc.Text;

                    d.getProperty("venue").Value = tb_venue.Text;
                    d.getProperty("latitude").Value = tb_lat.Value;
                    d.getProperty("longitude").Value = tb_lng.Value;

                    bool sync = false;
                    if (tb_capacity.Text != d.getProperty("capacity").Value.ToString())
                        sync = true;

                    d.getProperty("capacity").Value = tb_capacity.Text;

                    d.getProperty("start").Value = DateTime.Parse(dp_startdate.Text);
                    d.getProperty("end").Value = DateTime.Parse(dp_enddate.Text);

                    d.Save();
                    d.Publish(new umbraco.BusinessLogic.User(0));
                    
                    umbraco.library.UpdateDocumentCache(d.Id);

                    if (sync)
                    {
                        uEvents.Event ev = new uEvents.Event(d);
                        ev.syncCapacity();
                    }
                    

                    Response.Redirect(umbraco.library.NiceUrl(d.Id));
                }
            }
            else
            {
                Document d = Document.MakeNew(tb_name.Text, DocumentType.GetByAlias("Event"), new umbraco.BusinessLogic.User(0), EventsRoot);

                d.getProperty("description").Value = tb_desc.Text;

                d.getProperty("venue").Value = tb_venue.Text;
                d.getProperty("latitude").Value = tb_lat.Value;
                d.getProperty("longitude").Value = tb_lng.Value;

                d.getProperty("capacity").Value = tb_capacity.Text;
                d.getProperty("signedup").Value = 0;

                d.getProperty("start").Value = DateTime.Parse(dp_startdate.Text);
                d.getProperty("end").Value = DateTime.Parse(dp_enddate.Text);

                d.getProperty("owner").Value = m.Id;

                d.Save();
                d.Publish(new umbraco.BusinessLogic.User(0));
                umbraco.library.UpdateDocumentCache(d.Id);

                Response.Redirect(umbraco.library.NiceUrl(d.Id));
            }
        }

    }
}