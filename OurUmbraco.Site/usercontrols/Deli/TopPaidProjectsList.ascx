<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopPaidProjectsList.ascx.cs" Inherits="Marketplace.usercontrols.Deli.TopPaidProjectsList" %>

<asp:Repeater runat="server" ID="TopRepeater">
<HeaderTemplate><ul></HeaderTemplate>
<ItemTemplate><li><a href="<%# Eval("NiceUrl") %>"><%# Eval("Name") %></a><br />
<%--<small><%# Marketplace.library.GetManufacturerName(((Marketplace.Interfaces.IVendor)Eval("Vendor"))) %></small>--%>
</li></ItemTemplate>
<FooterTemplate></ul></FooterTemplate>
</asp:Repeater>
