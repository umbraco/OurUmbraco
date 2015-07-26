<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SignupSimple.ascx.cs" Inherits="OurUmbraco.Our.usercontrols.SignupSimple" %>


<asp:panel ID="Panel1" runat="server" defaultbutton="bt_submit">
<div class="form simpleForm" id="registrationForm">
<fieldset>
<legend>Basic Information</legend>
  <p>
  We just need the most basic information from you.
  </p>
  <p>
    <asp:label ID="Label1" AssociatedControlID="tb_name" CssClass="inputLabel" runat="server">Name</asp:label>
    <asp:TextBox ID="tb_name" runat="server" ToolTip="Please enter your name" CssClass="required title"/>
  </p>
  <p>
    <asp:label ID="Label3" AssociatedControlID="tb_email" CssClass="inputLabel"  runat="server">Email</asp:label>
    <asp:TextBox ID="tb_email" runat="server" onBlur="lookupEmail(this);" ToolTip="Please enter your email address" CssClass="required email title"/>
  </p>
  <p>
    <asp:label ID="Label4" AssociatedControlID="tb_password" CssClass="inputLabel" runat="server">Password</asp:label>
    <asp:TextBox ID="tb_password" runat="server" ToolTip="Please enter a password, minimum 5 characters" TextMode="Password" CssClass="password title"/>
  </p>
</fieldset>



<div class="buttons">
<asp:Button ID="bt_submit" Text="Sign up" CssClass="submitButton" OnClick="createMember" runat="server" />
</div>

</div>

</asp:panel>

<script type="text/javascript">

    $(document).ready(function () {

        jQuery.validator.addClassRules({
            password: {
                required: true,
                minlength: 5
            }
        });

        $("form").validate();
    });
</script>