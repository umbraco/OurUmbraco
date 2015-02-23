<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrderReview.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Admin.OrderReview" %>
<h2>Deli Orders</h2>
<p>Select Date Range
    <asp:DropDownList ID="DateRangeDdl" runat="server">
        <asp:ListItem Value="1">1 Day</asp:ListItem>
        <asp:ListItem Value="3">3 Days</asp:ListItem>
        <asp:ListItem Value="7" Selected="True">7 Days</asp:ListItem>
        <asp:ListItem Value="14">14 Days</asp:ListItem>
        <asp:ListItem Value="30">30 Days</asp:ListItem>
        <asp:ListItem Value="60">60 Days</asp:ListItem>
        <asp:ListItem Value="90">90 Days</asp:ListItem>
        <asp:ListItem Value="120">120 Days</asp:ListItem>
    </asp:DropDownList>
&nbsp;&nbsp;
    <asp:Button ID="GetOrdersButton" runat="server" Text="Get Orders" 
        onclick="GetOrdersButton_Click" />
</p>

<%--<asp:GridView ID="OrderView" runat="server" AutoGenerateColumns="false">

<HeaderStyle BackColor="#D1D1D1"></HeaderStyle>

<Columns>

<asp:BoundField HeaderText="Deli Id" DataField="Id" />
<asp:BoundField HeaderText="Umbraco Id" DataField="OrderId" />
<asp:BoundField HeaderText="Status" DataField="Status" />
<asp:BoundField HeaderText="TaxTotal EUR" DataField="TaxTotal" />
<asp:BoundField HeaderText="SubTotal EUR" DataField="SubTotal" />
<asp:BoundField HeaderText="Total EUR" DataField="Total" />
<asp:BoundField HeaderText="Currency" DataField="Currency" />
<asp:BoundField HeaderText="Local Total" DataField="ProcessedTotal" />
<asp:BoundField HeaderText="Company" DataField="CompanyName" />
<asp:BoundField HeaderText="Invoice Email" DataField="CompanyInvoiceEmail" />
<asp:BoundField HeaderText="Country" DataField="CompanyCountry" />
<asp:BoundField HeaderText="PaymentDate" DataField="PaymentDate" />
<asp:BoundField HeaderText="Refunded?" DataField="IsRefunded" />
<asp:BoundField HeaderText="Refund Date" DataField="RefundDate" />

</Columns>
</asp:GridView>--%>


<asp:Repeater runat="server" ID="OrderView">
<HeaderTemplate>
<table class="dataTable">
<thead>
<tr>
    <th>Deli Id</th>
    <th>Economid Id</th>
    <th>Status</th>
    <th>Company</th>
    <th>VAT Id</th>
    <th>Invoice Email</th>
    <th>Country</th>
    <th>Date</th>
    <th class="right">Sub Total EUR</th>
    <th class="right">Tax Total EUR</th>
    <th class="right">Total EUR</th>
    <th>Currency</th>
    <th>Refunded?</th>
    <th>Refund Date</th>
</tr>
</thead>
</HeaderTemplate>
<ItemTemplate>
<tr>
    <td>
    <asp:HiddenField ID="rowId" runat="server" Value='<%# Eval("Id") %>' />
    <span class="expand" id="showHide_<%# Eval("Id") %>">[+]</span>&nbsp;
    <%# Eval("Id")%>
    </td>
    <td>
    <%# Eval("EconomicId")%>
    </td>
    <td>
    <%# Eval("Status")%>
    </td>
    <td>
    <%# Eval("Company")%>
    </td>
    <td>
    <%# Eval("VatID")%><br />
    <asp:Literal ID="IsValid" runat="server" />
    <asp:Button runat="server" ID="ValidateVat" style="font-family:Webdings" Text="a" OnCommand="RowCommand" CommandArgument='<%# Eval("VatID") %>' CommandName="ValidateVat" />
    </td>
    <td>
    <%# Eval("InvoiceEmail")%>
    </td>
    <td>
    <%# Eval("Country")%>
    </td>
    <td>
    <%# Eval("Date")%>
    </td>
    <td class="right">
    <%# Eval("SubTotal")%>
    </td>
    <td class="right">
    <%# Eval("TaxTotal")%>
    </td>
    <td class="right">
    <%# Eval("Total")%>
    </td>
    <td>
    <%# Eval("Currency")%>
    </td>
    <td>
    <%# Eval("Refunded")%>
    </td>
    <td>
    <%# Eval("RefundDate")%>
    </td>
</tr>
<tr class="detailsRow" id="details_<%# Eval("Id") %>">
    <td colspan="13" class="borderTop">
        <h3>Order details</h3>
        <p>These are the individual products ordered</p>      
        <asp:Repeater runat="server" ID="orderItemRepeater">
            <HeaderTemplate>
            <table class="dataTable">
            <thead>
                <tr>
                    <th>Vendor</th>
                    <th>Package</th>
                    <th>License</th>
                    <th>Quantity</th>
                    <th>Value</th>
                </tr>
            </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Vendor")%></td>
                    <td><%# Eval("Package")%></td>
                    <td><%# Eval("LicenseTypeName")%></td>
                    <td><%# Eval("Quantity")%></td>
                    <td class="right"><%# Eval("Value")%></td>
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

