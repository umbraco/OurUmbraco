<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumSpamCleaner.ascx.cs" Inherits="uForum.usercontrols.ForumSpamCleaner" %>

<div>
    These forum comments have been automatically marked as spam:<br />
    <asp:GridView ID="GridViewSpamComment" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" ShowFooter="True" AllowPaging="True" PageSize="10" PageIndexChanging="GridViewSpamComment_PageIndexChanging"> 
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
            <asp:TemplateField HeaderText="-" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:LinkButton ID="NotSpamComment" runat="server" CommandArgument='<%# Bind("id") %>' OnCommand="NotSpamComment">not spam</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns> 
        </asp:GridView> 
        These forum topics have been automatically marked as spam:<br />

        <asp:GridView ID="GridViewSpamTopic" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" ShowFooter="True" AllowPaging="True" PageSize="10" PageIndexChanging="GridViewSpamTopic_PageIndexChanging"> 
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
            <asp:TemplateField HeaderText="-" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:LinkButton ID="NotSpamTopic" runat="server" CommandArgument='<%# Bind("id") %>' OnCommand="NotSpamTopic">not spam</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns> 
        </asp:GridView> 
</div>

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
