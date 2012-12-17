using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umb.OurUmb.MemberLocator.GeoCoding.Services;
using Umb.OurUmb.MemberLocator.GeoCoding;
using Umb.OurUmb.MemberLocator.GeoCoding.Services.Google;
using System.ComponentModel;
using umbraco.cms.businesslogic.member;
using System.Data.SqlClient;
using System.Xml;
using System.Collections;
using System.Net.Mail;

namespace Umb.OurUmb.MemberLocator.Usercontrols
{
    public partial class MemberLocator : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            ((umbraco.UmbracoDefault)this.Page).ValidateRequest = false;
        }

       
        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");
            umbraco.library.RegisterJavaScriptFile("googlemaps", "http://maps.google.com/maps?file=api&v=2&key=" + Utility.GetGoogleMapsKey());


           

            if (!IsPostBack)
            {
                if (Member.GetCurrentMember() == null)
                {
                    pnlLoggedIn.Visible = false;
                    pnlNotLoggedIn.Visible = true;
                }
                else
                {
                   


                    if (Member.GetCurrentMember().Groups.ContainsKey(4686))
                    {
                        //throw new Exception("ok");
                        pnlCreateTopic.Visible = true;
                        SliderExtender1.Maximum = 10000;
                    }


                   

                    txtRadius.Text = "200";
                    double radius = 200;

                    if (Request["radius"] != null && Request["radius"] != string.Empty)
                    {
                        if (double.Parse(Request["radius"], Utility.GetNumberFormatInfo()) <= SliderExtender1.Maximum)
                        {
                            radius = double.Parse(Request["radius"], Utility.GetNumberFormatInfo());
                        }

                    }

                    string location = Member.GetCurrentMember().getProperty("location").Value.ToString();

                    if (Request["s"] != null && Request["s"] != string.Empty)
                    {
                        location = Request["s"];
                    }

                    if (Request["event"] != null)
                    {
                        umbraco.presentation.nodeFactory.Node ev = new umbraco.presentation.nodeFactory.Node(Convert.ToInt32(Request["event"]));

                        location = ev.Name;


                        LocationContainer.Visible = false;

                        lblNotify.Text = "Announce the event";
                        lblSearch.Text = "Change Radius";
                        lblOK.Text = "Ok, event announced";

                        TextBox1.Text = ev.GetProperty("description").Value;

                    }
                    litResults.Text = string.Format("Looking for members within {0} KM of {1} <img src=\"/css/img/ajax-loadercircle.gif\" alt=\"loading\" />", radius.ToString(), location);

                    //show organize meetup if member is in the admin membergroup

                    //MemberGroup admin = MemberGroup.GetByName("admin");
                    //foreach (MemberGroup group in Member.GetCurrentMember().Groups)
                    //{
                    //    //throw new Exception(group.Text);

                    //}
                    

                    //check if member has allready suggested meetup

                    //if (Member.GetCurrentMember().getProperty("lastMeetupSuggestDate").Value != null
                    //    && Member.GetCurrentMember().getProperty("lastMeetupSuggestDate").Value.ToString() != string.Empty
                    //    )
                    //{
                    //    DateTime lastDate = Convert.ToDateTime(Member.GetCurrentMember().getProperty("lastMeetupSuggestDate").Value);

                    //    if (lastDate >= DateTime.Now.AddDays(-14))
                    //    {
                    //        pnlRecentMeetup.Visible = true;
                    //        pnlCreateTopic.Visible = false;

                    //        lnkRecentTopic.NavigateUrl =
                    //            uForum.Library.Xslt.NiceTopicUrl(int.Parse(Member.GetCurrentMember().getProperty("lastMeetupTopicId").Value.ToString()));
                    //    }
                    //}
                }
            }
        }


      

        protected void btnChooseMultiple_Click(object sender, EventArgs e)
        {
            pnlMultiple.Visible = false;
            ShowResult(rblLocations.SelectedItem.Text, new Location(
                Convert.ToDouble(rblLocations.SelectedValue.Split(',')[0], Utility.GetNumberFormatInfo()),
                 Convert.ToDouble(rblLocations.SelectedValue.Split(',')[1], Utility.GetNumberFormatInfo())));
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (Request["event"] == null)
            {
                Response.Redirect(Request.ServerVariables["URL"] + "?s=" + txtLocation.Text + "&radius=" + txtRadius.Text);
            }
            else
            {
                Response.Redirect(Request.ServerVariables["URL"] + "?event=" + Request["event"] + "&radius=" + txtRadius.Text);
            }
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            loadMap();
        }
        protected void btnCreateTopic_Click(object sender, EventArgs e)
        {
            string subject = Member.GetCurrentMember().Text + " suggests a meetup";
            string extrabody = "";

            if (Request["event"] != null)
            {
                umbraco.presentation.nodeFactory.Node ev = new umbraco.presentation.nodeFactory.Node(Convert.ToInt32(Request["event"]));

                subject = "New event in your area: " + ev.Name;

                extrabody = string.Format(
                    "<br/><br/>Check out the event details <a href='{0}'>here</a>",
                    "http://" + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ev.NiceUrl);


                NewTopicContainer.Visible = false;
            }
            else
            {
                uForum.Businesslogic.Topic meetupTopic = uForum.Businesslogic.Topic.Create(1184, Member.GetCurrentMember().Text + " suggests a meetup", TextBox1.Text, Member.GetCurrentMember().Id);

                Member current = Member.GetCurrentMember();
                current.getProperty("lastMeetupSuggestDate").Value = DateTime.Now;

                current.getProperty("lastMeetupTopicId").Value = meetupTopic.Id;
                current.Save();

                lnkNewTopic.NavigateUrl = uForum.Library.Xslt.NiceTopicUrl(meetupTopic.Id);
            }

            if (SendMails && Request["event"] != null)
            {
                foreach (string memberId in ViewState["MemberLocatorMemberIds"].ToString().Split(';'))
                {
                    Member member = new Member(int.Parse(memberId));

                    if (member.getProperty("bugMeNot").Value.ToString() == "0" ||
                        member.getProperty("bugMeNot").Value.ToString() == null ||
                        member.getProperty("bugMeNot").Value.ToString() == "")
                    {
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("robot@umbraco.org","Our umbraco");
                        mail.Subject = subject;
                        mail.To.Add(new MailAddress(new Member(int.Parse(memberId)).Email));
                        //mail.To.Add(new MailAddress("testing@nibble.be"));
                        mail.Body = TextBox1.Text.Replace("\n","<br/>") + extrabody;
                        mail.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient();
                        client.Send(mail);
                    }
                }
            }

            

            pnlCreateTopic.Visible = false;
            pnlCreateTopicSuccess.Visible = true;
        }

        #region Methods


        private void loadMap()
        {

            try
            {

                //check if event
                if (Request["event"] != null)
                {
                    umbraco.presentation.nodeFactory.Node e = new umbraco.presentation.nodeFactory.Node(Convert.ToInt32(Request["event"]));

                   
                    ShowResult(e.Name, new Location(
                        Convert.ToDouble(e.GetProperty("latitude").Value, Utility.GetNumberFormatInfo()),
                         Convert.ToDouble(e.GetProperty("longitude").Value, Utility.GetNumberFormatInfo())));
                }

                else
                {
                    //address supplied
                    if (Request["s"] != null && Request["s"] != string.Empty)
                    {
                        litMultipleResults.Text = MultipleResultsCaption;

                        litResults.Text = string.Empty;
                        pnlMultiple.Visible = false;

                        IGeoCoder geoCoder = new GoogleGeoCoder(Utility.GetGoogleMapsKey());
                        Address[] addresses = geoCoder.GeoCode(Request["s"]);
                        if (addresses.Length == 0)//no results
                        {
                            pnlCreateTopic.Visible = false;

                            litResults.Text = umbraco.macro.GetXsltTransformResult(umbraco.content.Instance.XmlContent, umbraco.macro.getXslt(ResultsXslt));

                            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "test",
           umbraco.macro.GetXsltTransformResult(umbraco.content.Instance.XmlContent, umbraco.macro.getXslt("MemberLocatorScript.xslt")), true);
                        }
                        else
                        {
                            if (addresses.Length > 1)//multiple results, need to choose first
                            {
                                //show the first result
                                ShowResult(addresses[0].ToString(), new Location(addresses[0].Coordinates.Latitude, addresses[0].Coordinates.Longitude));

                                //rblLocations.Items.Clear();
                                //foreach (Address address in addresses)
                                //{
                                //    rblLocations.Items.Add(new ListItem(address.ToString(), address.Coordinates.Latitude.ToString(Utility.GetNumberFormatInfo()) + "," + address.Coordinates.Longitude.ToString(Utility.GetNumberFormatInfo())));
                                //}
                                //pnlMultiple.Visible = true;
                            }
                            else// single result, ok show results
                            {
                                ShowResult(addresses[0].ToString(), new Location(addresses[0].Coordinates.Latitude, addresses[0].Coordinates.Longitude));
                            }
                        }
                    }
                    else //node address, use member location
                    {
                        //use member location
                        Member current = Member.GetCurrentMember();
                        Location memberLocation = new Location(
                            double.Parse(current.getProperty("latitude").Value.ToString(), Utility.GetNumberFormatInfo()),
                            double.Parse(current.getProperty("longitude").Value.ToString(), Utility.GetNumberFormatInfo())
                            );

                        ShowResult(current.getProperty("location").Value.ToString(), memberLocation);
                    }
                }
            }
            catch(Exception ex) {

                if (Request["debug"] == null)
                {
                    litResults.Text = "Error displaying locator results";
                }
                else
                {
                    litResults.Text = ex.Message + "</br>" + ex.StackTrace;
                }
            }

            // }
        }

        private void ShowResult(string locationName, Location location)
        {

          


            Double radius = 200;
            if (Request["radius"] != null && Request["radius"] != string.Empty)
            {
                if (double.Parse(Request["radius"], Utility.GetNumberFormatInfo()) <= SliderExtender1.Maximum)
                {
                    radius = double.Parse(Request["radius"], Utility.GetNumberFormatInfo());
                }

            }

            DistanceUnits distanceunit = DistanceUnits.Miles;
            if (UnitInKm == true)
                distanceunit = DistanceUnits.Kilometers;


            XmlDocument content = new XmlDocument();
            XmlElement root = content.CreateElement("root");


            //search node
            XmlElement infoNode = content.CreateElement("location");
            infoNode.InnerText = location.ToString().Replace(" ", "");
            XmlAttribute nameAttribute = content.CreateAttribute("name");
            nameAttribute.Value = locationName;
            infoNode.Attributes.Append(nameAttribute);
            XmlAttribute latitudeAttribute = content.CreateAttribute("latitude");
            latitudeAttribute.Value = location.Latitude.ToString();
            infoNode.Attributes.Append(latitudeAttribute);
            XmlAttribute longitudeAttribute = content.CreateAttribute("longitude");
            longitudeAttribute.Value = location.Longitude.ToString();
            infoNode.Attributes.Append(longitudeAttribute);
            XmlAttribute unitAttribute = content.CreateAttribute("unit");
            unitAttribute.Value = distanceunit.ToString().ToLower();
            infoNode.Attributes.Append(unitAttribute);
            XmlAttribute radiusAttribute = content.CreateAttribute("radius");
            radiusAttribute.Value = radius.ToString(Utility.GetNumberFormatInfo());
            infoNode.Attributes.Append(radiusAttribute);

            root.AppendChild(infoNode);

            string selectMembLoc = @"
              select m.nodeId, m.email, m.loginName, m.password, lat.dataNvarchar as Latitude, lon.dataNvarchar as Longitude, karma.dataInt as Karma, memberdata.text as MemberData, avatar.datanvarchar as Avatar  from cmsMember m
                    INNER JOIN cmsPropertyData lat
                    on m.nodeId = lat.contentnodeid
                    INNER JOIN cmsPropertyData lon
                    on m.nodeId = lon.contentnodeid
                    Inner Join cmsPropertyData karma
                    on m.nodeId = karma.contentNodeId
                    Inner Join umbracoNode memberdata
                    on m.nodeId =  memberdata.id
                    Inner Join cmsPropertyData avatar
                    on m.nodeId = avatar.contentnodeid
                    where lat.propertytypeid = 43 and lat.dataNvarchar is not null and lat.dataNvarchar != ''
                    and lon.propertytypeid = 34 and lon.dataNvarchar is not null and lon.dataNvarchar != ''
                    and karma.propertytypeid = 32 and avatar.propertytypeid = 39
                and Cast(lat.dataNvarchar as decimal(10)) > @minlat and Cast(lat.dataNvarchar as decimal(10)) < @maxlat
                and Cast(lon.dataNvarchar as decimal(10)) > @minlon and Cast(lon.dataNvarchar as decimal(10)) < @maxlon;";

            SqlConnection conn = new SqlConnection(umbraco.GlobalSettings.DbDSN);
            SqlCommand commMembLoc = new SqlCommand(selectMembLoc, conn);

           

            commMembLoc.Parameters.AddWithValue("@minlat", location.Latitude - Math.Ceiling(radius / 85));
            commMembLoc.Parameters.AddWithValue("@maxlat", location.Latitude + Math.Ceiling(radius / 85));
            commMembLoc.Parameters.AddWithValue("@minlon", location.Longitude - Math.Ceiling(radius / 85));
            commMembLoc.Parameters.AddWithValue("@maxlon", location.Longitude + Math.Ceiling(radius / 85));

            conn.Open();

            bool found = false;
            string memberIds = string.Empty;

            try
            {
                SqlDataReader reader = commMembLoc.ExecuteReader();

                while (reader.Read())
                {


                    int id = reader.GetInt32(0);
                    int karma = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);

                    memberIds += id.ToString() + ";";


                    Location memberLocation = new Location(
                        double.Parse(reader.GetString(4), Utility.GetNumberFormatInfo()),
                        double.Parse(reader.GetString(5), Utility.GetNumberFormatInfo()));

                    double distance = location.DistanceBetween(memberLocation, distanceunit);
                    //Response.Output.WriteLine("Member " + id.ToString() + "distance " + distance.ToString(Utility.GetNumberFormatInfo()));

                    if (distance <= radius)
                    {
                        found = true;

                        XmlElement memberNode = content.CreateElement("member");

                        XmlAttribute idAttribute = content.CreateAttribute("id");
                        idAttribute.Value = id.ToString();
                        memberNode.Attributes.Append(idAttribute);

                        //name
                        XmlAttribute membernameAttribute = content.CreateAttribute("name");
                        membernameAttribute.Value = reader.GetString(7);
                        memberNode.Attributes.Append(membernameAttribute);

                       

                        //karma
                        XmlAttribute karmaAttribute = content.CreateAttribute("karma");
                        karmaAttribute.Value = karma.ToString();
                        memberNode.Attributes.Append(karmaAttribute);

                        XmlElement distanceNode = content.CreateElement("data");
                        distanceNode.InnerText = distance.ToString(Utility.GetNumberFormatInfo());
                        XmlAttribute aliasAttribute = content.CreateAttribute("alias");
                        aliasAttribute.Value = "distance";
                        distanceNode.Attributes.Append(aliasAttribute);
                        memberNode.AppendChild(distanceNode);

                        XmlElement avatarNode = content.CreateElement("data");
                        avatarNode.InnerText = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                        XmlAttribute avatarAliasAttribute = content.CreateAttribute("alias");
                        avatarAliasAttribute.Value = "avatar";
                        avatarNode.Attributes.Append(avatarAliasAttribute);
                        memberNode.AppendChild(avatarNode);

                        XmlElement locationNode = content.CreateElement("data");
                        locationNode.InnerText = memberLocation.ToString();
                        XmlAttribute locationAliasAttribute = content.CreateAttribute("alias");
                        locationAliasAttribute.Value = "location";
                        locationNode.Attributes.Append(locationAliasAttribute);
                        memberNode.AppendChild(locationNode);



                        root.AppendChild(memberNode);
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            if (!found)
            {
                pnlCreateTopic.Visible = false;
            }


            

            ViewState["MemberLocatorMemberIds"] =
                memberIds.Length > 0 ? memberIds.Substring(0, memberIds.Length - 1) : "";

            //litResults.Text = root.InnerXml.ToString() + ResultsXslt;

            content.AppendChild(root);
            litResults.Text = umbraco.macro.GetXsltTransformResult(content, umbraco.macro.getXslt("MemberLocator.xslt"));

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "MemberLocator",
                umbraco.macro.GetXsltTransformResult(content, umbraco.macro.getXslt("MemberLocatorScript.xslt")), true);

          
            if (Member.GetCurrentMember().Groups.ContainsKey(4686))
            {
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "MemberLocatorTiny",
                    "topictiny();", true);
            }

        }

        #endregion


        #region Properties


        private string _resultXslt = "MemberLocator.xslt";
        private bool _unitInKm = false;
        private string _multipleResultsCaption = "There are multiple results, please choose:";
        private bool _sendMails = false;

        [DefaultValue("MemberLocator.xslt")]
        public string ResultsXslt
        {
            get { return _resultXslt; }
            set { _resultXslt = value; }
        }

        [DefaultValue(false)]
        public bool UnitInKm
        {
            get { return _unitInKm; }
            set { _unitInKm = value; }
        }

        [DefaultValue("There are multiple results, please choose:")]
        public string MultipleResultsCaption
        {
            get { return _multipleResultsCaption; }
            set { _multipleResultsCaption = value; }
        }

        [DefaultValue(false)]
        public bool SendMails
        {
            get { return _sendMails; }
            set { _sendMails = value; }
        }

        #endregion

     

       

    }
}