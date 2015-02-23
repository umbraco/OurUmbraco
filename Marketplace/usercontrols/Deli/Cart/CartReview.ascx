<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CartReview.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Cart.CartReview" %>


<asp:PlaceHolder runat="server" ID="CartHolder">
<table class="dataTable">

    <thead>
        <tr>
            <th>Product</th>
            <th>License Type</th>
            <th class="money">Price</th>
            <th class="count">Quantity</th>
            <th class="money">SubTotal</th>
        </tr>
    </thead>

<asp:repeater runat="server" ID="CartItems">
<ItemTemplate>
    <tbody>
        <tr>
            <td><%#Eval("Name") %></td>
            <td><%#Eval("License") %></td>
            <td class="money"><asp:Literal ID="price" runat="server" /></td>
            <td class="count">
                <asp:label ID="Quantity" text='<%# Eval("Quantity") %>' runat="server" />
            </td>
			<td class="money"><asp:Literal ID="subtotal" runat="server" /></td>
        </tr>
    </tbody>

</ItemTemplate>
</asp:repeater>

<asp:PlaceHolder runat="server" ID="TaxRow">
    <tr>
    <td colspan="3"></td>
    <td class="money">Tax:</td>
    <td class="money"><asp:literal runat="server" ID="TaxTotal" /></td>
    </tr>
</asp:PlaceHolder>

<tr class="totals">
<td colspan="3"></td>
<td class="money">Total:</td>
<td class="money"><asp:literal runat="server" ID="CartTotal" /></td>
</tr>
</table>

<asp:PlaceHolder ID="VatBadMessage" runat="server" Visible="false">
    <div class="deliNotification">
        <h2>VAT Code not specified or is invalid or cannot be validated at this time</h2>
        <p>From what you have told us you live in a country that is subject to European VAT, but from what we can tell you either have not specifed a VAT code or the one you have given us is invalid.</p>
        <p>If you dont have a VAT code or you can't put your hands on it right now, dont worry you can still continue with your purchase but we had to add VAT to your order.</p>
        <p>If you would like to go back and give us a VAT code <asp:linkbutton runat="server" Text="click here" OnClick="CartNavPrevious_Click" />.</p>
    </div>
</asp:PlaceHolder>


<fieldset>
<legend>Your Information</legend>


        <p><strong><asp:Literal ID="OrderMembername" runat="server"></asp:Literal></strong></p>
        <p><strong>Company:</strong></p>
        <p><asp:Literal ID="OrderCompany" runat="server"></asp:Literal></p>
        <asp:PlaceHolder runat="server" ID="VATDetails">
            <p><strong>VAT Details:</strong></p>
            <p><asp:Literal ID="OrderVATCode" runat="server"></asp:Literal></p>
        </asp:PlaceHolder>
        <p><strong>Invoice Email:</strong></p>
        <p><asp:Literal ID="OrderInvoiceEmail" runat="server"></asp:Literal></p>


        <div class="buttons">
                <asp:linkbutton runat="server" Text="&lt; Make Changes" ID="CartNavPrevious" OnClick="CartNavPrevious_Click" />&nbsp;
                <asp:Button runat="server" Text="Checkout &gt;" ID="CartNavNext" OnClick="CartNavNext_Click" />
        </div>
</fieldset>
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="CartEmptyHolder" Visible="false">
<div>
<p>Your cart is currently empty</p>
</div>

</asp:PlaceHolder>