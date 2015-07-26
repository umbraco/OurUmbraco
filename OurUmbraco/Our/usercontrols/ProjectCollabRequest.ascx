<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectCollabRequest.ascx.cs" Inherits="our.usercontrols.ProjectCollabRequest" %>

<h1><asp:Literal runat="server" ID="lt_title" Text="Create project"></asp:Literal> collaboration request</h1>

<asp:Panel ID="projectCollabForm" runat="server">

<div class="form simpleForm" id="registrationForm">
<fieldset>
<legend>Message to project owner</legend>

 <p>
    <asp:TextBox ID="tb_message" runat="server" style="width: 600px; height: 500px;" TextMode="MultiLine" CssClass="required" />
</p>


</fieldset>

<div class="buttons">
  <asp:Button ID="bt_submit" Text="Send request" CssClass="submitButton"  
        runat="server" onclick="bt_submit_Click" />
</div>

</div>

<script type="text/javascript">

    $(document).ready(function () {
        $("form").validate();
    });

</script>

</asp:Panel>



