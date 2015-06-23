<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumSpamListComments.ascx.cs" Inherits="uForum.usercontrols.ForumSpamListComments" %>

<div>
    These forum comments have been automatically marked as spam:<br />
    <asp:GridView ID="GridViewSpamComment" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" ShowFooter="True" AllowPaging="True" PageSize="10" PageIndexChanging="GridViewSpamComment_PageIndexChanging"> 
        <Columns> 
            <asp:TemplateField HeaderText="Id"  HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="MemberId" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblName" runat="server" Text='<%# Bind("memberId") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="Body" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblBody" runat="server" Text='<%# Bind("body") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="Created" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblCreated" runat="server" Text='<%# Bind("created") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="-" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:LinkButton ID="NotSpamComment" runat="server" CommandArgument='<%# Bind("id") %>' OnCommand="NotSpamComment">not spam</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns> 
        </asp:GridView> 
</div>
