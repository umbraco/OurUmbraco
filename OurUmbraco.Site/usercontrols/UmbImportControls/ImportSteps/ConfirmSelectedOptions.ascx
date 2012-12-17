<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmSelectedOptions.ascx.cs" Inherits="UmbImport.ImportSteps.ConfirmSelectedOptions" %>
<br />
<h4><asp:Literal ID="ConfirmIntroTextLiteral" runat="server" /></h4>
<table class="PropertyPane" width="98%">
<tr><td width="180"><asp:Literal ID="ConfirmDataSourceTypeLiteral" runat="server" /></td><td><asp:Literal ID="DataSourceTypeValueLiteral" runat="server" /></td></tr>
<asp:PlaceHolder ID="DatasourcePlaceholder" runat="server"><tr><td width="180"><asp:Literal ID="ConfirmDataSourceLiteral" runat="server" /></td><td><asp:Literal ID="DataSourceValueLiteral" runat="server" /></td></tr></asp:PlaceHolder>
<asp:PlaceHolder ID="DataCommandPlaceholder" runat="server"><tr><td width="180"><asp:Literal ID="ConfirmDataCommandLiteral" runat="server" /></td><td><asp:Literal ID="DataCommandValueLiteral" runat="server" /></td></tr></asp:PlaceHolder>
<asp:PlaceHolder ID="AdditionalParametersPlaceholder" runat="server"><tr><td width="180" valign="top"><asp:Literal ID="ConfirmAdditionalParametersLiteral" runat="server" /></td>
<td>
<asp:Repeater ID="DatasourceOptionsRepeater" runat="server" >
<ItemTemplate>
<%#Eval("key") %> = <%#Eval("value") %><br />
</ItemTemplate>
</asp:Repeater>
</td>
</tr>
</asp:PlaceHolder>
<asp:PlaceHolder ID="ContentSpecificOptions" runat="server" Visible="false">
<tr><td width="180"><asp:Literal ID="ConfirmDocumentLocationLiteral" runat="server" /></td><td><asp:Literal ID="DocumentLocationValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmDocumentTypeLiteral" runat="server" /></td><td><asp:Literal ID="DocumentTypeValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmAutoPublishLiteral" runat="server" /></td><td><asp:Literal ID="AutoPublishValueLiteral" runat="server" /></td></tr>
</asp:PlaceHolder>
<asp:PlaceHolder ID="MemberSpecificOptions" runat="server" Visible="false">
<tr><td width="180"><asp:Literal ID="ConfirmMembertypeLiteral" runat="server" /></td><td><asp:Literal ID="ConfirmMembertypeValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmSelectedMemberRolesLiteral" runat="server" /></td><td><asp:Literal ID="ConfirmSelectedMemberRoleValuesLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmActionWhenMemberExistsLiteral" runat="server" /></td><td><asp:Literal ID="ConfirmActionWhenMemberExistsValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmAutogeneratepasswordLiteral" runat="server" /></td><td><asp:Literal ID="ConfirmAutogeneratepasswordValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ConfirmSendUserCredentialsViaMailLiteral" runat="server" /></td><td><asp:Literal ID="ConfirmSendUserCredentialsViaMailValueLiteral" runat="server" /></td></tr>
</asp:PlaceHolder>
</table>
<h4><asp:Literal ID="ConfirmMappingLiteral" runat="server" /></h4>
<table class="PropertyPane" width="98%">
<tr><td width="180"><strong><asp:Literal ID="MapDocumentPropertyLiteral" runat="server" /></strong></td><td><strong><asp:Literal ID="MapDatabaseColumnLiteral" runat="server" /></strong></td></tr>
<asp:Repeater ID="MappingRepeater" runat="server" >
<ItemTemplate>
<tr><td width="180"><%#Eval("key") %></td><td><%#Eval("value") %></td></tr>
</ItemTemplate>
</asp:Repeater>
</table>




