<%@ Control Language="c#" AutoEventWireup="false" Codebehind="memberSearch.ascx.cs" Inherits="umbDashboard.memberSearch" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<h3 style="MARGIN-LEFT: 0px">Member Search</h3>
<P class="guiDialogNormal">
	<asp:TextBox id="TextBox1" runat="server" Width="216px"></asp:TextBox></P>
<P class="guiDialogNormal">
	<asp:CheckBox id="CheckBoxExtended" runat="server" Text="Extended search (slow)"></asp:CheckBox>
	<asp:Button id="ButtonSearch" runat="server" Text="Button"></asp:Button><BR>
	<BR>
</P>
<asp:DataGrid id="DataGrid1" runat="server" Visible="False" Width="100%">
	<AlternatingItemStyle BackColor="#E0E0E0"></AlternatingItemStyle>
	<HeaderStyle BackColor="Silver"></HeaderStyle>
</asp:DataGrid>
