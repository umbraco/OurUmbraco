<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="acceptTos.ascx.cs" Inherits="our.usercontrols.acceptTos" %>
<asp:PlaceHolder ID="error" runat="server" Visible="false">
<div class="error">You need to accept the ToS!</div>
</asp:PlaceHolder>
<asp:checkbox id="fair" runat="server" Text="That's fair, I accept"></asp:checkbox> 
<asp:button id="cool" runat="server" Text="Now let me move on" 
    onclick="cool_Click"></asp:button>
