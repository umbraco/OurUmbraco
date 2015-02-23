<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PayPalPayment.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Purchase.PayPalPayment" %>

<asp:PlaceHolder ID="pl_orderInfo" Visible="false" runat="server">
<form action="<%= _paypalurl %>" id="payPalForm" style="display: inline;" method="post">
  <input type="hidden" name="cmd" value="_cart"/>
  <input type="hidden" name="upload" value="1"/>
  <input type="hidden" name="redirect_cmd" value="_xclick"/>
  <input type="hidden" name="business" value="<%= MerchantEmailAddress %>"/>
  <input type="hidden" name="undefined_quantity" value="0"/>

<%
// PayPal item identifier
int i = 1;     

// build order post when more than one item in cart
foreach (Marketplace.Providers.OrderItem.DeliOrderItem item in Order.Items)
{
%>
  <input type="hidden" name="item_name_<%=i.ToString() %>" value="<%= ListingProvider.GetListing(item.ListingItemId, false).Name %>, License Type: <%= LicenseProvider.GetLicense(item.LicenseId).LicenseType.ToString() %>" />
  <input type="hidden" name="item_number_<%=i.ToString() %>" value="<%= item.ListingItemId.ToString() %>"/>
  <input type="hidden" name="quantity_<%=i.ToString() %>" value="<%= item.Quantity.ToString() %>"/>
  <input type="hidden" name="amount_<%=i.ToString() %>" value="<%= CartProvider.GetLocalizedPrice((double)item.NetPrice).ToString() %>" />
<%
    i++;
}
%>

  <input type="hidden" name="tax_cart" value="<%= _orderTaxTotal %>"/>
  <input type="hidden" name="currency_code" value="<%= Order.Currency.ToString() %>"/>
  <input type="hidden" name="lc" value="US"/>
  <input type="hidden" name="invoice" value="<%= Order.Id.ToString() %>"/>
  <input type="hidden" name="email" value="<%= _email %>"/>
  <input type="hidden" name="notify_url" value="<%= _notifyurl %>"/>
  <input type="hidden" name="return" value="<%= _returnurl %>"/>

</form>

<script type="text/javascript">
    document.forms.payPalForm.submit();
</script>

</asp:PlaceHolder>