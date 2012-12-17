<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="getMembersFromGroup.ascx.cs" Inherits="Umbraco.Shop.usercontrols.administration.Emails.getMembersFromGroup" %>
<%@ Register TagPrefix="cc" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc:Pane ID="pane_query" runat="server" Text="Get members in group">

<cc:PropertyPanel runat="server" Text="Select group">
<asp:DropDownList ID="dd_group" runat="server" OnSelectedIndexChanged="loadMembers" AutoPostBack="true">
    <asp:ListItem Value="">Select...</asp:ListItem>
</asp:DropDownList>
</cc:PropertyPanel>

<cc:PropertyPanel runat="server">
<asp:TextBox TextMode="MultiLine" ID="tb_emails" runat="server" />

<br />

<asp:BulletedList ID="lb_emails" runat="server" />

</cc:PropertyPanel>


</cc:Pane>