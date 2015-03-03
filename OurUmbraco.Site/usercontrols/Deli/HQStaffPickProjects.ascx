<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HQStaffPickProjects.ascx.cs" Inherits="Marketplace.usercontrols.Deli.HQStaffPickProjects" %>
<asp:Repeater runat="server" ID="Listing">
<HeaderTemplate><ul></HeaderTemplate>
<ItemTemplate><li><a href="<%# Eval("NiceUrl") %>"><%# Eval("Name") %></a><br />
<%--<small><%# uProject.library.GetManufacturerName(((Marketplace.Interfaces.IVendor)Eval("Vendor"))) %></small>--%></li>
</ItemTemplate>
<FooterTemplate></ul></FooterTemplate>
</asp:Repeater>