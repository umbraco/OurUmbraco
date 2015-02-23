<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorPayoutHistory.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Payout.VendorPayoutHistory" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<h2>Payout History</h2>
<p>The following list shows all your past and payouts and their status</p>

<asp:Repeater runat="server" ID="PayoutHistoryList">
<HeaderTemplate>
<table class="dataTable">
<thead>
<tr>
<th>Date</th>
<th>Reference</th>
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
<%# Eval("PayoutDate") %>
</td>
<td>
<%# Eval("Reference") %>
</td>
<td>
<%# Eval("Status") %>
</td>
<td class="right">
€<%# Eval("PayoutAmount") %>
</td>
</tr>
<tr class="detailsRow" id="details_<%# Eval("Id") %>">
    <td colspan="4">
        <h3>Payout details</h3>
        <asp:Repeater runat="server" ID="payoutItemRepeater">
            <HeaderTemplate>
            <table>
                <tr>
                    <th>Package</th>
                    <th>License</th>
                    <th>Purchaser</th>
                    <th>Country</th>
                    <th>VAT #</th>
                    <th>Order Date</th>
                    <th class="right">Payout Value*</th>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Package") %></td>
                    <td><%# Eval("LicenseTypeName") %></td>
                    <td><a href="mailto:<%# ((IMember)Eval("Member")).Email %>"><%# ((IMember)Eval("Member")).Name %></a></td>
                    <td><%# ((IMember)Eval("Member")).CompanyCountry %></td>
                    <td><%# ((IMember)Eval("Member")).CompanyVATNumber %></td>
                    <td><%# Eval("OrderDate") %></td>
                    <td class="right">€<%# Eval("PayoutAmount") %></td>
                </tr>            
            </ItemTemplate>
            <FooterTemplate>
            </table>
            </FooterTemplate>
        </asp:Repeater>
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