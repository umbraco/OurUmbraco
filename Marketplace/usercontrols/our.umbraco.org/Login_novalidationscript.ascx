<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="Login_novalidationscript.ascx.cs" Inherits="Marketplace.usercontrols.ourUmbraco.Login_novalidationscript" %>
<asp:panel ID="Panel1" runat="server" defaultbutton="bt_login">
<div class="form simpleForm" id="registrationForm">
  <fieldset>
    <asp:Literal ID="lt_err" runat="server" Visible="false" />
    <div>
      <asp:label ID="Label2" AssociatedControlID="tb_email" CssClass="inputLabel"  runat="server">Email</asp:label>
      <asp:TextBox ID="tb_email" runat="server"  ToolTip="Please enter your email address" CssClass="email title"/>
    </div>

    <div>
      <asp:label ID="Label3" AssociatedControlID="tb_password" CssClass="inputLabel" runat="server">Password</asp:label>
      <asp:TextBox ID="tb_password" runat="server" ToolTip="Please enter your password" TextMode="Password" CssClass="password title"/>
    </div>
  <div class="buttons">
  <asp:Button ID="bt_login" onclick="login" CssClass="cancel" runat="server" Text="Login" />
  </div>
</fieldset>
</div>
</asp:panel>