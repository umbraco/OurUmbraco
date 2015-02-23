<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CartProgress.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Cart.CartProgress" %>
<div class="cartProgress">
<ul>
<asp:repeater runat="server" ID="ProgressIndicator">
<ItemTemplate>
    <li class="<%#Eval("StepClass") %>"><%# Eval("StepName") %></li>
</ItemTemplate>
</asp:repeater>
</ul>
</div>