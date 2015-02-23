<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Cart.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Cart.Cart" %>


<asp:PlaceHolder runat="server" ID="CartHolder">
<table class="dataTable">

    <thead>
        <tr>
            <th>Product</th>
            <th>License Type</th>
            <th class="money">Price</th>
            <th class="count">Quantity</th>
            <th></th>
            <th class="money">Sub-Total</th>
        </tr>
    </thead>

<asp:repeater runat="server" ID="CartItems">
<ItemTemplate>
    <tbody>
        <tr>
            <td><%#Eval("Name") %></td>
            <td><%#Eval("License") %></td>
            <td class="money"><asp:Literal id="price" runat="server" /></td>
            <td class="count">
			    <asp:HiddenField id="LicenseId" value='<%# Eval("Id") %>' runat="server" />
                <asp:textbox ID="Quantity" columns="3" text='<%# Eval("Quantity") %>' runat="server" OnTextChanged="QuantityOnChange"  onkeypress="return isNumberKey(event)" />
            </td>
			<td><asp:button ToolTip="Remove item" id="Del" text="remove" runat="server" /></td>

            <td class="money"><asp:Literal id="subtotal" runat="server" /></td>
        </tr>
    </tbody>

</ItemTemplate>
</asp:repeater>

<tr class="totals">
<td colspan="3"></td>
<td class="count"><asp:Button runat="server" Text="update" /></td>
<td class="count">Total:</td>
<td class="money"><asp:literal runat="server" ID="CartTotal" /></td>
</tr>
</table>


<script type="text/javascript">
    //stop things other than numbers being entered into quantity.
    function isNumberKey(evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode
        if (charCode > 31 && (charCode < 48 || charCode > 57))
            return false;

        return true;
    }
</script>
<fieldset>


<div class="buttons">
    <asp:linkbutton runat="server" Text="&lt; Continue Shopping" ID="CartNavPrevious" OnClick="CartNavPrevious_Click" />&nbsp;
    <asp:Button runat="server" Text="Next &gt;" ID="CartNavNext" class="submitButton" OnClick="CartNavNext_Click" />
</div>
</fieldset>
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="CartEmptyHolder" Visible="false">
<div>
<p>Your cart is currently empty</p>
</div>

</asp:PlaceHolder>