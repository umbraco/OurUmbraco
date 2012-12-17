<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Installer.ascx.cs" Inherits="Umbraco.Courier.UI.Usercontrols.Installer" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<umb:Feedback ID="fb1" runat="server" />

<div style="padding: 10px;">
<asp:PlaceHolder ID="ph" runat="server">
<h3><asp:Literal ID="header" runat="server" /></h3>
<asp:Literal ID="status" runat="server" />
<h3>Add a location</h3>
<p>
	To move changes from one location to another, please enter the domain of another umbraco installation, running Courier 2
</p>
<p>
	<asp:TextBox runat="server" ID="tb_domain" />
</p>
<p>
	<asp:Button runat="server" OnClick="addLocation" Text="Create location" /> 
	<em> or </em> 
	<a href="#" onclick="window.parent.location.href = '/umbraco/umbraco.aspx?app=courier'; return false;">Open Umbraco Courier</a>
</p>
</asp:PlaceHolder>
</div>