<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectContributors.ascx.cs" Inherits="our.usercontrols.ProjectContributors" %>

<asp:Panel ID="holder" runat="server" Visible="false">



<div class="form simpleForm">
<fieldset>
<legend>Add new contributor</legend>
<p>Add a new contributor by email address (contributor needs to be registered)</p>

<div id="error" class="alert" runat="server" visible="false">
 Member not found
</div>

<div id="confirm" class="confirm" runat="server" visible="false">
 New contributor added
</div>

<p>
    <asp:Label ID="lbl_email" runat="server" Text="Email" AssociatedControlID="tb_email" CssClass="inputLabel"></asp:Label>
    <asp:TextBox ID="tb_email" runat="server" ToolTip="Please enter email address of the contributor you wish to add" CssClass="required title"></asp:TextBox>
    
</p>
</fieldset>

<div class="buttons">

  <asp:Button ID="bt_submit" Text="Add" CssClass="submitButton" runat="server" 
        onclick="bt_submit_Click" />
  
</div>

</div>


<script type="text/javascript">

    function RemoveContributor(obj, projectid, memberid) {
        $.get("/base/projects/RemoveContributor/" + projectid + "/" + memberid + ".aspx");
        obj.parent().hide("slow");
    }
 
  $(document).ready(function() {
    $("form").validate();

    $(".removeContributor").click(function() {
      RemoveContributor($(this), <%= Request["id"] %>, $(this).attr("rel"));
     });
        
  });

  
  
  
</script>


</asp:Panel>


