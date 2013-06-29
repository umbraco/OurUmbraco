<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumSpamCleaner.ascx.cs" Inherits="uForum.usercontrols.ForumSpamCleaner" %>

<div>
    Delete forum comments belonging Member ID:
    <asp:TextBox runat="server" ID="memberId"></asp:TextBox>
    <asp:Button runat="server" ID="CleanSpam" Text="Kill the spammers comments" OnClick="CleanSpamClick" />
</div>
<asp:PlaceHolder runat="server" ID="AreYouSurePanel" Visible="False">
    <div>
        Are you sure you want to delete comments belonging to member:
        <asp:Literal runat="server" ID="MemberName"></asp:Literal>.
        <br />
        <br />
        <strong>This process is not reversible so make sure you get it right before you do it!!!!!!</strong>
        <br />
        <br />
        <asp:GridView ID="commentsGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" ShowFooter="True" OnRowDeleting="grdContact_RowDeleting"> 
        <Columns> 
            <asp:TemplateField HeaderText="ID"  HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="Name" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblName" runat="server" Text='<%# Bind("memberId") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="Body" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblBody" runat="server" Text='<%# Bind("body") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="Delete" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    
                    <a href="#" class="deleteComment" rel='<asp:literal runat="server" id="deleteId" text='<%# Bind("Id")%>' />'>delete</a>
                </ItemTemplate> 
            </asp:TemplateField>
        </Columns> 
        </asp:GridView> 
    </div>
</asp:PlaceHolder>

<script type="text/javascript">
    jQuery(document).ready(function () {
        $("a.deleteComment").click(function () {
            if (confirm("Do you really want to delete this comment?")) {
                var link = $(this);
                var commentId = link.attr("rel");
                var commentRow = link.closest('tr');
                $.get("/base/uForum/DeleteComment/" + commentId + ".aspx");
                commentRow.hide("slow");
            }

            return false;
        });
    });


</script>
