<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MicroCart.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Cart.MicroCart" %>
<asp:placeholder runat="server" ID="microCartHolder">
<span class="microCart"><a href="/Deli/Cart">View Cart (<asp:Literal runat="server" ID="NumItems" />)</a>&nbsp;<asp:Literal runat="server" ID="CartTotal" /></span>&nbsp;&nbsp;
</asp:placeholder>