<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master" CodeBehind="ResourceBrowser.aspx.cs" Inherits="Umbraco.Courier.UI.Dialogs.ResourceBrowser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

        <style type="text/css">
            .cb{width: 30px !Important; float: left; height: 30px; clear: left;}
            .item{height: 30px; padding: 5px;}
            h3{padding-bottom: 20px}
        </style>
        

        
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


<ul id="filesList">

</ul>
<asp:repeater runat="server" id="rp_files">
    <ul>

    </ul>
</asp:repeater>

</asp:Content>