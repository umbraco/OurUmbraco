<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="VendorBIOverview.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Dashboard.VendorBIOverview" %>



<h2>Products YTD Overview</h2>
<div class="statsOverview" >
    <div class="statsBox green">
        <div class="stat"><span><asp:Literal runat="server" ID="RevenueOverview" /></span></div>
        <div class="statType"><span>Revenue</span></div>
    </div>

    <div class="statsBox red">
        <div class="stat"><span><asp:Literal runat="server" ID="RefundsOverview" /></span></div>
        <div class="statType"><span>Refunded</span></div>
    </div>
</div>
<div class="overviewCharts">
<div class="biChart">
<h3>Revenue</h3>
<div id="revenueChart"></div>
</div>

<div class="biChart">
<h3>Purchases</h3>
<div id="purchasesChart"></div>
</div>


<div class="biChart">
<h3>PageViews</h3>
<div id="pageViewsChart"></div>
</div>

<div class="biChart last">
<h3>Downloads</h3>
<div id="downloadsChart"></div>
</div>
</div>

    <table class="dataTable">
<asp:repeater runat="server" ID="rp_productRepeater" >
<HeaderTemplate>

        <thead>
            <tr>
            <th>Product</th>
            <th class="count">Page Views</th>
            <th class="count">Downloads</th>
            <th class="count">Purchases</th>
            <th class="money">Revenue</th>
            </tr>
        </thead>
        <tbody>
</HeaderTemplate>
<ItemTemplate>
        <tr>
            <td><a href="<%# Eval("DetailView") %>"><%# Eval("Name") %></a></td>
            <td class="count"><%# Eval("PageViews") %></td>
            <td class="count"><%# Eval("Downloads") %></td>
            <td class="count"><%# Eval("Purchases") %></td>
            <td class="money"><asp:Literal ID="DetailItemRevenue" runat="server" /></td>
        </tr>
</ItemTemplate>
<FooterTemplate>
        </tbody>
</FooterTemplate>
</asp:repeater>
        <tr class="totals">
            <td>Totals</td>
            <td class="count"><span><asp:literal runat="server" ID="PageViewTotal" /></span></td>
            <td class="count"><span><asp:literal runat="server" ID="DownloadsTotal" /></span></td>
            <td class="count"><span><asp:literal runat="server" ID="PurchasesTotal" /></span></td>
            <td class="money"><span><asp:literal runat="server" ID="RevenueTotal" /></span></td>
        </tr>
    </table>


<script type="text/javascript">
    
    function drawCharts() {
        // Create and populate the data table.

        var revData = new google.visualization.DataTable();
        <%= RevenueScript %>
        new google.visualization.PieChart(document.getElementById('revenueChart')).draw(revData,{legend:"none", width:215,height:210});
        var downData = new google.visualization.DataTable();
        <%= DownloadsScript %>
        new google.visualization.PieChart(document.getElementById('downloadsChart')).draw(downData,{legend:"none",width:215,height:210});
        var pageViewData = new google.visualization.DataTable();
        <%= PageViewsScript %>
        new google.visualization.PieChart(document.getElementById('pageViewsChart')).draw(pageViewData,{legend:"none",width:215,height:210});
        var purchasesData = new google.visualization.DataTable();
        <%= PurchasesScript %>
        new google.visualization.PieChart(document.getElementById('purchasesChart')).draw(purchasesData,{legend:"none",width:215,height:210});

    }

    google.setOnLoadCallback(drawCharts);
        
</script>