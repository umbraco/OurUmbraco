<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorPayout.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Payout.VendorPayout" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<asp:PlaceHolder runat="server" ID="payoutHolder">
<h2>Eligable Orders</h2>
<p>Payouts are given on orders occured in the previous month and have not been refunded.</p>
<p>To view past payout requests <a href="<%= HistoryPage %>">View your payout history</a>.</p>
<asp:PlaceHolder runat="server" ID="payoutForm">
<asp:Repeater runat="server" ID="PayoutList">
<HeaderTemplate>
<table class="dataTable">
<thead>
    <tr>
        <th><input type="checkbox" id="SelectAllBoxes" onchange="CheckAll(this.id)" /> Select all</th>
        <th>Package</th>
        <th>License</th>
        <th>Purchaser</th>
        <th>Country</th>
        <th>VAT #</th>
        <th>Order Date</th>
        <th class="right">Payout Value*</th>
    </tr>
</thead>
<tbody>
</HeaderTemplate>
<ItemTemplate>
    <tr>
        <td>
            <asp:CheckBox runat="server" ID="RowCheckbox" class="rowCheck"  />
            <asp:HiddenField runat="server" ID="RowId" Value='<%# Eval("Id") %>' />
        </td>
        <td class="packageName"><%# Eval("Package") %></td>
        <td><%# Eval("LicenseTypeName") %></td>
        <td><a href="mailto:<%# ((IMember)Eval("Member")).Email %>"><%# ((IMember)Eval("Member")).Name %></a></td>
        <td><%# ((IMember)Eval("Member")).CompanyCountry %></td>
        <td><%# ((IMember)Eval("Member")).CompanyVATNumber %></td>
        <td><%# Eval("OrderDate") %></td>
        <td class="right payoutValue"><%# Eval("PayoutAmount") %></td>
    </tr>
</ItemTemplate>
<FooterTemplate>
    <tr class="totals">
        <td colspan="7" class="right">Select Payout Items Total*:</td><td class="right grand">€<span id="payoutTotal">0.00</span></td>
    </tr>
</tbody>
</table>
<small>* your profit after any vouchers & fees have been removed</small>
</FooterTemplate>
</asp:Repeater>


<div class="form simpleForm">
    <fieldset>
    <legend>Generate your payout request</legend>
<p>
<asp:Label ID="Label1" runat="server" AssociatedControlID="vendor_reference" Text="Your reference" cssclass="inputLabel" />
<asp:TextBox runat="server" ID="vendor_reference" CssClass="title" />
</p>
<small>If you are required to generate an invoice against your income please use this field to record your invoice</small>
<div class="buttons"><asp:Button runat="server" ID="bt_submit" cssclass="submitButton" Text="Request payout" OnClick="PayoutRequest_Click" OnClientClick="return confirmPayout()" /></div>
</fieldset>
</div>



<script type="text/javascript">
    function CheckAll(id) {
        $(".rowCheck :checkbox").attr('checked', $('#' + id).is(':checked'));
        calculateTotal();
    }

    function calculateTotal() {

        var payoutTotal = parseFloat(0.00);
        $(".rowCheck :checked").each(function () {

            var payoutValue = $(this).parents("tr").find(".payoutValue").text();
            payoutTotal += parseFloat(payoutValue);

        });
        $("#payoutTotal").text(payoutTotal.toFixed(2));
    }

    function confirmPayout() {
        if ($('#<%= vendor_reference.ClientID %>').val() != '') {
            return confirm('By clicking ok your payout will be sent for processing.  Are you sure?');
        }
        else {
            return confirm('You have not provided a reference.  Are you sure that you want to proceed?');
        }
    }


    $(document).ready(function () {
        $(".rowCheck :checkbox").click(calculateTotal);
    });
</script>
</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="NoPayouts">
<div class="deliNotification">
<p>There are currently no orders eligable for payout. <a href="<%= HistoryPage %>">View your payout history</a></p>
</div>
</asp:PlaceHolder>


</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="PayoutThanks" Visible="false">

<%--
    // temporary notification code.
    umbraco.library.SendMail("nh@umbraco.dk", "nh@umbraco.dk", "Payout requested", "Hey Niels, There's a payout requested. Check <a href=\"http://our.umbraco.org/umbraco/\">our.umbraco.org/umbraco/</a>",true);
    
    --%>




<h2>Payout Submitted</h2>
<div class="deliNotification">
<p>Your payout request has been sent for processing.  All payouts are processed on the 20th of the month. <a href="<%= HistoryPage %>">View your payout history</a></p>
</div>
</asp:PlaceHolder>