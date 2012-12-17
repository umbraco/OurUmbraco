<%@ Control Language="c#" AutoEventWireup="false" Codebehind="umbRegister.ascx.cs" Inherits="dguUmbracoControls.tilmelding" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:ValidationSummary id="ValidationSummary1" runat="server"></asp:ValidationSummary>
<asp:Literal id="additionalErrors" runat="server"></asp:Literal>
<table border="0" cellspacing="0" cellpadding="0">
	<tr>
		<td class="dguNormal" align="right"><b>Name</b></td>
		<td>&nbsp;&nbsp;</td>
		<td><asp:TextBox ID="name" Runat="server" CssClass="umbSignUpText" Width="200"></asp:TextBox>
			<asp:RequiredFieldValidator id="RequiredFieldValidator1" runat="server" ErrorMessage="Name is mandatory" ControlToValidate="name"
				Display="None"></asp:RequiredFieldValidator></td>
	<tr>
	<tr>
		<td class="dguNormal" align="right"><b>Email</b></td>
		<td>&nbsp;&nbsp;</td>
		<td><asp:TextBox ID="email" Runat="server" CssClass="umbSignUpText" Width="200"></asp:TextBox>
			<asp:RequiredFieldValidator id="RequiredFieldValidator2" runat="server" ErrorMessage="Email is mandatory" ControlToValidate="email"
				Display="None"></asp:RequiredFieldValidator></td>
	<tr>
	<tr>
		<td class="dguNormal" align="right"><b>Email Confirm</b></td>
		<td>&nbsp;&nbsp;</td>
		<td><asp:TextBox ID="emailConfirm" Runat="server" CssClass="umbSignUpText" Width="200"></asp:TextBox>
			<asp:CompareValidator id="CompareValidator1" runat="server" ErrorMessage="Email is different" ControlToCompare="email"
				ControlToValidate="emailConfirm" Display="None"></asp:CompareValidator></td>
	<tr>
	<tr>
		<td class="dguNormal" align="right"><b>Username</b></td>
		<td>&nbsp;&nbsp;</td>
		<td><asp:TextBox ID="username" Runat="server" CssClass="umbSignUpText" Width="100"></asp:TextBox>
			<asp:RequiredFieldValidator id="RequiredFieldValidator4" runat="server" ErrorMessage="Username is mandatory"
				ControlToValidate="username" Display="None"></asp:RequiredFieldValidator></td>
	<tr>
	<tr>
		<td class="dguNormal" align="right"><b>Password</b></td>
		<td>&nbsp;&nbsp;</td>
		<td><asp:TextBox ID="password" Runat="server" CssClass="umbSignUpText" Width="100" TextMode="Password"></asp:TextBox>
			<asp:RequiredFieldValidator id="RequiredFieldValidator5" runat="server" ErrorMessage="Password is mandatory"
				ControlToValidate="password" Display="None"></asp:RequiredFieldValidator></td>
	<tr>
	<tr>
		<td colspan="3" align="center"><br>
			<asp:Button id="ButtonSignup" runat="server" Text="Sign Up"></asp:Button></td>
	</tr>
</table>
