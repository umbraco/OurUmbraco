<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectMembertype.ascx.cs" Inherits="UmbImport.UmbImportControls.ImportSteps.MemberImport.SelectMembertype" %>
<table width="98%" class="propertypane" >
<tr><td width="180" valign="top"><asp:Literal ID="ImportMemberTypeLiteral" runat="server"/></td><td><asp:DropDownList ID="MemberTypeDropdown" runat="server"/></td></tr>
<tr><td width="180" valign="top"><asp:Literal ID="ImportAssignRoleLiteral" runat="server"/></td><td><asp:CheckboxList ID="AssignRoleList" runat="server" /></td></tr>
</table>
<table width="98%" class="propertypane" >
<tr><td width="180" valign="top"><asp:Literal ID="ActionWhenMemberExistsLiteral" runat="server"/></td><td><asp:DropDownList ID="ActionWhenMemberExistsDropdown" runat="server"/></td></tr>
<tr><td width="180" valign="top"><asp:Literal ID="AutogeneratepasswordLiteral" runat="server"/></td><td><asp:CheckBox ID="AutogeneratepasswordCheckbox" runat="server"/></td></tr>
<tr><td width="180" valign="top"><asp:Literal ID="SendUserCredentialsViaMailLiteral" runat="server"/></td><td><asp:CheckBox ID="SendUserCredentialsViaMailCheckbox" runat="server"/></td></tr>
</table>