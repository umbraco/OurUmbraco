<%@ Control Language="c#" AutoEventWireup="false" Codebehind="umbLogin.ascx.cs" Inherits="umbracoMemberControls.umbGroupSignInOut" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:Panel id="PanelSignIn" runat="server">
	<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="300" border="0">
		<TR>
			<TD>
				<asp:Literal id="LiteralTitle" runat="server" Text="Username"></asp:Literal></TD>
			<TD>
				<asp:TextBox id="TextBoxUserName" runat="server" Width="104px"></asp:TextBox></TD>
		</TR>
		<TR>
			<TD>Password</TD>
			<TD>
				<asp:TextBox id="TextBoxPassword" runat="server" Width="104px" TextMode="Password"></asp:TextBox></TD>
		</TR>
		<TR>
			<TD></TD>
			<TD>
				<asp:Button id="ButtonForumCreate" runat="server" Text="Login" CssClass="umbGroupButton"></asp:Button></TD>
		</TR>
	</TABLE>
</asp:Panel>
<asp:Panel id="PanelSignOut" runat="server">
	<asp:Literal id="LiteralLogout" runat="server"></asp:Literal>
	<asp:LinkButton id="LinkButtonLogout" runat="server">
		<asp:Literal ID="signoutText" Runat="server">Sign Out</asp:Literal>
	</asp:LinkButton>
</asp:Panel>
