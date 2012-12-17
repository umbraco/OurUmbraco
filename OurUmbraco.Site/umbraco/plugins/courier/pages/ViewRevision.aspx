<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="ViewRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.ViewRevision" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        div.action{width: 25%; float: left;}
        div.action .inner{padding: 25px;}
        div.action p{line-height: 20px; margin-bottom: 25px}
        div.action h3{color: #999; border-bottom: 1px solid #efefef; padding-bottom: 4px;}
        div.action h3 a{color: #999; text-decoration: none;}
        div.last{border-right:none;}
    </style>

    <script type="text/javascript" src="/umbraco_client/ui/jQuery.js"></script>
    <script type="text/javascript" src="../scripts/RevisionDetails.js"></script>
    <script type="text/javascript">
    
        var currentRevision = '<asp:literal runat="server" id="lt_revisionName" />';
        function ShowTransferModal() {
            <% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>
            var nodeId = UmbClientMgr.mainTree().getActionNode().nodeId;
            UmbClientMgr.openModalWindow('plugins/courier/dialogs/transferRevision.aspx?revision= <%= Request["revision"] %>', 'Transfer Revision', true, 600, 500);
            <% }else{ %>
            openModal('plugins/courier/dialogs/transferRevision.aspx?revision=<%= Request["revision"] %>', 'Transfer Revision', 500, 600);
            <% } %>
        }

        function GotoExtractPage() {
            $('button').attr('disabled', 'disabled')
            $('input:submit').attr('disabled', 'disabled')
            var val = $('#statusId').val();;

            window.location = 'deployRevision.aspx?revision=<%= Request["revision"] %>&statusId='+val;
        }


        function GotoEditPage() {
            $('button').attr('disabled', 'disabled')
            $('input:submit').attr('disabled', 'disabled')
            window.location = 'editLocalRevision.aspx?revision=<%= Request["revision"] %>';
        }


        function GotoDetailPage() {
            $('button').attr('disabled', 'disabled')
            $('input:submit').attr('disabled', 'disabled')
            window.location = 'ViewRevisionDetails.aspx?revision=<%= Request["revision"] %>';
        }


        function displayStatusModal(title, message)
        {
            var val = $('#statusId').val();

            if (message == undefined)
                message = "Please wait while Courier loads";

            UmbClientMgr.openModalWindow('plugins/courier/pages/status.aspx?statusId='+val +"&message=" + message, title, true, 500, 450);
        }
    </script>

</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<input type="hidden" name="statusId" id="statusId" value="<%= Guid.NewGuid() %>" />


<umb:UmbracoPanel runat="server" ID="panel" Text="Revision Details">
    <umb:Pane runat="server" ID="NamePanel" Text="Revision name">
        <umb:PropertyPanel runat="server" Text="Revision items"><asp:Literal runat="server" Id="RevisionCountValue" /></umb:PropertyPanel>
        <umb:PropertyPanel runat="server" Text="Resource items"><asp:Literal runat="server" Id="ResourceCountValue" /></umb:PropertyPanel>

        <umb:PropertyPanel runat="server" ID="pp_download" Text="Download as:"> 
            <asp:LinkButton runat="server" onclick="ArchiveDownloadClick">Zip archive</asp:LinkButton> | 
            <asp:LinkButton runat="server" onclick="CreateGraphClick">Xml Graph</asp:LinkButton> | 
            <asp:LinkButton runat="server" onclick="CreateMindMapClick">Mindmap</asp:LinkButton>
        </umb:PropertyPanel>
</umb:Pane>  

<umb:Pane runat="server" ID="ActionResultPane" Visible="false">
    <div style="margin:10px;">
        <asp:Literal runat="server" Id="ActionResultMessage" />
    </div>
</umb:Pane>

<umb:Pane runat="server" ID="p_name" Text="Revision actions">
<p>
<strong>
    A Courier revision is a set of items, which you can either transfer to another location, deploy on this installation to install or update the items locally, or 
    finally you can edit the revision by adding and removing items to include.
</strong>
</p>          

<div class="actions">
<div class="action">
<div class="inner">
<h3><a href="#" onclick="GotoDetailPage(); return false;">Detailed view</a></h3>
<p>A revision can be a complex affair. Open the detailed view to see what
items are included, what items are connected and which act as a dependency.
</p>
</div>
</div>

<div class="action">
<div class="inner">
<h3><a href="#" onclick="GotoEditPage(); return false;">Select contents</a></h3>
<p>
Edit the contents of this revision, by selecting which 
items to include. Courier will then automaticly include resource files and
dependencies.
</p>
</div> 
</div>
        
<div class="action">
<div class="inner">
<h3><a href="#" onclick="displayStatusModal('Compare status', 'Please wait while Courier compares the state of your installation with items in this revision');GotoExtractPage(); return false;">Compare and install</a></h3>
<p>
Compare the contents of this revision to
your current system to determine what 
should be installed.
</p>
</div>
</div>

<div class="action last">
<div class="inner">
<h3><a href="#" onclick="ShowTransferModal(); return false;">Transfer</a></h3>
<p>
Move the content of this revision to another location. That could be another website
or simply a folder on another server.
</p>
</div>
</div>

<div style="clear:both"></div>
</div>
<div style="clear:both"></div>
</umb:Pane>

<umb:Pane runat="server" Text="Download & Upload" Id="DownloadsPane" Visible="false">
<p>
    <asp:literal runat="server" Id="UpDownErrorMessages">
    </asp:literal>
    <asp:Panel runat="server" ClientIDMode="static" ID="UploadPanel" Style="display:none;border:1px solid #888888;padding:10px;">
        <h2>Upload Courier Package</h2>
        <asp:FileUpload runat="server" ID="UploadPackageField"  />
        <asp:Button runat="server" ID="UploadPackageButton" Text="Upload" /><br /><br />
        Select a valid courier .zip file from your local machine by clicking the "Choose File" button.<br />
        Then when you press "Upload" it will read the package and, when it is a valid Courier Package file, overwite the current revision with its contents.
    </asp:Panel>
    <asp:Button runat="server" ID="UploadPackage" ClientIDMode="static" ClientId="UploadPackage" Text="Create from Courier Package File" Visible="true" OnClientClick="$('#UploadPackage').hide();$('#UploadPanel').show();return false;" />
    <asp:Button runat="server" Id="CreateGraphAndMindmap" Text="Generate MindMap and Graph downloads" Visible="false" />
    <asp:HiddenField runat="server" Id="MindMapHiddenField" />
    <asp:HiddenField runat="server" Id="GraphHiddenField" />
    <ul>
        <li runat="server" id="ArchiveDownloadListItem" visible="false"><asp:LinkButton runat="server" Id="ArchiveDownload" Text="Revision as archive"/></li>
        <asp:PlaceHolder runat="server" Id="DownloadGraphAndMindMap" Visible="false">
            <li style="margin-top:10px;"><asp:LinkButton runat="server" Id="MindmapDownload" Text="Mindmap"/> <small>(View with <a href="http://www.microsofttranslator.com/bv.aspx?ref=Internal&from=zh-chs&to=en&a=http://www.hyfree.net/product/blumind" target="_blank">BlueMind</a>)</small></li>
            <li><asp:LinkButton runat="server" Id="GraphDownload" Text="Xml Graph"/></li>
        </asp:PlaceHolder>
    </ul>
</p>
</umb:Pane>


</umb:UmbracoPanel>

</asp:Content>