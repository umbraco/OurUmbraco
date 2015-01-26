<%@ Control Language="C#" AutoEventWireup="true" Inherits="usercontrols.Umbraco.ExamineIndexAdmin" Codebehind="ExamineIndexAdmin.ascx.cs" %>
<asp:Repeater ID="indexManager" runat="server">
        <HeaderTemplate>Index manager<br /></HeaderTemplate>
        <ItemTemplate>
            <%#Eval("Name")%>&nbsp;<asp:Button ID="rebuild" CommandArgument='<%#Eval("Name")%>' Text="Rebuild" runat="server" OnClick="RebuildClick" />
        </ItemTemplate>
        <SeparatorTemplate><br /></SeparatorTemplate>
</asp:Repeater>
<asp:Label ID="result" runat="server" Visible="false"></asp:Label>
