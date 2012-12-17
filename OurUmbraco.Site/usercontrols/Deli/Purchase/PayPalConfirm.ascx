<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PayPalConfirm.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Purchase.PayPalConfirm" %>

<p>&nbsp;</p>
<h2>Thank You!</h2>
<p>&nbsp;</p>
<p>
Your order is being processed and you licenses will be available shortly.
</p>
<p>
<a href="/member/profile/my-licenses/">Your Licenses</a>
<br />
<a href="/member/profile/">Your Profile</a>
</p>



<asp:Panel ID="PaymentConfirmPanel" runat="server" Visible="false">

<div>
Your order is confirmed and your license is ready to configure.
Please select a link below to continue:
</div>

<p>
<a href="/member/profile/my-licenses/">Your Licenses</a>
<br />
<a href="/member/profile/">Your Profile</a>
</p>

<div>

</div>
</asp:Panel>

<asp:panel ID="InvoicePanel" runat="server" Visible="false">
<p>&nbsp;</p>
<h2>Invoice</h2>
<p>&nbsp;</p>
<div>

    Your invoice is available <asp:HyperLink ID="InvoiceLink" runat="server">here</asp:HyperLink>

</div>
</asp:panel>

<asp:Panel ID="PaymentPendingPanel" runat="server" Visible="false">

<h2>Thank You!</h2>
<div>
Your order is pending approval and your license will be ready soon.
Please select a link below to continue:
</div>

<p>
<a href="/member/profile/my-licenses/">Your Licenses</a>
<br />
<a href="/member/profile/">Your Profile</a>
</p>

</asp:Panel>

