<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorPayout.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Admin.VendorPayout" %>
<h2>Vendor Payout</h2>

<asp:Button ID="GetPayoutButton" runat="server" Text="Get Payout Details" 
    onclick="GetPayoutButton_Click" />

<asp:PlaceHolder ID="PayoutPlaceholder" runat="server" Visible="false">

<h3>Period ending <asp:literal ID="EndDate" runat="server" /></h3>

<p>
<asp:Repeater ID="PayoutReport" runat="server">
        <HeaderTemplate>
            <table class="dataTable">
                <thead>
                    <tr>
                        <th>Vendor Name</th>
                        <th>Economid Id</th>
                        <th>Vendor Ref</th>
                        <th>Net Payout</th>
                        <th>Payout Date</th>
                    <tr>
                </thead>
        </HeaderTemplate>
        <ItemTemplate>
                <tbody>
                    <tr>
                        <td>
                            <asp:Literal ID="VendorName" runat="server" />
                        </td>
                        <td>
                            <asp:Literal ID="EconomicId" runat="server" />
                        </td>
                        <td>
                            <%#Eval("VendorRef").ToString() %>
                        </td>
                        <td>
                            <asp:Literal ID="NetPayout" runat="server" />
                        </td>
                        <td>
                            <%#Eval("PayoutDate").ToString()%>
                        </td>
                    </tr>
                </tbody>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
</asp:Repeater>
</p>

<asp:Button ID="MarkAsPaid" runat="server" Text="Mark as Paid" OnClick="MarkAsPaidButton_Click" Visible="false" />

</asp:PlaceHolder>
