<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="editRepository.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.editRepository" %>
<%@ Import Namespace="Umbraco.Courier.UI" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
         function ShowTransferModal(revision, folder) {
                    <% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>
                    var nodeId = UmbClientMgr.mainTree().getActionNode().nodeId;
                    UmbClientMgr.openModalWindow('plugins/courier/dialogs/transferRevision.aspx?revision=' + revision + '&repo=<%= Request["repo"] %>&folder='+folder, 'Transfer Revision', true, 400, 200);
                    <% }else{ %>
                    openModal('plugins/courier/dialogs/transferRevision.aspx?revision=' + revision + '&repo=<%=Request["repo"] %>&folder='+folder, 'Transfer Revision', 200, 400);
                    <% } %>
                }

                function updateTransferItemsInput()
                {
                    var toTransfer = [];
                    $('.submitCb:checked').each(function()
                    {
                        toTransfer.push($(this).val());
                    });
                    $('#revisionsToTransfer').val(toTransfer);

                    $('#TransferSelectedRevisions').attr('disabled',toTransfer.length == 0);
                }

                function select(type) {
                    $('.submitCb:visible').each(function () {
                        switch (type) {
                            case 0:
                            case 1:
                                $(this).attr('checked', type == 1); break;
                            case 2:
                                $(this).attr('checked', !$(this).attr('checked')); break
                        }

                    });
                    updateTransferItemsInput();
                    return false;
                }

                $(document).ready(function () {
                    updateTransferItemsInput();
                });
        </script>
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
            
            .revisionItem
            {
                padding-left:18px;
                background-image:url('/umbraco/images/umbraco/<%=UIConfiguration.RevisionTreeIcon %>');
            }
            
            small.selects
            {
                display:inline-block;
                padding:5px 0px 0px 0px;
            }
        </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<umb:UmbracoPanel Text="Location" runat="server" hasMenu="false" ID="panel">
<umb:Pane Text="Details" runat="server" ID="paneRepoDetails">

    <umb:PropertyPanel ID="ppName" runat="server" Text="Name">
         <%= Repository.Name %>
    </umb:PropertyPanel>

    <umb:PropertyPanel ID="ppType" runat="server" Text="Description">
         <%= Repository.Provider.Description%>
    </umb:PropertyPanel>

    <umb:PropertyPanel ID="ppFolder" runat="server" Text="Description">
         <%= Folder%>
    </umb:PropertyPanel>

    <umb:PropertyPanel runat="server" Text=" ">
        <strong>This is an external location, outside of the umbraco website you are currently viewing.</strong> You can connect to different locations, to transfer contents to your local installation.<br />
        A location is typically another Umbraco website, containing different "sets" of predefined content you can download to install locally.
        <br /><br />
        Below is a list of the available sets of content you can download. Simply click "transfer" to move it to your local installation, where you can then view and install it.
    </umb:PropertyPanel>
</umb:Pane>


<umb:Pane Text="Available sets of content" runat="server" ID="paneRepoRevisions">
    <input id="revisionsToTransfer" type="hidden" name="revisionsToTransfer" />
    <asp:repeater runat="server" id="rp_revisions">
        <headerTemplate>
            <table>
        </headerTemplate>

        <itemtemplate>            
            <tr>
                <td>
                    <span style="<%# Container.DataItem.ToString().StartsWith("/") ? "" : "display:none;" %>" class="folderItem">
                        <a href="<%= UIConfiguration.RepositoryEditPage%>?repo=<%= Repository.Alias %>&folder=<%= !String.IsNullOrEmpty(Folder) ? Folder+"\\" : "" %><%# Container.DataItem.ToString().Trim('/') %>"><%# Container.DataItem.ToString().Trim('/') %></a>
                    </span>
                    <span class="revisionItem" style="<%# Container.DataItem.ToString().StartsWith("/") ? "display:none;" : "" %>">
                        <input type="checkbox" class="submitCb" value="<%# Container.DataItem.ToString().Trim('/') %>" onchange="updateTransferItemsInput()" />
                        <a href="<%= UIConfiguration.RemoteRevisionEditPage%>?revision=<%# Container.DataItem.ToString().Trim('/') %>&repo=<%= Repository.Alias %>&folder=<%= !String.IsNullOrEmpty(Folder) ? Folder+"\\" : "" %>"><%# Container.DataItem.ToString().Trim('/') %></a>
                     </span>
                </td>
            </tr>
        </itemtemplate>

        <footerTemplate>
            </table>
        </footerTemplate>
    </asp:repeater>
    <asp:Button runat="server" ClientIDMode="static" id="TransferSelectedRevisions" Text="Transfer" />
    <small class="selects">select: <a href="#all" onclick="return select(0);">none</a> | <a href="#none" onclick="return select(1);">all</a> | <a href="#inverse" onclick="return select(2);">inverse</a></small>
</umb:Pane>

</umb:UmbracoPanel>

</asp:Content>
