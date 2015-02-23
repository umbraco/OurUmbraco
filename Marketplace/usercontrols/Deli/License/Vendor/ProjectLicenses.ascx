<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectLicenses.ascx.cs" Inherits="Marketplace.usercontrols.Deli.License.Vendor.ProjectLicenses" %>
<h2>
Manage Licenses for <asp:Literal runat="server" id="ProjectName" />
</h2>
<asp:Button runat="server" ID="CreateNew" OnClick="CreateNew_Click" text="Create new license"/>

    <asp:PlaceHolder runat="server" ID="Create_LicenseError" Visible="false">
    <div class="errorBox">
        <p><asp:Literal runat="server" ID="LicenseErrorMessage" /></p>    
    </div>
    </asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="ConfigurationForm" Visible="false">

<div class="form simpleForm">
    <fieldset>
    <legend>Configure <asp:Literal runat="server" ID="ConfigurationLicenseType" /> for <asp:Literal runat="server" ID="ConfigurationProjectName" /> for Member: <asp:Literal runat="server" ID="ConfigurationMemberName" /></legend>
<asp:HiddenField ID="Configure_Id" runat="server" />
<asp:PlaceHolder runat="server" ID="DomainIpConfig">
    <p>
    <asp:Label ID="Label1" runat="server" AssociatedControlID="Configure_DevConfig" Text="Development" cssclass="inputLabel" />
    <asp:TextBox runat="server" ID="Configure_DevConfig" CssClass="required title" />
    </p>
    <p>
    <asp:Label ID="Label2" runat="server" AssociatedControlID="Configure_StagingConfig" Text="Staging" cssclass="inputLabel" />
    <asp:TextBox runat="server" ID="Configure_StagingConfig" CssClass="title" />
    </p>
    <p>
    <asp:Label ID="Label3" runat="server" AssociatedControlID="Configure_ProductionConfig" Text="Production" cssclass="inputLabel" />
    <asp:TextBox runat="server" ID="Configure_ProductionConfig" CssClass="required title" />
    </p>
    <small>(Hint: use the root of your domain. &quot;mydomain.com&quot; instead of &quot;www.mydomain.com&quot; as this will allow the license to be used on all subdomains also)</small>
</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="UnlimitedConfig" Visible="false">
    <p>This is an unlimited license and does not require any further configuration.</p>
</asp:PlaceHolder>

<div class="buttons"><asp:Button runat="server" ID="bt_Configure" cssclass="submitButton" Text="Generate License" OnClick="UpdateLicense_Click" /></div>
</fieldset>


</div>
<script type="text/javascript">

    $(document).ready(function () {
        var form = $("form");

        form.validate({
            onsubmit: false,
            invalidHandler: function (f, v) {
                var errors = v.numberOfInvalids();
                if (errors.length > 0) {
                    validator.focusInvalid();
                }
            }
        });


        $(".submitButton").click(function (evt) {
            // Validate the form and retain the result.
            var isValid = $("form").valid();

            // If the form didn't validate, prevent the
            //  form submission.
            if (!isValid)
                evt.preventDefault();
        });

    });


    </script>


</asp:PlaceHolder>



<asp:PlaceHolder runat="server" ID="CreateNewForm" Visible="false">

<div class="form simpleForm">
    <fieldset>
    <legend>Create License</legend>
<p>
<asp:label ID="Label4" runat="server" AssociatedControlID="Create_LicenseType" Text="License Type" cssclass="inputLabel" />
<asp:DropDownList runat="server" id="Create_LicenseType" cssclass="title" OnSelectedIndexChanged="CreateTypeChanged" AutoPostBack="true">
</asp:DropDownList>
</p>

<p>
<asp:label ID="Label5" runat="server" AssociatedControlID="Create_MemberEmail" Text="Member Email Address" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Create_MemberEmail" cssclass="required title email" ToolTip="You must provide a valid email address" />
<asp:placeholder runat="server" ID="Create_InvalidMember" Visible="false"><p>Sorry this user cannot be found.</p></asp:placeholder>
</p>

