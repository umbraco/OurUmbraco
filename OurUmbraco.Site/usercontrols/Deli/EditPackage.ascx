<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditPackage.ascx.cs" Inherits="Marketplace.usercontrols.Deli.EditPackage" %>
<fieldset>
    <asp:Label runat="server" AssociatedControlID="Title">Title</asp:Label>
    <asp:TextBox runat="server" ID="Title" />
    <asp:RequiredFieldValidator runat="server" ControlToValidate="Title" ErrorMessage="<span>Required</span>" Display="Dynamic" CssClass="Error" />

    <asp:dropdownlist runat="server" ID="Category" />

    <asp:Label runat="server" AssociatedControlID="Description">Description</asp:Label>
    <asp:TextBox runat="server" ID="Description" TextMode="MultiLine" />
    <asp:RequiredFieldValidator runat="server" ControlToValidate="Description" ErrorMessage="<span>Required</span>" Display="Dynamic" CssClass="Error" />

    <asp:Label runat="server" AssociatedControlID="Tags">Tags</asp:Label>
    <asp:TextBox runat="server" ID="Tags" />

    <asp:Label runat="server" AssociatedControlID="Currency">Currency</asp:Label>
    <asp:dropdownlist runat="server" ID="Currency" />

    <asp:Label runat="server" AssociatedControlID="Logo">Logo</asp:Label>
    <asp:FileUpload runat="server" ID="Logo" />

    <asp:Label runat="server" AssociatedControlID="SupportUrl">Support Url</asp:Label>
    <asp:TextBox runat="server" ID="SupportUrl" />
    <asp:RequiredFieldValidator runat="server" ControlToValidate="SupportUrl" ErrorMessage="<span>Required</span>" Display="Dynamic" CssClass="Error" />

    <asp:Label runat="server" AssociatedControlID="VendorUrl">Vendor Url</asp:Label>
    <asp:TextBox runat="server" ID="VendorUrl" />
    <asp:RequiredFieldValidator runat="server" ControlToValidate="VendorUrl" ErrorMessage="<span>Required</span>" Display="Dynamic" CssClass="Error" />
</fieldset>

<fieldset>
    <asp:Label runat="server" AssociatedControlID="PackageFile">Package File</asp:Label>
    <asp:FileUpload runat="server" ID="PackageFile" />

    <asp:Label runat="server" AssociatedControlID="DocumentationFile">Documentation File</asp:Label>
    <asp:FileUpload runat="server" ID="DocumentationFile" />
</fieldset>

<fieldset>
    <asp:Label runat="server" AssociatedControlID="UmbracoVersions">Umbraco Versions Supported</asp:Label>
    <asp:ListBox runat="server" ID="UmbracoVersions">
        <asp:ListItem Text="4.0" Value="4.0" />
        <asp:ListItem Text="4.5" Value="4.5" />
        <asp:ListItem Text="4.6" Value="4.6" />
        <asp:ListItem Text="4.7" Value="4.7" />
        <asp:ListItem Text="5.0" Value="5.0" />
    </asp:ListBox>

    <asp:Label runat="server" AssociatedControlID="DotNetVersions">.NET Runtime Supported</asp:Label>
    <asp:ListBox runat="server" ID="DotNetVersions">
        <asp:ListItem Text="2.0" Value="2.0" />
        <asp:ListItem Text="3.5" Value="3.5" />
        <asp:ListItem Text="4.0" Value="4.0" />
    </asp:ListBox>

    <asp:Label runat="server" AssociatedControlID="TrustLevel">Trust Level Required</asp:Label>
    <asp:RadioButton Text="Full" runat="server" ID="FullTrust" GroupName="Trust" />
    <asp:RadioButton Text="Medium" runat="server" ID="MediumTrust" GroupName="Trust" />

    <asp:Checkbox runat="server" ID="Terms" Text="I have read and agree to the terms" />
    <a href="#">Read the Terms here</a>
</fieldset>

<asp:Button runat="server" ID="CreatePackage" Text="Create Package Listing" />