<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="ViewRevisionDetails.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.ViewRevisionDetails" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {

            $('.openProvider').click(function () {
                $(this).closest('.revisionItemGroup').find('.revisionItems').show(100);
                $(this).closest('h3').find('.openProvider').hide();
                $(this).closest('h3').find('.allDependencies').show();
                $(this).closest('h3').find('.closeProvider').show();
            });

            $('.closeProvider').hide().click(function () {
                $(this).closest('.revisionItemGroup').find('.revisionItems').hide(100);
                $(this).closest('h3').find('.openProvider').show();
                $(this).closest('h3').find('.allDependencies').hide();
                $(this).closest('h3').find('.closeProvider').hide();
            });

            $('.showItemDependencies').click(function () {
                $(this).hide();
                $(this).closest('li.revisionItem').find('.dependencies').show(100);
                $(this).closest('li.revisionItem').find('.hideItemDependencies').show();
            });

            $('.hideItemDependencies')
                .hide()
                .click(function () {
                    $(this).hide();
                    $(this).closest('li.revisionItem').find('.dependencies').hide(100);
                    $(this).closest('li.revisionItem').find('.showItemDependencies').show();
                });

            $('.hideAllDependencies').click(function () {
                $(this).closest('h2').next('div.propertypane').find('.dependencies').hide(100);
                $(this).closest('h2').next('div.propertypane').find('.showItemDependencies').show();
                $(this).closest('h2').next('div.propertypane').find('.hideItemDependencies').hide();
            });

            $('.showAllDependencies').click(function () {
                $(this).closest('h2').next('div.propertypane').find('.dependencies').show(100);
                $(this).closest('h2').next('div.propertypane').find('.showItemDependencies').hide();
                $(this).closest('h2').next('div.propertypane').find('.hideItemDependencies').show();
            });

            $('.showAll').click(function () {
                $('.revisionItems').find('ul.revisionItems').show(100);
                $('.revisionItems').find('.openProvider').hide();
                $('.revisionItems').find('.closeProvider').show();

                $('.revisionItems').find('.dependencies').show();
                $('.revisionItems').find('.allDependencies').show();
                $('.revisionItems').find('.showItemDependencies').hide();
                $('.revisionItems').find('.hideItemDependencies').show();
            });
            $('.hideAll').click(function () {
                $('.revisionItems').find('ul.revisionItems').hide(100);
                $('.revisionItems').find('.openProvider').show();
                $('.revisionItems').find('.closeProvider').hide();

                $('.revisionItems').find('.dependencies').hide();
                $('.revisionItems').find('.allDependencies').hide();
                $('.revisionItems').find('.showItemDependencies').show();
                $('.revisionItems').find('.hideItemDependencies').hide();
            });
        });

        var currentRevision = '<asp:literal runat="server" id="lt_revisionName" />';
        function displayStatusModal(title, message)
        {
            var val = $('#statusId').val();
            if (message == undefined)
                message = "Please wait while Courier loads";

            UmbClientMgr.openModalWindow('plugins/courier/pages/status.aspx?statusId=' + val +"&message=" +message, title, true, 500, 450);
        }

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

    </script>
    <style>
        .showItemDependencies, .hideItemDependencies
        {
            text-align:center;
            height:9px;
            margin-left:2px;
            display:inline-block;
            font-weight:bold;
            position:relative;
            top:1px;
            line-height:9px;
            padding-left: 9px;
            color: #999;
        }
        
        .openProvider, .closeProvider
        {   
            width:12px;
            text-align:center;
            height:12px;
            margin-right:4px;
            display:inline-block;
            font-weight:bold;
            line-height:10px;
        }
        
    .clickLink
    {
        color:#555599;
        cursor:pointer;
    }
    
    .dependencies
    {
        display:none;
    }
    
    .dependencies
    {
         border-left:1px dotted #ccc;
         padding:3px 3px 3px 7px;
         margin-bottom:7px;
         margin-left: 15px;
         margin-top: 2px;
    }
    .dependImage
    {
        position:relative;
        top:2px; 
        padding:0px 1px 1px 0px;
        width:10px;
    }
    
    small.dependTitle
    {
        display:block;
        padding-top:2px;
        color:#aaa;
    }
    
    .allDependencies
    {
        font-size:10px;
        font-weight:normal;
    }
    
    .label
    {
        width:100px;
        font-weight:bold;
        color:#888888;
        display:inline-block;
    }
    
    .showHideAll
    {
        font-size:11px;
        display:inline-block;
        position: absolute;
        right: 10px;
        top: 1px;
    }
    
    
    .revisionItemGroup
    {
        padding: 7px;
        background: url("/umbraco_client/tabView/images/background.gif") #EEE repeat-x bottom;
        border: 1px solid #ccc;
        margin-bottom: 3px;
    }
    .revisionItemGroup h3{font-size: 12px; font-weight: bold; padding: 0px 0px 0px 25px; margin: 0px; background: 3px 2px no-repeat;}
    
    ul.revisionItems
    {
        list-style: none; 
        display: none;
        background: #fff;
        margin: 7px 7px 7px 0px;
        padding: 7px;
        border: 1px solid #ccc;    
    }
    
    li.revisionItem{display: block; padding: 3px 3px 3px 0px; background: none;}
    li.revisionItem .toggleDependencies{font-size: 10px; color: #999; display: block;}
    
    li.revisionItem .clickLink{background:no-repeat 2px 0px; padding-left: 15px; color: #666}
    li.revisionItem .showItemDependencies{background-image: url(/umbraco_client/tree/themes/umbraco/fplus.gif)}
    li.revisionItem .hideItemDependencies{background-image: url(/umbraco_client/tree/themes/umbraco/fminus.gif)}
    
        div.action{width: 31%; float: left; padding: 0 1% 0 1%;}
        div.action p{line-height: 20px; margin-bottom: 25px}
        div.mid{border-left: #999 1px dotted; border-right: #999 1px dotted; width: 31%; float: left;}
        
        .action a{text-decoration: none; color: #999; height: 32px; display: block; padding: 2px 2px 2px 40px; background: url(/umbraco/dashboard/images/dmu.png) no-repeat left top; font-size: 12px}
        .action a:hover strong{text-decoration: underline;}
        .action a strong{display: block; padding-bottom: 3px;}
        
        a.transfer{background-image: url(../images/transfer.png)}
        a.install{background-image: url(../images/install.png)}
        a.edit{background-image: url(../images/edit.png)}
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
 <umb:UmbracoPanel runat="server" ID="panel" Text="Revision Details">
    
 <umb:Pane runat="server" ID="ActionsPanel" Text="Revision name">
       <div class="action">
            <a href="#" class="install" onclick="displayStatusModal('Compare status', 'Please wait while Courier compares the state of your installation with items in this revision');GotoExtractPage(); return false;">
                <strong>Compare and install</strong>
                <small>Determine what to install on this instance</small>
            </a>
       </div>

       <div class="action mid">
            <a href="#" class="edit" onclick="GotoEditPage(); return false;">
                <strong>Change contents</strong>
                <small>Select what items should be part of this</small>
            </a>
       </div>

       <div class="action">
            <a href="#" class="transfer" onclick="ShowTransferModal(); return false;">
                <strong>Transfer</strong>
                <small>Move this revision to another location</small>
            </a>
       </div>

 </umb:Pane>

    <umb:Pane runat="server" ID="NamePanel" Text="Revision name" Visible="false">
        <umb:PropertyPanel runat="server" Text="Name"><asp:Literal runat="server" Id="NameValue" /></umb:PropertyPanel>
        <umb:PropertyPanel runat="server" Text="Revision items"><asp:Literal runat="server" Id="RevisionCountValue" /></umb:PropertyPanel>
        <umb:PropertyPanel runat="server" Text="Resource items"><asp:Literal runat="server" Id="ResourceCountValue" /></umb:PropertyPanel>
        <umb:PropertyPanel runat="server" Text="Download as:"> 
            <asp:LinkButton runat="server" onclick="ArchiveDownloadClick">Zip archive</asp:LinkButton> | 
            <asp:LinkButton runat="server" onclick="CreateGraphClick">Xml Graph</asp:LinkButton> | 
            <asp:LinkButton runat="server" onclick="CreateMindMapClick">Mindmap</asp:LinkButton>
        </umb:PropertyPanel>
    </umb:Pane>    

<div class="revisionItems">
        
        <h2 class="propertypaneTitel" style="position: relative; height: 20px;">Items in this revision:
            <span class="showHideAll"><span class="clickLink showAll">expand/show all</span> | <span class="clickLink hideAll">collapse/hide all</span></span>
        </h2>
    
        <asp:Repeater runat="server" ID="RevisionProviderRepeater">
            <ItemTemplate>
                    <div class="revisionItemGroup">
                        <h3 style='background-image: url(<%# GetProviderIcon((Guid)Eval("Provider.Id")) %>);'><%# Eval("Provider.Name") + " ("+Eval("Items.Length")+")"%> 
                        <img src="/umbraco/images/expand.png" class="openProvider" style="FLOAT: right"/>
                        <img src="/umbraco/images/collapse.png" class="closeProvider" style="FLOAT: right"/>
                        </h3>
                        
                        
                        <ul class="revisionItems">
                                <asp:Repeater runat="server" DataSource=<%#Eval("Items")%>>
                                <ItemTemplate>

                                    <li class="revisionItem" itemId="<%#Eval("Item.ItemId.Id") %>" providerId="<%#Eval("Item.ItemId.ProviderId") %>">
                                                                            
                                        
                                        <%#Eval("Item.Name") %> 
                                        
                                        <span class="toggleDependencies" runat="server" visible=<%# IsVisible(Eval("DependsOn")) || IsVisible(Eval("Dependents")) %>>
                                                <span title="Show dependencies" class="clickLink showItemDependencies">Show dependencies</span>
                                                <span title="Hide dependencies" class="clickLink hideItemDependencies">Hide dependencies</span>
                                        </span>        

                                         <span class="toggleDependencies" runat="server" visible=<%# !IsVisible(Eval("DependsOn")) && !IsVisible(Eval("Dependents")) %>>
                                               No dependencies
                                         </span>    

                                            <div class="dependencies" runat=server visible=<%# IsVisible(Eval("DependsOn")) || IsVisible(Eval("Dependents")) %>>
                                                
                                                
                                                <div runat="server" visible=<%# this.IsVisible(Eval("DependsOn")) %>>
                                                    <div><small class="dependTitle">Depends on: (<%#Eval("DependsOn.Length") %>)</small></div>
                                                    <asp:Repeater runat="server" DataSource=<%#Eval("DependsOn")%>>
                                                        <ItemTemplate>
                                                            <div><%#(HasProviderIcon((Guid)Eval("Item.Provider.Id")) ? ("<img title='" + Eval("Item.Provider.Name") + "' class='dependImage' src='" + GetProviderIcon((Guid)Eval("Item.Provider.Id")) + "' />") : "")%> <%#Eval("Item.Name") %> <small><%#((bool)Eval("Dependency.IsChild")) ? "[child]" : "" %></small></div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>

                                                <div runat="server" visible=<%# this.IsVisible(Eval("Dependents")) %>>
                                                    <div><small class="dependTitle">Is depended on by: (<%#Eval("Dependents.Length") %>)</small></div>
                                                    <asp:Repeater runat="server" DataSource=<%#Eval("Dependents")%>>
                                                        <ItemTemplate>
                                                            <div><%#(HasProviderIcon((Guid)Eval("Item.Provider.Id")) ? ("<img title='" + Eval("Item.Provider.Name") + "' class='dependImage' src='" + GetProviderIcon((Guid)Eval("Item.Provider.Id")) + "' />") : "")%> <%#Eval("Item.Name") %> <small><%#((bool)Eval("Dependency.IsChild")) ? "[child]" : "" %></small></div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>

                                        </div>
                                    </li>


                                </ItemTemplate>
                            </asp:Repeater>
                                              
                        </ul>
                    </div>

            </ItemTemplate>
        </asp:Repeater>
    </div>
 </umb:UmbracoPanel>
</asp:Content>