<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorProjectBI.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Dashboard.VendorProjectBI" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<h2>Stats for <asp:Literal runat="server" ID="ProjectName" /></h2>
<div class="statsOverview" >
    <div class="statsBox green">
        <div class="stat"><span><asp:Literal runat="server" ID="RevenueOverview" /></span></div>
        <div class="statType"><span>Revenue</span></div>
    </div>

    <div class="statsBox red">
        <div class="stat"><span><asp:Literal runat="server" ID="RefundOverview" /></span></div>
        <div class="statType"><span>Refunded</span></div>
    </div>
</div>

<h2>Usage</h2>
<div class="statsOverview">
    <div class="statsBoxLine">
        <h3>Page Views Last 7 Days</h3>
        <div id="pageViewsChart"></div>
    </div>

</div>
<div class="statsOverview" >

<div class="statsBox">
    <div class="stat"><span><asp:Literal runat="server" ID="PageViewOverview" /></span></div>
    <div class="statType"><span>Page Views</span></div>
</div>

<div class="statsBox">
    <div class="stat"><span><asp:Literal runat="server" ID="DownloadsOverview" /></span></div>
    <div class="statType"><span>Downloads</span></div>
</div>

<div class="statsBox">
    <div class="stat"><span><asp:Literal runat="server" ID="PurchasesOverview" /></span></div>
    <div class="statType"><span>Purchased</span></div>
</div>


</div>
<h2>Licenses</h2>
<div class="statsOverview" >
<asp:Repeater runat="server" ID="LicensesSold" OnItemDataBound="repeater_ItemDataBound">
<ItemTemplate>
   <div class="statsBox tall">
    <div class="holder50">
    <div class="stat50"><span><%# (Eval("LicensesSold") as IEnumerable<IMemberLicense>).Count().ToString()%></span><br /><small>Sold</small></div>
    <div class="stat50"><span><%# (Eval("LicensesSold") as IEnumerable<IMemberLicense>).Where(x => x.IsActive).Count().ToString()%></span><br /><small>Configured</small></div>
    </div> 
    <div class="statType"><span><%# Eval("TypeSold")%></span></div>
    <div class="stat"><span><asp:Literal runat="server" ID="DetailItemRevenue" /></span></div>
   </div>
</ItemTemplate>
</asp:Repeater>
</div>

<h2>Orders</h2>
<asp:Repeater runat="server" ID="OrdersRepeater" OnItemDataBound="repeater_ItemDataBound">
<HeaderTemplate>
<table class="dataTable">
<thead>
<tr>
    <th>Order Id.</th>
    <th>License Type</th>
    <th>Member</th>
    <th>Date</th>
    <th class="center">Quantity</th>
    <th class="center">Refunded</th>
    <th class="money">Order Value</th>
</tr>
</thead>
</HeaderTemplate>
<ItemTemplate>
<tr class="<%# Eval("CssClass") %>">
    <td><%# ((IOrder)Eval("Order")).OrderId%></td>
    <td><%# Eval("LicenseSold")%></td>
    <td><%# ((IOrder)Eval("Order")).Member.Name %></td>
    <td><%# ((IOrder)Eval("Order")).CreateDate.ToString("D")%></td>
    <td class="center"><%# Eval("Quantity")%></td>
    <td class="center"><%# Eval("Refunded")%></td>
    <td class="money"><asp:Literal runat="server" ID="DetailItemRevenue" /></td>
</tr>
</ItemTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
</asp:Repeater>

<script type="text/javascript">
    
    function drawCharts() {
        // Create and populate the data table.

        var data = new google.visualization.DataTable();
        <%= PageViewsScript %>
        new google.visualization.LineChart(document.getElementById('pageViewsChart')).draw(data,{legend:"none", width:490,height:200});

    }

    google.setOnLoadCallback(drawCharts);
        
</script>