<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectDataSourceType.ascx.cs" Inherits="UmbImport.ImportSteps.SelectDataSourceType" %>
<asp:Panel ID="DataSourceTypePanel" runat="server">
<table>
<tr>
    <th width="180"><asp:Literal ID="SelectDataSourceTypeLiteral" runat="server"/></th>
    <td><asp:ListBox ID="DataSourceTypeList" runat="server" Rows="1"/></td>
</tr>
<tr>
    <th width="180"><asp:Literal ID="SelectImportAsLiteral" runat="server"/></th>
    <td><asp:ListBox ID="DataImportAsList" runat="server" Rows="1"/></td>
</tr>
</table>
</asp:Panel>
