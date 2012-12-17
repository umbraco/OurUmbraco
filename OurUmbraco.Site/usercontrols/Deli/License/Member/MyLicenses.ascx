<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyLicenses.ascx.cs" Inherits="Marketplace.usercontrols.Deli.License.Member.MyLicenses" %>
<h2>
Manage your licenses
</h2>

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
<asp:TextBox runat="server" ID="Configure_DevConfig" CssClass='required title' />
</p>
<p>
<asp:Label ID="Label2" runat="server" AssociatedControlID="Configure_StagingConfig" Text="Staging" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Configure_StagingConfig" CssClass='title' />
</p>
<p>
<asp:Label ID="Label3" runat="server" AssociatedControlID="Configure_ProductionConfig" Text="Production" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="Configure_ProductionConfig" CssClass='required title' />
</p>
<small>(Hint: use the root of your domain. &quot;mydomain.com&quot; instead of &quot;www.mydomain.com&quot; as this will allow the license to be used on all subdomains also)</small>
</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="UnlimitedConfig" Visible="false">
<p>This license requires no configuration as it is an Unlimited License</p>
</asp:PlaceHolder>
<div class="buttons"><asp:Button runat="server" ID="bt_Configure" cssclass="submitButton" Text="Generate License" OnClick="UpdateLicense_Click" /></div>
</fieldset>


</div>
<script type="text/javascript">


    $(document).ready(function () {
        var form = $("form");

        $.validator.addMethod("onlyIpAddress",
            function (value, element) {
                var re = new RegExp('^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$');
                if (value != '')
                    return (value.match(re));
                else
                    return true;
            },
            "please enter a valid IP address"
        );

        $.validator.addMethod("onlyDomainName",
            function (value, element) {
                var re = new RegExp('^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$');
                if (value != '')
                    return (value.match(re));
                else
                    return true;
            },
            "please enter a valid domain without http:// or trailing /"
        );

        $.validator.addClassRules({
            onlyIpAddress: { onlyIpAddress: true },
            onlyDomainName: { onlyDomainName: true }
        });

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
            <th>Product</th>
            <th>License Type</th>
            <th>Configured For</th>
            <th class="center">License File</th>
        <tr>
    </thead>
</HeaderTemplate>
<ItemTemplate>
    <tbody>
        <tr>
            <td>
                <%#((Marketplace.Interfaces.IListingItem)Eval("Product")).Name %>
                <small><br /><%#((Marketplace.Interfaces.IListingItem)Eval("Product")).Vendor.VendorCompanyName %></small>
            </td>
            <td>
                <%# Eval("LicType") %>
            </td>
            <td><small>
                <strong>Dev:</strong> <%# Eval("DevConfig") %><br />
                <strong>Staging:</strong> <%# Eval("StagingConfig") %><br />
                <strong>Production:</strong> <%# Eval("ProductionConfig")%>
                </small>
            </td>
            <td class="right"><asp:Button ID="BT_Action" runat="server" CssClass="actionButton" /> <asp:LinkButton runat="server" ID="LNK_Download" Text="Download" CssClass="downloadLink" /></td>
        </tr>
    </tbody>
</ItemTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
</asp:repeater>

