<%@ Page Language="C#" MasterPageFile="../../../masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="FergusonMoriyama.Umbraco.Config" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="Stylesheet" href="<%= ConfigurationManager.AppSettings["umbracoPath"] %>/plugins/FergusonMoriyama/Config/styles.css" />
    <script type="text/javascript" src="<%= ConfigurationManager.AppSettings["umbracoPath"] %>/plugins/FergusonMoriyama/Config/script.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel id="Panel1" Text="Feed Cache - Configuration" runat="server" Width="612px" Height="600px" hasMenu="false">
    <p>
        <a href="#" onClick="window.open('<asp:Literal ID='Literal2' runat='server'></asp:Literal>'); return false;">Test current configuration</a>
    </p>

    <asp:Literal ID="Literal1" runat="server"></asp:Literal> 
  </cc1:UmbracoPanel>
</asp:Content>
