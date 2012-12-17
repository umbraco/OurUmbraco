<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="ViewRevisions.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.ViewRevisions" %>
<%@ Import Namespace="Umbraco.Courier.UI" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
                function updateActionItems()
                {
                    var toTransfer = [];
                    $('.submitCb:checked').each(function()
                    {
                        toTransfer.push($(this).val());
                    });
                    $('#actionRevisions').val(toTransfer);

                    $('#DeleteSelectedRevisions').attr('disabled',toTransfer.length == 0);
                    // $('#MergeSelectedRevisions').attr('disabled',toTransfer.length < 2);
                    $('#DeploySelectedRevisions').attr('disabled',toTransfer.length == 0);
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
                    updateActionItems();
                    return false;
                }
                $(document).ready(function () {
                    updateActionItems();
                });
        </script>
        <style>
            .revisionItem
            {
                display:inline-block;
                background-repeat:no-repeat;
                background-position:left center;
                height:22px;
                padding-left:18px;
                background-image:url('/umbraco/images/umbraco/<%=UIConfiguration.RevisionTreeIcon %>');
            }
            
            tr.row td
            {
                padding-right:20px;
            }
            small.selects
            {
                display:inline-block;
                padding:5px 0px 0px 0px;
            }
            
        </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<umb:UmbracoPanel Text="Revisions" runat="server" hasMenu="false" ID="panel">

<umb:Pane Text="Available revisions" runat="server" ID="paneRepoRevisions">
    <input id="actionRevisions" type="hidden" name="actionRevisions" />
    <asp:repeater runat="server" id="rp_revisions">
        <headerTemplate>
            <table>
        </headerTemplate>

        <itemtemplate>            
            <tr class="row">
                <td>
                    <span class="revisionItem">
                        <input type="checkbox" class="submitCb" value="<%# Container.DataItem.ToString() %>" onchange="updateActionItems()" />
                        <a href="<%=UIConfiguration.RevisionViewPage %>?revision=<%# Container.DataItem.ToString() %>"><%# Container.DataItem.ToString() %></a>
                     </span>
                </td>
            </tr>
        </itemtemplate>

        <footerTemplate>
            </table>
        </footerTemplate>
    </asp:repeater>
    <asp:Button runat="server" ClientIDMode="static" id="DeleteSelectedRevisions" Text="Delete" 
        OnClientClick="return confirm('Are you sure you want to remove the selected revisions?', 'Removal confimation');" />
    <asp:Button runat="server" ClientIDMode="static" id="DeploySelectedRevisions" Text="Direct Deploy" />

    <small class="selects">select: <a href="#all" onclick="return select(0);">none</a> | <a href="#none" onclick="return select(1);">all</a> | <a href="#inverse" onclick="return select(2);">inverse</a></small>
</umb:Pane>

</umb:UmbracoPanel>

</asp:Content>
