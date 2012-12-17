<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectUmbracoTypeAndLocation.ascx.cs" Inherits="UmbImport.ImportSteps.SelectUmbracoTypeAndLocation" %>
<table width="98%" class="propertypane" >
<tr><td width="180"><asp:Literal ID="ImportLocationLiteral" runat="server"/></td><td><asp:PlaceHolder ID="PagePickerHolder" runat="server"></asp:PlaceHolder></td></tr>
<tr><td width="180"><asp:Literal ID="ImportDocumentTypeLiteral" runat="server"/></td><td><asp:DropDownList ID="DocumentTypeDropdown" runat="server"/></td></tr>
<tr><td width="180"><asp:Literal ID="ImportAutoPublishLiteral" runat="server"/></td><td><asp:CheckBox ID="AutoPublishCheckBox" runat="server" /></td></tr>
</table>