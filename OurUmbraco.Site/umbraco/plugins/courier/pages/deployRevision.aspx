<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../MasterPages/CourierPage.Master" CodeBehind="deployRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.deployRevision" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <style type="text/css">
      .cb{float: left; clear: left;}
        
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
    
     
     .dependencies
    {
         border-left:1px dotted #ccc;
         padding:2px 2px 2px 7px;
         margin-bottom:7px;
         margin-left: 20px;
         margin-top: 2px;
         font-size: 10px;
         color: #666;
    }     
    h2.propertypaneTitel{padding-bottom: 10px !Important; padding-top: 10px;}   
    </style>



    <script type="text/javascript">
       function displayStatusModal(title, message) {
           $('button').attr('disabled', 'disabled');
           var id = $('#statusId').val();

           if (message == undefined)
               message = "Please wait while Courier loads";


           UmbClientMgr.openModalWindow('plugins/courier/pages/status.aspx?statusId=' + id +"&message="+message, title, true, 500, 450);
       }

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
       });

    </script>

</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<umb:UmbracoPanel ID="panel" Text="Deploy revision" hasMenu="false" runat="server">
<umb:Pane ID="p_done" runat="server" Visible="false">

<div class="umbSuccess">
    <h4>Your changes has been installed on this instance</h4>
    <p>
        All items in the revision were created and updated without any issues
    </p>
</div>

</umb:Pane>


<umb:Pane ID="p_intro" runat="server">
    
    <input type="hidden" name="statusId" id="statusId" value="<%=Guid.NewGuid() %>" />
    <div style="float: right; margin-left: 30px; padding: 20px; border-left: #999 1px dotted; text-align: center;">
        <asp:button id="deployButton" runat="server" onclick="deploy" text="Install" OnClientClick="displayStatusModal('Install status', 'Please wait while Courier installs your selected items, this can take some time, depending on the amount of items')" /><br />
        <small>Install selected items</small>
    </div>
    
    <p>
        Below lists what items will be updated and created as new items. You can deselect the items you do
        not wish to transfer. However, if the complete transfer depends on an item that does not exist already, you 
        cannot de-select it.
    </p>

    <span class="options">
           <small>Overwrite items that already exist:<asp:CheckBox id="cb_overwriteExistingItems" Checked="true" runat="server" /></small>
           <small>Overwrite existing dependencies:<asp:CheckBox id="cb_overwriteExistingDependencies" Checked="true" runat="server" /></small>
           <small>Overwrite existing resources:<asp:CheckBox id="cb_overwriteExistingResources" Checked="false" runat="server" /></small>
    </span>

</umb:Pane>




<asp:panel runat="server" id="p_updates">
<h2 class="propertypaneTitel">Items which will be updated</h2>
       <asp:Repeater id="rp_providers_updates" runat="server" OnItemDataBound="bindProvider">
       <ItemTemplate>
         <div class="revisionItemGroup">
            <h3 style='background-image: url(<%# umbraco.IO.IOHelper.ResolveUrl(((Umbraco.Courier.Core.ItemProvider)Container.DataItem).ProviderIcon) %>);'><asp:Literal ID="lt_name" runat="server" />
               <img src="/umbraco/images/expand.png" class="openProvider" style="FLOAT: right"/>
               <img src="/umbraco/images/collapse.png" class="closeProvider" style="FLOAT: right"/>
            </h3>
                        
            <ul class="revisionItems">
            <asp:Repeater ID="rp_changes" runat="server" OnItemDataBound="bindChanges">
            <ItemTemplate>
                <li class="revisionItem">
                <div class="cb" style="float:left;">
                    <asp:CheckBox ID="cb_transfer" Checked="true" runat="server" />
                </div>
                    <div>   
                       <asp:Literal ID="lt_item" runat="server" /><br />
                       <div class="dependencies"><asp:Literal ID="lt_desc" runat="server" /></div>
                       <asp:HiddenField ID="hf_key" runat="server" />
                    </div>
                </li>
            </ItemTemplate>
           </asp:Repeater>
           </ul>
          </div>
       </ItemTemplate>
       </asp:Repeater>
</asp:panel>

<asp:panel runat="server" id="p_new">
<h2 class="propertypaneTitel">Items which will be created</h2>
  <asp:Repeater id="rp_providers_created" runat="server" OnItemDataBound="bindProvider">
       <ItemTemplate>
         <div class="revisionItemGroup">
            <h3 style='background-image: url(<%# umbraco.IO.IOHelper.ResolveUrl(((Umbraco.Courier.Core.ItemProvider)Container.DataItem).ProviderIcon) %>);'><asp:Literal ID="lt_name" runat="server" />
               <img src="/umbraco/images/expand.png" class="openProvider" style="FLOAT: right"/>
               <img src="/umbraco/images/collapse.png" class="closeProvider" style="FLOAT: right"/>
            </h3>
                        
            <ul class="revisionItems">
            <asp:Repeater ID="rp_changes" runat="server" OnItemDataBound="bindCreated">
            <ItemTemplate>
                <li class="revisionItem">
                <div class="cb" style="float:left;">
                    <asp:CheckBox ID="cb_transfer" Checked="true" runat="server" />
                </div>
                    <div>   
                       <asp:Literal ID="lt_item" runat="server" /><br />
                       <div class="dependencies"><asp:Literal ID="lt_desc" runat="server" /></div>
                       <asp:HiddenField ID="hf_key" runat="server" />
                    </div>
                </li>
            </ItemTemplate>
           </asp:Repeater>
           </ul>
          </div>
       </ItemTemplate>
       </asp:Repeater>
</asp:panel>

<asp:panel runat="server"  id="p_match">
<h2 class="propertypaneTitel">Items that already exist and did not change</h2>
  <asp:Repeater id="rp_providers_match" runat="server" OnItemDataBound="bindMatchProvider">
       
       <ItemTemplate>
         <div class="revisionItemGroup">
            <h3 style='background-image: url(<%# umbraco.IO.IOHelper.ResolveUrl(((Umbraco.Courier.Core.ItemProvider)Container.DataItem).ProviderIcon) %>);'><asp:Literal ID="lt_name" runat="server" />
               <img src="/umbraco/images/expand.png" class="openProvider" style="FLOAT: right"/>
               <img src="/umbraco/images/collapse.png" class="closeProvider" style="FLOAT: right"/>
            </h3>
                        
            <ul class="revisionItems">
            <asp:Repeater ID="rp_changes" runat="server" OnItemDataBound="bindMatches">
            <ItemTemplate>
                <li class="revisionItem">
                <div class="cb" style="float:left;">
                    <asp:CheckBox ID="cb_transfer" Checked="false" runat="server" />
                </div>
                    <div>   
                       <asp:Literal ID="lt_item" runat="server" /><br />
                       <asp:HiddenField ID="hf_key" runat="server" />
                    </div>
                </li>
            </ItemTemplate>
           </asp:Repeater>
           </ul>
          </div>
       </ItemTemplate>

       </asp:Repeater>
</asp:panel>


</umb:UmbracoPanel>
</asp:Content>