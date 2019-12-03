<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectEditor.ascx.cs" Inherits="OurUmbraco.Our.usercontrols.ProjectEditor" %>

<asp:PlaceHolder runat="server" ID="notallowed" Visible="False">
    <h2>Sorry, your account is too new to create projects! If you're human, make sure to <a href="https://umbraco.com/about-us/team">get in touch with us</a> to get this restriction lifted.</h2>
</asp:PlaceHolder>

<asp:PlaceHolder ID="holder" runat="server">

<h1><asp:Literal runat="server" ID="lt_title" Text="Create project"></asp:Literal></h1>

<div class="notice" runat="server" ID="d_notice">
<p>
  If you wish to add a <em>commercial package</em> please get in touch <a href="http://umbraco.org/about/contact/commercial-package" target="_blank" rel="noreferrer noopener">here</a>.
</p>

</div>

<div class="form simpleForm" id="registrationForm">
<fieldset>
<legend>Project Information</legend>
  <p>
  Essential project details
  </p>
  <p>
    <asp:label ID="Label1" AssociatedControlID="tb_name" CssClass="inputLabel" runat="server">Project Name</asp:label>
    <asp:TextBox ID="tb_name" runat="server" ToolTip="Please enter the project name" CssClass="required title"/>
  </p>
  <p>
    <asp:label ID="Label2" AssociatedControlID="tb_version" CssClass="inputLabel" runat="server">Current Version</asp:label>
    <asp:TextBox ID="tb_version" runat="server" ToolTip="Please enter the current version" CssClass="required title"/>
  </p>
  <p>
    <asp:label ID="Label3" AssociatedControlID="tb_status" CssClass="inputLabel"  runat="server">Developement Status</asp:label>
    <asp:TextBox ID="tb_status" runat="server" ToolTip="What state is the project in" CssClass="required title"/>
    <small>ex: "Beta", "Experimental" etc</small>
  </p>
  <p>
    <asp:label ID="Label4" AssociatedControlID="cb_stable" CssClass="inputLabel" runat="server">Project is stable</asp:label>
    <asp:CheckBox ID="cb_stable" runat="server" />
    <small>Can this safely be installed in a production enviroment?</small><br />
  </p>
  <p runat="server" id="p_file">
    <asp:label ID="Label10" AssociatedControlID="dd_package" CssClass="inputLabel" runat="server">Current Release</asp:label>
    <asp:DropDownList ID="dd_package" runat="server" CssClass="required title"></asp:DropDownList><br />
    <small>Select the file which is the current release file</small>
  </p>
  
  <p runat="server" id="p_screenshot">
    <asp:label ID="Label11" AssociatedControlID="dd_screenshot" CssClass="inputLabel" runat="server">Default screenshot</asp:label>
    <asp:DropDownList ID="dd_screenshot" runat="server" CssClass="required title"></asp:DropDownList><br />
    <small>Select the screenshot that will be used as project thumbnail</small>
  </p>
  
</fieldset>

<fieldset>
<legend>License</legend>
<p>
    <asp:label ID="Label5" AssociatedControlID="tb_license" CssClass="inputLabel" runat="server">License Name</asp:label>
    <asp:TextBox ID="tb_license" runat="server" ToolTip="Please enter the license name" CssClass="required title" Text="MIT"/>
  </p>
  <p>
    <asp:label ID="Label6" AssociatedControlID="tb_licenseUrl" CssClass="inputLabel" runat="server">Url to licenses</asp:label>
    <asp:TextBox ID="tb_licenseUrl" runat="server" ToolTip="Please enter a url to the license document" Text="http://www.opensource.org/licenses/mit-license.php" CssClass="required url title"/>
  </p>
</fieldset>

<fieldset>
<legend>Resources</legend>
  <p>
   Please enter URLs to external sources of information like souce control, project website and demo pages.
  </p>
  <p>
    <asp:label ID="Label7" AssociatedControlID="tb_sourceUrl" CssClass="inputLabel" runat="server">Source code</asp:label>
    <asp:TextBox ID="tb_sourceUrl" runat="server" ToolTip="Please enter a valid Url, ei: http://test.org" CssClass="title url" Text=""/>
    <small>url to where the source code is stored, (ie: codeplex.com, github.com etc)</small>
  </p>
  <p>
    <asp:label ID="Label8" AssociatedControlID="tb_demoUrl" CssClass="inputLabel" runat="server">Demonstration</asp:label>
    <asp:TextBox ID="tb_demoUrl" runat="server" ToolTip="Please enter a valid Url, ei: http://test.org" CssClass="title url" Text=""/>
    <small>url to where a demonstration or demostration video / description can be found</small>
  </p>
  <p>
    <asp:label ID="Label9" AssociatedControlID="tb_websiteUrl" CssClass="inputLabel" runat="server">Workspace</asp:label>
    <asp:TextBox ID="tb_websiteUrl" runat="server" ToolTip="Please enter a valid Url, ei: http://test.org" CssClass="title url" Text=""/>
    <small>If you have a project website somewhere else, (like codeplex.com) link to it here.</small>
  </p>
  <p runat="server" ID="p_purchaseUrl" visible="false">
     <asp:label ID="Label12" AssociatedControlID="tb_purchaseUrl" CssClass="inputLabel" runat="server">Purchase</asp:label>
     <asp:TextBox ID="tb_purchaseUrl" runat="server" ToolTip="Please enter a valid Url, ei: http://test.org" CssClass="title url" Text=""/>
      <small>If it's a commercial project please link to the purchase url here.</small>
  </p>
</fieldset>

<fieldset>
  <legend>Project Description</legend>
  <p>
    <asp:TextBox ID="tb_desc" runat="server" style="width: 600px; height: 500px;" TextMode="MultiLine" />
  </p>
</fieldset>

<fieldset>
  <legend>Project Tags</legend>
  <p>
        <input class="tagger" type="text" name="projecttags[]" id="projecttagger"/>
  </p>
</fieldset>

<fieldset>
  <legend>Project category</legend>
  <p>
       <asp:DropDownList ID="dd_category" runat="server" ToolTip="Please select a category this project would fit into" CssClass="required title" />
       <small>To be listed in the umbraco repository, a project must have a overall category</small>
  </p>
</fieldset>

<div class="buttons">
  <asp:Button ID="bt_submit" Text="Save" CssClass="submitButton" OnCommand="saveProject" CommandName="create" runat="server" />
</div>
   
</div>

<script type="text/javascript">

    $(document).ready(function () {
        $("form").validate();
    });

    tinyMCE.init({
        // General options
        mode: "exact",
        elements: "<%= tb_desc.ClientID %>",
    content_css: "/css/fonts.css",
    auto_resize: true,
    theme: "simple",
    remove_linebreaks: false
  });

</script>

</asp:PlaceHolder>