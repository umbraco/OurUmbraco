<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumSpamListTopics.ascx.cs" Inherits="uForum.usercontrols.ForumSpamListTopics" %>

<div>
        These forum topics have been automatically marked as spam:<br />

        <asp:GridView ID="GridViewSpamTopic" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" ShowFooter="True" AllowPaging="True" PageSize="10" PageIndexChanging="GridViewSpamTopic_PageIndexChanging"> 
        <Columns> 
            <asp:TemplateField HeaderText="TopicId"  HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label> 
                </ItemTemplate> 
            </asp:TemplateField> 
            <asp:TemplateField HeaderText="MemberId" HeaderStyle-HorizontalAlign="Left"> 
                <ItemTemplate> 
                    <asp:Label ID="lblMemberId" runat="server" Text='<%# Bind("memberId") %>'></asp:Label> 
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
                    <asp:LinkButton ID="NotSpamTopic" runat="server" CommandArgument='<%# Bind("id") %>' OnCommand="NotSpamTopic">not spam</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns> 
        </asp:GridView> 
</div>

