<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Importing.ascx.cs" Inherits="UmbImport.ImportSteps.Importing" %>
<br />
<asp:Literal ID="ImportFinishedMessageLiteral" runat="server" />
<br />
<table class="PropertyPane" width="98%">
<tr><td width="180"><asp:Literal ID="ImportRecordCountLiteral" runat="server" /></td><td><asp:Literal ID="ImportRecordCountValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ImportSuccesCountLiteral" runat="server" /></td><td><asp:Literal ID="ImportSuccesCountValueLiteral" runat="server" /></td></tr>
<tr><td width="180"><asp:Literal ID="ImportErrorCountLiteral" runat="server" /></td><td><asp:Literal ID="ImportErrorCountValueLiteral" runat="server" /></td></tr>
<asp:PlaceHolder ID="skippedPlaceholder" runat="server" Visible="false"><tr><td width="180"><asp:Literal ID="ImportSkippedCountLiteral" runat="server" /></td><td><asp:Literal ID="ImportSkippedCountValueLiteral" runat="server" /></td></tr></asp:PlaceHolder>
</table>
<asp:PlaceHolder ID="ErroPlaceholder" runat="server" Visible="false">
<div class="PropertyPane" width="98%">
<asp:Literal ID="ImportErrorHeaderLiteral" runat="server" /><br />
<asp:Repeater id="ImportErrorRepeater" runat="server">
<ItemTemplate><%#Eval("Message") %> <br /></ItemTemplate>
</asp:Repeater>
</div>
</asp:PlaceHolder>