<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master" CodeBehind="extractRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.extractRevision" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


<umb:UmbracoPanel ID="panel" Text="Extract revision" runat="server" hasMenu="true">


<umb:Pane runat="server" Text="Details" id="paneResources">

    <umb:PropertyPanel ID="PropertyPanel3" runat="server" Text="Resources">
       
        <asp:Literal runat="server" ID="resourceCount"></asp:Literal>

    </umb:PropertyPanel>

    <umb:PropertyPanel ID="PropertyPanel4" runat="server" Text="Revision items">
        <asp:Literal runat="server" ID="revisionCount"></asp:Literal>
    </umb:PropertyPanel>

    <br />
    
    <p>
        <asp:Literal runat="server" ID="extractInfo" ></asp:Literal>
    </p>

</umb:Pane>

<umb:Pane ID="p_compare" Text="Compare with current umbraco instance" runat="server" Visible="false">
    <umb:PropertyPanel ID="PropertyPanel1" runat="server" Text="">
       
       <asp:Repeater id="rp_providers" runat="server" OnItemDataBound="bindProvider">
       <ItemTemplate>
       <h2><asp:Literal ID="lt_name" runat="server" /></h2>
       <asp:Repeater ID="rp_changes" runat="server" OnItemDataBound="bindChanges">
        <HeaderTemplate>
            <table>
            <tr>
                <th>Item</th><th>Description</th><th>Transfer</th>
            </tr>
        </HeaderTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
        <ItemTemplate>
            <tr>
                <td><asp:Literal ID="lt_item" runat="server" /></td>
                <td><asp:Literal ID="lt_error" runat="server" /></td>
                <td><asp:CheckBox ID="cb_transfer" Checked="true" runat="server" /></td>
                <asp:HiddenField ID="hf_key" runat="server" />
            </tr>
        </ItemTemplate>
       </asp:Repeater>
       </ItemTemplate>
       </asp:Repeater>

    </umb:PropertyPanel>
</umb:Pane>


<umb:Pane ID="p_extract" Text="Extract snapshot to server" runat="server" Visible="false">
    <umb:PropertyPanel ID="PropertyPanel2" runat="server" Text="Extraction history">
        <ul>
            <asp:Literal ID="e_list" runat="server" />
        </ul>
    </umb:PropertyPanel>
</umb:Pane>

</umb:UmbracoPanel>

</asp:Content>