<asp:PlaceHolder runat="server" ID="CreateDomainIpConfig">
<p>
<asp:Label ID="Label6" runat="server" AssociatedControlID="Create_DevConfig" Text="Development" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Create_DevConfig" cssclass="required title" ToolTip="You must provide the development domain"  />
</p>
<p>
<asp:Label ID="Label7" runat="server" AssociatedControlID="Create_StagingConfig" Text="Staging" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Create_StagingConfig" CssClass="title" />
</p>
<p>
<asp:Label ID="Label8" runat="server" AssociatedControlID="Create_ProductionConfig" Text="Production" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Create_ProductionConfig" cssclass="required title" ToolTip="You must provide the production domain" />
</p>

<small>(Hint: use the root of your domain. &quot;mydomain.com&quot; instead of &quot;www.mydomain.com&quot; as this will allow the license to be used on all subdomains also)</small>
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="CreateUnlimitedConfig">
    <p>You have selected the unlimited license type.  This license type does not require any further configuration.</p>
    </asp:PlaceHolder>
    <div class="buttons">
        <asp:Button runat="server" id="bt_Create" cssclass="submitButton" Text="Generate License" OnClick="CreateLicense_Click" />
    </div>
</fieldset>
</div>



<script type="text/javascript">


    $(document).ready(function () {
        var form = $("form");

        form.validate({
            onsubmit: false,
            invalidHandler: function (f, v) {
                var errors = v.numberOfInvalids();
                if (errors.length > 0) {
                    validator.focusInvalid();
                }
            }
        });


        $(".submitButton").click(function (evt) {
            // Validate the form and retain the result.
            var isValid = $("form").valid();

            // If the form didn't validate, prevent the
            //  form submission.
            if (!isValid)
                evt.preventDefault();
        });

    });


    </script>

</asp:PlaceHolder>




<div><p>
<asp:Label runat="server" AssociatedControlID="Find_LicenseTerm" Text="Find by IP address or domain:" />
<asp:TextBox runat="server" ID="Find_LicenseTerm" />
<asp:Button runat="server" ID="Find_LicenseSubmit" Text="Find" OnClick="Find_LicenseSubmit_Click" />
</p></div>
<asp:placeHolder runat="server" ID="FilteringByMessage" Visible="false">
    <p>
     <strong><asp:literal runat="server" ID="Find_RecordsFound" /></strong> Found with term <strong><asp:Literal ID="Find_Filter" runat="server" /></strong>
    </p>
</asp:placeHolder>
<asp:repeater runat="server" ID="RP_Licenses"  >
<HeaderTemplate>
<table class="dataTable">
    <thead>
        <tr>
            <th>License Type</th>
            <th>Configured For</th>
            <th>Member</th>
            <th class="center">License File</th>
        <tr>
    </thead>
</HeaderTemplate>
<ItemTemplate>
    <tbody>
        <tr>
            <td>
                <%# Eval("LicType") %>
            </td>
            <td><small>
                <strong>Dev:</strong> <%# Eval("DevConfig") %><br />
                <strong>Staging:</strong> <%# Eval("StagingConfig") %><br />
                <strong>Production:</strong> <%# Eval("ProductionConfig")%>
                </small>
            </td>
            <td>
                <%# Eval("MemberName") %>
            </td>
            <td class="right"><asp:Button ID="BT_Action" runat="server" CssClass="actionButton" /> <asp:Button ID="BT_EnableDisable" runat="server" OnClientClick="return confirm('Are you sure you want to disable this license?');" CssClass="actionButton" /> <asp:LinkButton runat="server" ID="LNK_Download" Text="Download" CssClass="downloadLink" /></td>
        </tr>
    </tbody>
</ItemTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
</asp:repeater>

