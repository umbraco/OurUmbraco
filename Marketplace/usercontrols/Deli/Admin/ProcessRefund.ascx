<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProcessRefund.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Admin.ProcessRefund" %>

<h2>Process Refund</h2>
<p>
Order Id: <asp:TextBox ID="OrderId" runat="server"></asp:TextBox>
&nbsp;<asp:Button ID="GetOrderButton" runat="server" Text="Lookup Order" 
        onclick="GetOrderButton_Click" />
</p>
<h4>Order Review</h4>
<p>
    <strong><asp:Literal ID="OrderReviewDisplay" runat="server"></asp:Literal></strong>
</p>

<h4>Select an Order item to Refund (also revokes the member license)</h4>
<p>
    <asp:RadioButtonList ID="OrderItems" runat="server">
    </asp:RadioButtonList>
</p>

<h4>Order Notes</h4>
<p>
    <asp:Repeater ID="OrderNotesRepeater" runat="server">
    <ItemTemplate>
        <%# Eval("OrderNote") %> - <%# Eval("OrderNoteDate").ToString() %>
        <br />
    </ItemTemplate>
    </asp:Repeater>
</p>
<p>
    <asp:Button ID="ProcessRefundButton" runat="server" Text="Process Refund" 
        onclick="ProcessRefundButton_Click" />
&nbsp;<asp:Literal ID="RefundConfirmation" runat="server"></asp:Literal>
</p>
<p>
    <b>Note</b>:&nbsp; This only processes the Deli refund, please also process via 
    PayPal and E-conomic Credit Note</p>
<p>
    &nbsp;</p>

