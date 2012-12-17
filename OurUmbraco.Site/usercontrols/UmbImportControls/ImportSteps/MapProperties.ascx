<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MapProperties.ascx.cs" Inherits="UmbImport.ImportSteps.MapProperties" %>
<%@ Register Assembly="UmbImport" Namespace="UmbImport.BaseTypes" TagPrefix="umbImport" %>

<!--generic props go here-->

<strong><asp:Literal ID="MapGenericContentPropertiesLiteral" runat="server" /></strong>
<br />
<asp:PlaceHolder ID="MapGenericContentPropertiesPlaceHolder" runat="server" Visible="false">
<table width="98%" class="propertypane" >
<tr><td width="180"><asp:Literal  ID="MapNamePropertyLiteral" runat="server" /></td><td> <umbImport:CommandDropdownlist ID="NodeNameDatasourceDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='nodeName'    ondatabinding="ColumnDropdown_DataBinding" OnDataBound="ColumnDropdown_DataBound"  runat="server" /></td></tr>
<tr><td width="180"><asp:Literal  ID="MapPublishPropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="PublishDateDatasourceDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='publishDate'    ondatabinding="ColumnDropdown_DataBinding"  OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="MapUnPublishPropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="UnPublishDateDatasourceDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='expireDate'    ondatabinding="ColumnDropdown_DataBinding"  OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr>
</table>
</asp:PlaceHolder>
<asp:PlaceHolder ID="MapGenericMemberPropertiesPlaceHolder" runat="server" Visible="false">
<table width="98%" class="propertypane" >
<tr><td width="180"><asp:Literal  ID="NamePropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="NamePropertyDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='name'    ondatabinding="ColumnDropdown_DataBinding"  OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal  ID="LoginPropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="LoginPropertyDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='login'    ondatabinding="ColumnDropdown_DataBinding"  OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr>
<asp:PlaceHolder ID="PasswordVisiblePlaceHolder" runat="server"><tr><td width="180"><asp:Literal ID="PasswordPropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="PasswordPropertyDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='password'    ondatabinding="ColumnDropdown_DataBinding" OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr></asp:PlaceHolder>
<tr><td width="180"><asp:Literal ID="EmailPropertyLiteral" runat="server" /></td><td><umbImport:CommandDropdownlist ID="EmailPropertyDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='email'    ondatabinding="ColumnDropdown_DataBinding" OnDataBound="ColumnDropdown_DataBound" runat="server" /></td></tr>
</table>
</asp:PlaceHolder>
<br /><br />
<!--Dynamic Data-->
<table width="98%" class="propertypane" >
<tr>
<td><asp:Literal ID="MapDocumentPropertyLiteral" runat="server" /></td><td><asp:Literal ID="MapDatabaseColumnLiteral" runat="server" /></td>
</tr>

<asp:Repeater ID="dtRepeater" runat="server">

<ItemTemplate>

<tr><td width="180"><%# DataBinder.Eval(Container.DataItem, "Name")%></td><td>
    <umbImport:CommandDropdownlist ID="datasourceDropdown" OnSelectedIndexChanged="ColumnDropdown_SelectedIndexChanged" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "Alias") %>'    ondatabinding="ColumnDropdown_DataBinding" OnDataBound="ColumnDropdown_DataBound" runat="server" />
</td></tr>
</ItemTemplate>
</asp:Repeater>
</table>