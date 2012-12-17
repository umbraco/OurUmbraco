<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master" CodeBehind="editCourierSecurity.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.editCourierSecurity" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<%@ Register Src="../Usercontrols/ProviderSecurity.ascx" TagPrefix="courier" TagName="providersec" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .propertypane div.propertyItem .propertyItemheader{width: 200px !Important;}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


<umb:UmbracoPanel Text="Courier security" runat="server" hasMenu="true" ID="panel">


<umb:Pane Text="General Settings" runat="server" ID="paneSettings" />

<umb:Pane Text="Provider Settings" runat="server" ID="phProviderSettings" />

</umb:UmbracoPanel>

</asp:Content>
