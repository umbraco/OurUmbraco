<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorPayoutReport.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Admin.VendorPayoutReport" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<h2>Payout Report</h2>
<p>The following list shows all payouts and their status</p>
<asp:Button runat="server" text="Load Pending Report" OnClick="LoadReport" />
<asp:Button runat="server" text="Load Processed Report (can take a while)" OnClick="LoadProcessedReport" />
<asp:Repeater runat="server" ID="PayoutHistoryList" OnItemCommand="processPayoutRow">
<HeaderTemplate>
<table class="dataTable">
<thead>
<tr>
<th>Vendor Name</th>
<th>Economid Id</th>
<th>Vendor Ref</th>
<th>Date</th>
<th>Status</th>
<th class="right">Value</th>
</tr>
</thead>
</HeaderTemplate>
<ItemTemplate>
<tr>
<td>
<asp:HiddenField ID="rowId" runat="server" Value='<%# Eval("Id") %>' />
<span class="expand" id="showHide_<%# Eval("Id") %>">[+]</span>&nbsp;
<%# ((IVendor)Eval("Vendor")).Member.Name%>
</td>
<td>
<%# ((IVendor)Eval("Vendor")).EconomicId%>
</td>
<td>
<%# Eval("Reference")%>
</td>
<td>
<%# Eval("PayoutDate")%>
</td>
<td>
<%# Eval("Status")%>
</td>
<td class="right">
<%# Eval("PayoutAmount")%>
</td>
</tr>
<tr class="detailsRow" id="details_<%# Eval("Id") %>">
    <td colspan="6" class="borderTop">
        <h3>Payout details</h3>
        <p><strong>Company Name:</strong> <%# ((IVendor)Eval("Vendor")).VendorCompanyName %></p>
        <p><strong>Billing Email:</strong> <%# ((IVendor)Eval("Vendor")).BillingContactEmail %></p>
        <p><strong>VAT:</strong> <%# ((IVendor)Eval("Vendor")).VATNumber%></p>
        <p><strong>Tax ID:</strong> <%# ((IVendor)Eval("Vendor")).TaxId%></p>
        <p><strong>Paypal:</strong> <%# ((IVendor)Eval("Vendor")).PayPalAccount%></p>
        <p><strong>IBAN:</strong> <%# ((IVendor)Eval("Vendor")).IBAN%></p>
        <p><strong>SWIFT:</strong> <%# ((IVendor)Eval("Vendor")).SWIFT%></p>
        <p><strong>BSB:</strong> <%# ((IVendor)Eval("Vendor")).BSB%></p>
        <p><strong>Account Number:</strong> <%# ((IVendor)Eval("Vendor")).AccountNumber %></p>
        
        <asp:Repeater runat="server" ID="payoutItemRepeater">
            <HeaderTemplate>
            <table class="dataTable">
            <thead>
                <tr>
                    <th>Package</th>
                    <th>License</th>
                    <th>Purchaser</th>
                    <th>Country</th>
                    <th>VAT #</th>
                    <th>Order Date</th>
                    <th class="right">Payout Value*</th>
                </tr>
                </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Package")%></td>
                    <td><%# Eval("LicenseTypeName")%></td>
                    <td><a href="mailto:<%# ((IMember)Eval("Member")).Email %>"><%# ((IMember)Eval("Member")).Name%></a></td>
                    <td><%# ((IMember)Eval("Member")).CompanyCountry%></td>
                    <td><%# ((IMember)Eval("Member")).CompanyVATNumber%></td>
                    <td><%# Eval("OrderDate")%></td>
                    <td class="right"><%# Eval("PayoutAmount")%></td>
                </tr>            
            </ItemTemplate>
            <FooterTemplate>
            </table>
            </FooterTemplate>
        </asp:Repeater>
        <asp:PlaceHolder runat="server" Visible='<%# Eval("Status") != "Processed" %>'>
        <div style="border:1px solid #ccc;background:#eee;padding:1em;">
                <h3>Process this Payment Request</h3>
                <p>Once you have processed this payment request please click the following button.</p>
                <asp:Button runat="server" ID="processPayment" Text="Process Payment" CommandArgument='<%# Eval("Id") %>' CommandName="Payout" OnClientClick="return confirm('Are you sure you want to process this payment?')" />
        </div>
        </asp:PlaceHolder>
    </td>
</tr>
</ItemTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
</asp:Repeater>

<script type="text/javascript">
    $(document).ready(function () {
        $(".detailsRow").hide();

        $(".expand").click(function () {
            
            var id = $(this).attr("id").split("_")[1];

            if ($("#details_" + id).is(":visible")) {
                $("#details_" + id).hide();
                $(this).text("[+]");
            } else {
                $("#details_" + id).show();
                $(this).text("[-]");
            }
        });


    });
</script>