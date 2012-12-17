<%@ Page Title="" Language="C#" MasterPageFile="../MasterPages/CourierPage.Master" AutoEventWireup="true" CodeBehind="editRemoteRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.transferRevision" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<%@ Register Src="../usercontrols/RevisionContentsOverview.ascx" TagName="RevisionContents" TagPrefix="courier" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">

         function ShowTransferModal() {
                    <% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>
                    var nodeId = UmbClientMgr.mainTree().getActionNode().nodeId;
                    UmbClientMgr.openModalWindow('plugins/courier/dialogs/transferRevision.aspx?revision=<%= Request["revision"] %>&repo=<%= Request["repo"] %>&folder=<%=JsEscape(Request["folder"]) %>', 'Transfer Revision', true, 400, 200);
                    <% }else{ %>
                    openModal('plugins/courier/dialogs/transferRevision.aspx?revision=<%= Request["revision"] %>&repo=<%=Request["repo"] %>&folder=<%=JsEscape(Request["folder"]) %>', 'Transfer Revision', 200, 400);
                    <% } %>
                }

        </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


<umb:UmbracoPanel Text="Remote revision" runat="server" ID="panel">

<umb:Pane Text="Remote revision" runat="server" ID="pane">
   <umb:PropertyPanel runat="server" Text="Name"><%= Request.QueryString["revision"] %></umb:PropertyPanel>
   <umb:PropertyPanel runat="server" ID="ppFolder" Text="Folder"><%= Request.QueryString["folder"] %></umb:PropertyPanel>
   <umb:PropertyPanel runat="server" Text="Repository"><%= Request.QueryString["repo"] %></umb:PropertyPanel>
   <umb:PropertyPanel Visible="false" runat="server" Text="Last Modified"><%= Revision.LastModified %></umb:PropertyPanel>
</umb:Pane>

<umb:Pane Text="Transfer" runat="server">
<p>
    You can transfer the contents of this revision to the installation, you are currently browsing.
    Simply click the <strong>transfer</strong> button below.
</p>

<p>
    As soon as it's transfered, you can deploy the changes it contains locally.
</p>



<p>
    <button runat="server" id="bt_transfer" onclick="ShowTransferModal(); return false;">Transfer</button>
</p>

</umb:Pane>


</umb:UmbracoPanel>





</asp:Content>
