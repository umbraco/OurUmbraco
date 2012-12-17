<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Forgotpassword.ascx.cs" Inherits="our.usercontrols.Forgotpassword" %>
<asp:Literal ID="msg" runat="server" Visible="false"></asp:Literal>


<asp:panel runat="server" defaultbutton="bt_login">

<div class="form simpleForm" id="registrationForm">
<fieldset>
  <asp:Literal ID="lt_err" runat="server" Visible="false" />
  
  <p>
    <asp:label ID="Label2" AssociatedControlID="tb_email" CssClass="inputLabel"  runat="server">Email</asp:label>
    <asp:TextBox ID="tb_email" runat="server"  ToolTip="Please enter your email address" CssClass="required email title"/>
  </p>
  
</fieldset>

<div class="buttons">
<asp:Button ID="bt_login" onclick="sendPass" CssClass="submitButton" runat="server" Text="Retrieve password" />
</div>

</div>

<script type="text/javascript">
  $(document).ready(function() {
      $("form").validate();
  });
</script>
</asp:panel>