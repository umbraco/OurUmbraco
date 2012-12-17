<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="our.usercontrols.Login" %>

<asp:panel runat="server" defaultbutton="bt_login">
<div class="form simpleForm" id="registrationForm">
  <fieldset>
    <asp:Literal ID="lt_err" runat="server" Visible="false" />
    <p>
      <asp:label ID="Label2" AssociatedControlID="tb_email" CssClass="inputLabel"  runat="server">Email</asp:label>
      <asp:TextBox ID="tb_email" runat="server"  ToolTip="Please enter your email address" CssClass="required email title"/>
    </p>
    <p>
      <asp:label ID="Label3" AssociatedControlID="tb_password" CssClass="inputLabel" runat="server">Password</asp:label>
      <asp:TextBox ID="tb_password" runat="server" ToolTip="Please enter your password" TextMode="Password" CssClass="password title"/>
    </p>
  </fieldset>
  <div class="buttons">
  <asp:Button ID="bt_login" onclick="login" CssClass="submitButton" runat="server" Text="Login" />
  </div>
</div>

<script type="text/javascript">
  $(document).ready(function() {
    jQuery.validator.addClassRules({
      password: {
        required: true,
        minlength: 5
      }
    });
    $("form").validate();
  });
</script>
</asp:panel>