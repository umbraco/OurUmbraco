<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VendorSupportRequest.ascx.cs" Inherits="Marketplace.usercontrols.Deli.VendorSupport.VendorSupportRequest" %>

<asp:Panel ID="pnlForm" runat="server">

<asp:ValidationSummary ID="valsum" CssClass="error" DisplayMode="BulletList" runat="server" ValidationGroup="support"/>



<fieldset>

<legend>Submit a support ticket</legend>
<p>
<label  class="inputLabel">
Subject
 <asp:RequiredFieldValidator ValidationGroup="support" ControlToValidate="tb_subject" ID="RequiredFieldValidator1" runat="server" Text="*" ErrorMessage="Subject is a mandatory field"></asp:RequiredFieldValidator>
 </label> <asp:TextBox id="tb_subject" runat="server"  CssClass="required title" />
</p>

<p>
<label  class="inputLabel">Package Name
 <asp:RequiredFieldValidator ValidationGroup="support" ControlToValidate="tb_package" ID="RequiredFieldValidator3" runat="server" Text="*" ErrorMessage="Package name is a mandatory field"></asp:RequiredFieldValidator>
</label> 
<asp:textbox ID="tb_package" runat="server"  CssClass="required title" />
</p>

<p>
<label class="inputLabel">
Description
 <asp:RequiredFieldValidator ValidationGroup="support" ControlToValidate="tb_desc" ID="RequiredFieldValidator2" runat="server" Text="*" ErrorMessage="Description is a mandatory field"></asp:RequiredFieldValidator>
</label> <asp:TextBox ID="tb_desc" runat="server" TextMode="MultiLine" Columns="20" Rows="2" style="width: 580px; height: 300px;"/>
</p>

<p>
<asp:Button runat="server" ID="bt_submit" OnClick="bt_submit_Click" ValidationGroup="support" Text="Submit ticket"/>
</p>
</fieldset>

</div>
</asp:Panel>

<asp:Panel ID="pnlError" runat="server" Visible="false">
<p>Oops something went wrong, please try again. </p>
</asp:Panel>

<asp:Panel ID="pnlSuccess" runat="server" Visible="false">
<p>Thank you for your support ticket, we'll be in touch soon.</p>
</asp:Panel>