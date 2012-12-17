<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="../MasterPages/CourierPage.Master" CodeBehind="deployRevisions.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.deployRevisions" %>

<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        td.order
        {
            padding-right:6px;
        }
        
        td.title
        {
            padding-left:6px;
        }
        
        tr.revison
        {
            margin-bottom:2px;
        }
        
        .arrow
        {
            cursor:pointer;
            width:14px;
            cursor:hand;
            border:1px solid #aaaaaa;
        }

        .arrowUp
        {
            background:url(/umbraco/images/umbraco/bullet_arrow_up.png) center center no-repeat;
        }
        
        .arrowDown
        {
            background:url(/umbraco/images/umbraco/bullet_arrow_down.png) center center no-repeat;
        }
        
        .disabled
        {
            cursor:default;
            border: 1px solid #ffffff;
            background:inherit;
        }
    </style>

    <script>
        $(document).ready(function () {
            initializeArrows();
            updateRows();
        });

        function deployRevisions() {
            var statusId = $('#StatusIdField').val();
            UmbClientMgr.openModalWindow('plugins/courier/pages/status.aspx?statusId='+statusId+'&message=Deploying <%=ActionRevisions.Count() %> revisions', 'Direct Deploy Status', true, 500, 450);
        }

        function initializeArrows() {
            var order = 0;
            $('.revision').each(function () {
                $(this).attr('order', order++);
            });

            $('.revision .arrowUp').click(function () {
                if (!$(this).hasClass('disabled')) {
                    var current = $(this).closest('.revision');
                    var previous = current.prev();
                    previous.before(current);
                    updateRows();
                }
            });

            $('.revision .arrowDown').click(function () {
                if (!$(this).hasClass('disabled')) {
                    var current = $(this).closest('.revision');
                    var next = current.next();
                    next.after(current);
                    updateRows();
                }
            });
        }

        function updateRows() {
            $('.revision .arrow').removeClass('disabled');
            $('.revision:first .arrowUp').addClass('disabled');
            $('.revision:last .arrowDown').addClass('disabled');

            var revisions = [];
            $('.revision .title').each(function () {
                revisions.push($(this).text());
            });
            var count = 1;
            $('.revision .order').each(function () {
                $(this).html(count++);
            });
            $('#actionRevisions').val(revisions);
        }
    </script>
</asp:Content>
<asp:Content ID="ContentBody" ContentPlaceHolderID="body" runat="server">
    <asp:HiddenField runat="server" Id="StatusIdField" ClientIDMode="Static"/>
    <input type="hidden" name="actionRevisions" id="actionRevisions" />
    <umb:UmbracoPanel ID="panel2" Text="Deploy revision" hasMenu="false" runat="server">
        <umb:Pane ID="p_intro" runat="server" Visible="false">
            <div style="float: right; margin-left: 30px; padding: 20px; border-left: #999 1px dotted; text-align: center;">
                <asp:button id="deployButton" runat="server" text="Install" OnClientClick="deployRevisions();" /><br />
                <small>Install revisions</small>
            </div>
    
            <p>
                Below lists the revisions that will be installed, please review and if necessary correct the order.
            </p>

            <span class="options">
                   <small><strong>Overwrite:</strong></small>
                   <small><asp:CheckBox id="cb_overwriteExistingItems" Checked="true" runat="server" />Items that already exist <strong>|</strong> </small>  
                   <small><asp:CheckBox id="cb_overwriteExistingDependencies" Checked="true" runat="server" />Existing dependencies</small> <strong>|</strong> 
                   <small><asp:CheckBox id="cb_overwriteExistingResources" Checked="false" runat="server" />Existing resources</small>
            </span>

        </umb:Pane>
        <umb:Pane ID="p_start" runat="server" Visible="false">
            <h3>Revisons to deploy</h3>
            <br />
            <table>
                <asp:Repeater runat="server" Id="rRevisionSort">
                    <ItemTemplate>
                        <tr class="revision">
                            <td class="order">
                            </td>
                            <td class="arrow arrowUp">
                            </td>
                            <td class="arrow arrowDown">
                            </td>
                            <td class="title"><%#Container.DataItem.ToString() %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </table>
        </umb:Pane>
        <umb:Pane ID="p_done" runat="server" Visible="false">
            <div class="umbSuccess">
                <h4>
                    All the revisions have been deployed</h4>
                <p>
                    All the items in the revisions below were created and updated without any issues.
                    <br /><br />
                    <ul>
                    <asp:Repeater runat="server" Id="rRevision">
                        <ItemTemplate>
                            <li><%#Container.DataItem.ToString() %></li>
                        </ItemTemplate>
                    </asp:Repeater>    
                    </ul>            
                </p>
            </div>
        </umb:Pane>
    </umb:UmbracoPanel>
</asp:Content>
