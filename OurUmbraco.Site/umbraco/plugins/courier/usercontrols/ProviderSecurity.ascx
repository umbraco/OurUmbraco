<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProviderSecurity.ascx.cs" Inherits="Umbraco.Courier.UI.Usercontrols.ProviderSecurity" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<h3 style='color: #999; margin-top: 0px; border-bottom: 1px solid #D9D7D7; padding-bottom: 4px; margin-bottom: 12px;'><asp:Literal ID="name" runat="server" /></h3>
<div class="providerSecurity">
<asp:PlaceHolder runat="server" id="paneProvider" />
</div>
<br style="clear: both;" />