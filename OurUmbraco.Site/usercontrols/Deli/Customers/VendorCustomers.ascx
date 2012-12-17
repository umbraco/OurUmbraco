<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorCustomers.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Customers.VendorCustomers" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<h2>Your Customers</h2>
<p>The following is a list of customers who purchased your products.</p>
<div><p><asp:LinkButton Text="Download as CSV" runat="server" OnClick="ExportToCSV" /></p></div>
<asp:Repeater runat="server" ID="CustomerList">
<HeaderTemplate>

<table class="dataTable">
<thead>
    <tr>
        <th></th>
        <th>Package</th>
        <th>License</th>
        <th>Purchaser</th>
        <th>Email</th>
        <th>Country</th>
        <th>Order Date</th>
    </tr>
</thead>
<tbody>
</HeaderTemplate>
<ItemTemplate>
    <tr>
        <td><%# (Container.ItemIndex + 1) %></td>
        <td class="packageName"><%# Eval("Package") %></td>
        <td><%# Eval("LicenseTypeName") %></td>
        <td><%# ((IMember)Eval("Member")).Name %></td>
        <td><%# ((IMember)Eval("Member")).Email %></td>
        <td><%# ((IMember)Eval("Member")).CompanyCountry %></td>
        <td><%# Eval("OrderDate") %></td>
    </tr>
</ItemTemplate>
<FooterTemplate>
</tbody>
</table>

</FooterTemplate>
</asp:Repeater>