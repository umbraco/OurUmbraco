<%@ Control Language="c#" AutoEventWireup="false" Codebehind="umbPasswordForgot.ascx.cs" Inherits="umbracoMemberControls.PasswordForgot" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:panel id="error" Visible="False" Runat="server">
	<P class="umbForgotError">
		<asp:Literal id="mailError" Runat="server"></asp:Literal></P>
</asp:panel><asp:panel id="formular" Runat="server">
	<asp:TextBox id="EmailAddress" runat="server"></asp:TextBox>
	<asp:Button id="ok" Runat="server" Text=" OK "></asp:Button>
</asp:panel><asp:panel id="thankyou" Visible="False" Runat="server">
	<asp:Literal id="email" Runat="server"></asp:Literal>
</asp:panel>
