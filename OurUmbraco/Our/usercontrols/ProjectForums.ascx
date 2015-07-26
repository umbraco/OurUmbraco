<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectForums.ascx.cs" Inherits="our.usercontrols.ProjectForums" %>

<asp:PlaceHolder ID="holder" runat="server" Visible="false">

<h3><a href="?id=<%= Request.QueryString["id"] %>&add=true" title="Manage Project Forums">Add new forum</a></h3>
<p>Click to add new forum to your project area. You are not forced to have forums on your project page, but would be a nice thing to do for your users and fellow umbracians.</p>

<asp:PlaceHolder ID="ph_add" runat="server" Visible="false">
<div class="form simpleForm" id="registrationForm">
<fieldset>
  <p>
    <asp:label ID="Label3" AssociatedControlID="tb_name" CssClass="inputLabel"  runat="server">Forum name</asp:label>
    <asp:TextBox ID="tb_name" runat="server" ToolTip="Name of forum is mandatory" CssClass="required title"/>
    <small>ex: "Bug reports, developer forum, etc"</small>
  </p>
  <p>
    <asp:label ID="Label1" AssociatedControlID="tb_desc" CssClass="inputLabel"  runat="server">Forum description</asp:label>
    <asp:TextBox ID="tb_desc" runat="server" TextMode="MultiLine" ToolTip="Forum description is mandatory" CssClass="required title"/>
    <small>ex: "If you find any bugs, please post them here so we can handle them"</small>
  </p>  
</fieldset>

<div class="buttons">
  <asp:Button ID="bt_submit" Text="Save" CssClass="submitButton" OnCommand="modifyForum" CommandName="create" runat="server" />
  
  <asp:PlaceHolder ID="ph_edit" runat="server" Visible="false">
    <em> or </em> <asp:LinkButton ID="bt_delete" OnCommand="modifyForum" CommandName="delete"  Text="Delete forum" OnClientClick="Return confirm('Are you sure you want to delete this forum?');"  runat="server" />
  </asp:PlaceHolder>

</div>

</asp:PlaceHolder>


<asp:Repeater ID="rp_forums" OnItemDataBound="bindForum" runat="server">
<ItemTemplate>
<tr>
  <th>
    <h3></h3><asp:Literal ID="lt_titel" runat="server" /></h3>
    <small><asp:Literal ID="lt_desc" runat="server" /></small>
  </th>
  <td>
    <asp:Literal ID="lt_link" runat="server" />
  </td>
</tr>
</ItemTemplate>

<HeaderTemplate>
<div id="forums">
<fieldset>
<legend>Existing forum</legend>
<table class="forumList">
<tbody>
</HeaderTemplate>
<FooterTemplate>
</tbody>
</table>
</fieldset>
</div>
</FooterTemplate>
</asp:Repeater>






</div>

<script type="text/javascript">

  $(document).ready(function() {
      $("form").validate();
  });

  
</script>
</asp:PlaceHolder>