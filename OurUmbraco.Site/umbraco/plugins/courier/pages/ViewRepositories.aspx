<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="ViewRepositories.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.ViewRepositories" %>
<%@ Import Namespace="Umbraco.Courier.UI" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        <style>
            
            .folderItem, .revisionItem
            {
                display:inline-block;
                background-repeat:no-repeat;
                background-position:left center;
                height:22px;
            }

            .folderItem
            {
                padding-left:22px;
                background-position:left 0px;
                background-image:url('/umbraco/images/umbraco/<%=UIConfiguration.RepositoryTreeIcon %>');
            }
            
            tr.row td
            {
                padding-right:20px;
            }
            
        </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<umb:UmbracoPanel Text="Location" runat="server" hasMenu="false" ID="panel">

<umb:Pane Text="Result" runat="server" ID="paneResult" Visible="false">
    <asp:Literal runat="server" Id="txtResult"/>
</umb:Pane>

<umb:Pane Text="Available locations" runat="server" ID="paneRepoRevisions">
    <asp:repeater runat="server" id="rp_revisions">
        <headerTemplate>
            <table>
                <tr>
                    <td>Name</td>
                    <td>Type</td>
                </tr>
        </headerTemplate>

        <itemtemplate>            
            <tr class="row">
                <td>
                    <span class="folderItem">
                        <a href="<%= UIConfiguration.RepositoryEditPage%>?repo=<%# Eval("Alias") %>"><%# Eval("Name")%></a>
                    </span>
                </td>
                <td>
                    <small><%# Eval("Provider.Name")%></small>
                </td>
            </tr>
        </itemtemplate>

        <footerTemplate>
            </table>
        </footerTemplate>
    </asp:repeater>
</umb:Pane>

</umb:UmbracoPanel>

</asp:Content>
