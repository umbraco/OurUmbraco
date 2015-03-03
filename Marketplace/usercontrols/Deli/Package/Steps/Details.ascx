<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Details.ascx.cs" Inherits="uProject.usercontrols.Deli.Package.Steps.Details" %>

<div class="form simpleForm" id="registrationForm">
<fieldset>
    <legend>Project Information</legend>
    <p>
        <asp:Label runat="server" AssociatedControlID="Title" CssClass="inputLabel">Title</asp:Label>
        <asp:TextBox runat="server" ID="Title" CssClass="required title" ToolTip="Please enter the project name" />
    </p>
    <p>
        <asp:Label runat="server" AssociatedControlID="DevStatus" CssClass="inputLabel">Development Status</asp:Label>
        <asp:TextBox runat="server" ID="DevStatus" CssClass="required title" ToolTip="Please enter the developement status" />
        <small>ex: "Beta", "Experimental" etc</small>
    </p>
    <asp:PlaceHolder runat="server" ID="CommercialOption" Visible="false">
    <p>
        <asp:Label runat="server" AssociatedControlID="DevStatus" CssClass="inputLabel">Free or Commercial</asp:Label>
        <asp:dropdownlist runat="server" ID="CommercialOrFree" CssClass="required title" ToolTip="Project type">
            <asp:ListItem Text="Commercial" Value="commercial" />
            <asp:ListItem Text="Free" Value="free" />
        </asp:dropdownlist>
        <small>Are you intending to sell this or give it away free?</small>
    </p>
    </asp:PlaceHolder>
    <p>
        <asp:Label runat="server" AssociatedControlID="CurrentVersion" CssClass="inputLabel">Current Version</asp:Label>
        <asp:TextBox runat="server" ID="CurrentVersion" ToolTip="Please enter the current version" CssClass="required title"/>
    </p>
    <p>
        <asp:Label runat="server" AssociatedControlID="Stable" CssClass="inputLabel">Project is stable</asp:Label>
        <asp:Checkbox runat="server" ID="Stable" Text="Stable" />
        <small>Can this safely be installed in a production enviroment?</small><br />
    </p>
</fieldset>
<fieldset>
    <legend>Project Category</legend>
    <p>
        <asp:dropdownlist runat="server" ID="Category" CssClass="required title" ToolTip="Please select a category this project would fit into" />
        <small>This will help users find your package</small>
    </p>

</fieldset>
<fieldset>
    <legend>Project Description</legend>
    <p>Be clear and to the point about what your project does.  Provide simple examples of how to use your project.  DO NOT ask for Karma Votes.  If you mention Karma in your description we will instantly mark your project as SPAM.</p>
    <p>
        <asp:TextBox runat="server" ID="Description" style="width: 600px; height: 500px;" TextMode="MultiLine">
        <p>Project Description</p>
        </asp:TextBox>
    </p>
</fieldset>
<fieldset>
  <legend>Project Tags</legend>
  <p>
        <input class="tagger" type="text" name="projecttags[]" id="projecttagger"/>
  </p>
</fieldset>

<fieldset>
    <legend>License Information</legend>
    <p>
        <asp:Label runat="server" AssociatedControlID="LicenseName" CssClass="inputLabel">License Name</asp:Label>
        <asp:TextBox runat="server" ID="LicenseName" ToolTip="Please enter the license name" CssClass="required title" Text="MIT" />
    </p>
    <p>
        <asp:Label runat="server" AssociatedControlID="LicenseUrl" CssClass="inputLabel">License Url</asp:Label>
        <asp:TextBox runat="server" ID="LicenseUrl" ToolTip="Please enter a url to the license document" Text="http://www.opensource.org/licenses/mit-license.php" CssClass="required url title"/>
    </p>
</fieldset>
<fieldset>
    <legend>Resources</legend>
    <p>
    <asp:Label runat="server" AssociatedControlID="ProjectUrl" CssClass="inputLabel">Project Url</asp:Label>
    <asp:TextBox runat="server" ID="ProjectUrl" CssClass="url title"  />
    <small>If you have a project website somewhere else, (like codeplex.com) link to it here.</small>
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="DemoUrl" CssClass="inputLabel">Demonstration Url</asp:Label>
    <asp:TextBox runat="server" ID="DemoUrl" CssClass="url title"  />
    <small>url to where a demonstration or demostration video / description can be found</small>
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="SourceCodeUrl" CssClass="inputLabel">Source Code Url</asp:Label>
    <asp:TextBox runat="server" ID="SourceCodeUrl" CssClass="url title"  />
    <small>url to where the source code is stored, (ie: codeplex.com, github.com etc)</small>
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="SupportUrl" CssClass="inputLabel">Bug tracking Url</asp:Label>
    <asp:TextBox runat="server" ID="SupportUrl" CssClass="url title"  />
    <small>url where users can go to get support and log bugs. (ie: codeplex.com discussion, github.com bug tracking etc)</small>
    </p>
</fieldset>
<fieldset>
    <legend>Analytics and tracking</legend>
    <p>To best understand how your Project is performing, we suggest that you sign up to Google Analytics and track your project.  All you need to do is enter your tracking code.  We will pass you custom tracking events such as views, downloads etc</p>
    <p>
        <asp:Label runat="server" AssociatedControlID="GaCode" CssClass="inputLabel">Google Analytics Code</asp:Label>
        <asp:TextBox runat="server" ID="GaCode" CssClass="title"  />
    </p>
</fieldset>
<fieldset>
    <legend>Collaboration</legend>   
    <p>Are you looking for help making this package great?   Why not open it up for collaboration?</p>
    <p>
    <asp:Checkbox runat="server" ID="Collab" Text="This project is open for collaboration" />
    </p>
</fieldset>

<asp:PlaceHolder ID="LegalDisplay" runat="server" Visible="false">
    <fieldset>
        <legend>The legal part</legend>   
        <p>
        <asp:Checkbox runat="server" ID="Terms" Text="I have read and agree to the terms for my Commercial project" />
        <a href="/wiki/deli/deli-vendor-terms/">Read the Terms for Commercial projects here</a>
        </p>
    </fieldset>
</asp:PlaceHolder>

<div class="buttons">
<asp:Button runat="server" Text="Next" ID="MoveNext" OnClick="SaveStep"  CssClass="submitButton button green tiny"/>
</div>
</div>

<script type="text/javascript">

    $(document).ready(function () {
        $("form").validate({
            invalidHandler: function (f, v) {
                var errors = v.numberOfInvalids();
                if (errors.length > 0) {
                    validator.focusInvalid();
                }
            }
        });
    });

    tinyMCE.init({
        // General options
        mode: "exact",
        elements: "<%= Description.ClientID %>",
        content_css: "/css/fonts.css",
        auto_resize: false,
        theme: "advanced",
        theme_advanced_toolbar_location: "top",
        theme_advanced_buttons1: "bold,italic,underline,bullist,numlist",
        theme_advanced_buttons2: "",
        theme_advanced_buttons3: "",
        remove_linebreaks: false
    });
  
</script>

