<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="../MasterPages/CourierPage.Master"  CodeBehind="editLocalRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.EditRevisions" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<%@ Register Src="../usercontrols/RevisionContentsOverview.ascx" TagName="RevisionContents" TagPrefix="courier" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

        <style type="text/css">
            #simplemodal-container
            {
                width:420px;
                height:440px;
            }
        </style>

    <script type="text/javascript">
        
        var currentManifest = <%= ManifestJson %>;

        jQuery(document).ready(function () {
   
            BuildExistingContents();

            CheckMenuButtons();
            jQuery('.items input[type=checkbox]').change(
                function () {
                    CheckMenuButtons();
                }
            );

            jQuery(".itemCount").live('click',function(){
                
                jQuery(".hiddenItems", jQuery(this).parents(".providerContentsDetails")).toggle(100);
            });

            jQuery("a.toggleCB").click(function(){
                
            var div = jQuery(this).parents(".propertyItem").get(0);


            if (jQuery('input[type=checkbox]', div).filter(':checked').length > 0) {
                jQuery('input[type=checkbox]', div).attr("checked", "");
            } else {
               jQuery('input[type=checkbox]', div).attr("checked", "checked");
            }

            });


            jQuery(".deleteItem").live("click", function() {

                jQuery(this).parents('.providerContentsDetails').removeAttr("includeall");
                jQuery(this).parent().remove();
               

                SetSelectedContentIDS();

            });

            jQuery(".addItems").click(function(){

                 var src = "../dialogs/addItemsToLocalRevision.aspx?providerId=" + jQuery(this).attr("rel");
                jQuery.modal('<iframe src="' + src + '" height="440" width="410" style="border:0">');

            });
        });


        function BuildExistingContents()
        {
            if(currentManifest.Providers.length > 0)
            {
                 for (var p = 0; p < currentManifest.Providers.length; p++) { 

           
                        if(currentManifest.Providers[p].Items.length > 0)
                        {
                            var provId = currentManifest.Providers[p].Id;

                            jQuery(".WhatToPackage",jQuery("#"+provId)).val(currentManifest.Providers[p].DependecyLevel);

                            for (var i = 0; i < currentManifest.Providers[p].Items.length; i++) { 
                       
                                var item =  currentManifest.Providers[p].Items[i];
                                var name = item.Name;
                                var id = item.Id;



                            var cur = jQuery('<div/>').attr("id",item.Id).attr("class", "selectedItem").attr("includeChildren",item.IncludeChildren).appendTo(jQuery("#"+provId +'providerItems'));
                            
                            jQuery('<span/>').attr("class", "name").text(item.Name).appendTo(cur);
                            jQuery('<span/>').attr("class", "deleteItem").text("remove").appendTo(cur);
                            jQuery('<div/>').attr("class", "providerItems").attr("id", item.Id).appendTo(cur);
                       }

                       if(jQuery("#"+provId + " .selectedItem").size() > 0)
                       {
                            jQuery(".itemCount","#"+provId).text("(" + jQuery("#"+provId + " .selectedItem").size() + " items added)");
                       }


                        }
                }
            
             SetSelectedContentIDS();

            }



        }

        //only enable 'package selected' button once there is something selected
        function CheckMenuButtons() {
            if ('<%= Revision.RevisionCollection.Count()  %>' == 0) {
                jQuery(".classPackageSelected").hide();
                jQuery("#noContents").show();
            }


            if (jQuery('.selectedItem').length > 0) {
                jQuery(".classPackageSelected").show();
            } else {
                jQuery(".classPackageSelected").hide();
            }
        }


        function GotoExtractPage() {
            window.location = 'extractRevision.aspx?revision=<%= Request["revision"] %>';
        }

        function ShowTransferModal() {
            
            <% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>
            var nodeId = UmbClientMgr.mainTree().getActionNode().nodeId;
            UmbClientMgr.openModalWindow('plugins/courier/dialogs/transferRevision.aspx?revision= <%= Request["revision"] %>', 'Transfer Revision', true, 400, 200);
            <% }else{ %>
            openModal('plugins/courier/dialogs/transferRevision.aspx?revision=<%= Request["revision"] %>', 'Transfer Revision', 200, 400);
            <% } %>
        }

        function CloseModal()
        {
            jQuery.modal.close();
        }

        function ShowAddItemsModal()
        {
             var src = "../dialogs/addItemsToLocalRevision.aspx";
             jQuery.modal('<iframe src="' + src + '" height="440" width="410" style="border:0">');

        }

        function AddItemsToPackage(providerId,selectAll,items)
        {

            for (var i = 0; i < items.length; i++) { 
               
               if(jQuery("#"+items[i].Id ,"#"+providerId).size() == 0)
               {
                    var parentId = providerId;
                    if(items[i].ParentId != ""){
                        parentId = items[i].ParentId;
                    }

                    if(jQuery("#"+parentId).length == 0)
                    {
                       parentId = providerId;
                    }
                        
                    var cur = jQuery('<div/>').attr("class", "selectedItem").attr("id",items[i].Id).appendTo(jQuery("#"+parentId +'providerItems'));

                    cur.attr("includeChildren",items[i].IncludeChildren);
                    jQuery('<span>').attr("name", items[i].Name).text(items[i].Name).appendTo(cur);
                    jQuery('<span/>').attr("class", "deleteItem").text("remove").appendTo(cur);
                    jQuery('<div/>').attr("class", "providerItems").attr("id",items[i].Id + "providerItems").appendTo(cur);
                }
                else
                {
                    jQuery("#"+items[i].Id ,"#"+providerId).removeAttr("includeChildren");
                    jQuery("#"+items[i].Id ,"#"+providerId).attr("includeChildren",items[i].IncludeChildren);
                }
            }            

           if(jQuery("#"+providerId + " .selectedItem").size() > 0)
           {
                jQuery(".itemCount","#"+providerId).text("(" + jQuery("#"+providerId + " .selectedItem").size() + " items added)");
           }
           
           if(selectAll)
           {
                jQuery("#"+providerId).attr("includeAll",selectAll);
                jQuery("input","#"+providerId).attr('checked', true);
           }

           SetSelectedContentIDS();

           CloseModal();
        }

        function SetSelectedContentIDS()
        {
            var ids = "";


            jQuery(".selectedItem").each(function () {

            ids += jQuery(this).attr("id") + ";";

            });

            jQuery("#<%= txtSelectedContentIDs.ClientID %>").val(ids);

            UpdateItemCount();
            CheckMenuButtons();
        }

        function UpdateItemCount()
        {
             jQuery(".providerItems").each(function () {

                if(jQuery(".selectedItem",this).size() > 0)
                {
                    jQuery(".itemCount",jQuery(this).parent()).text("(" + jQuery(".selectedItem",this).size() + " items added)");
                }
                else
                {
                     jQuery(".itemCount",jQuery(this).parent()).text("");
                }
             });
       
        }


        function displayStatusModal(title,message)
        {
            //build manifest json
            buildManifestJson();

            var id = $('#statusId').val();

            if (message == undefined)
                message = "Please wait while Courier loads";


            UmbClientMgr.openModalWindow('plugins/courier/pages/status.aspx?statusId='+id +"&message="+message, title, true, 500, 450);
        }

        function buildManifestJson()
        {
            var Manifest = {};
            Manifest.Providers = [];

            var providers = new Array();
            var c = 0;
            jQuery(".providerContentsDetails").each(function () {
                
                var provId = jQuery(this).attr("id");
                var provIncludeAll = jQuery(this).attr("includeall") ? jQuery(this).attr("includeall") :  false;
                var provDependencyLevel = jQuery(".WhatToPackage",this).val();

                var Items = [];

                jQuery(".selectedItem",this).each(function () {

                    var itemID = jQuery(this).attr("id");
                    var itemName = jQuery(".name",this).html();
                    var itemIncludeChildren = jQuery(this).attr("includechildren") ? jQuery(this).attr("includechildren") :  false;

                    Items.push({
                        "Name": itemName,
                        "Id": itemID,
                        "ProviderId": provId,
                        "IncludeChildren": itemIncludeChildren,
                     }); 

                });

                if(Items.length > 0)
                {

                   
                    Manifest.Providers.push({
                        "Id": provId,
                        "IncludeAll": provIncludeAll,
                        "DependecyLevel":provDependencyLevel,
                        "Items": Items,                    
                    });

                
                   
                }
            });

            
            jQuery("#manifestJson").val(JSON.stringify(Manifest));
        }

        function packageAll()
        {
            if (!confirm('Are you sure you want to package everything?\r\n\r\nYou can click the Add texts on the right of each row below to add specific items to package.\r\nAnd use the dropdowns to refine even further what is packaged. '))
                return false; 

            displayStatusModal('Package all status', 'Please wait while Courier collects all available items and their dependencies'); 
            return true;
         }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<input type="hidden" name="statusId"  id="statusId" value="<%= Guid.NewGuid() %>" />
<input type="hidden" name="manifestJson" id="manifestJson" value="" />


<umb:UmbracoPanel ID="panel" Text="Revision" runat="server" hasMenu="false">

<umb:Pane  runat="server" ID="addPane">

    <div class="classPackageSelected" style="float: right; margin-left: 30px; padding: 20px; border-left: #999 1px dotted; text-align: center;">
        <asp:button id="packageSelectedButton" runat="server"  text="Package selected" /><br />
        <small>Only add selected items</small>
    </div>

    <div style="float: right; margin-left: 30px; padding: 20px; border-left: #999 1px dotted; text-align: center;">
        <asp:button id="packageAllButton" runat="server"  text="Package all"  /><br />
        <small>Add everything available</small>
    </div>

    <p>
        Choose from the different types of content below, to select the items you want to include in the this revision.
    </p>

    <p>
        You can choose to automaticly add all detected dependecies automaticly, or you can for each individual type of content
        decide the level of inclusion. This gives you great control over what actually gets included and deployed.
    </p>

    

</umb:Pane>

<umb:Pane Text="Current contents" runat="server" ID="paneContents" Visible="false">
    <courier:RevisionContents runat="server" ID="RevisionContents">
    </courier:RevisionContents>

    <p id="noContents" style="display:none;">Currently this revision doesn't contain any contents, please move to the package tab to select and package the desired contents.</p>
</umb:Pane>


<asp:panel runat="server" ID="pnlProviderContents">
<div id="addProviderContents">
    <select style="background-color:#DDD;border:1px solid #999" onchange="$('.WhatToPackage').val($(this).val())"><option value="0">Added + Dependencies</option><option value="1">Selected items only</option><option value="2">Selected + 1 Dependency level</option><option value="3">Selected + 2 Dependency levels</option><option value="4">Selected + 3 Dependency levels</option></select>
</div>

<div id="prodiderContents">
<asp:Repeater ID="rp_providers" runat="server" >
        <ItemTemplate>
         <div class="providerContentsDetails" id="<%#((Umbraco.Courier.Core.ItemProvider)Container.DataItem).Id %>">
            <div class="providerName">
            
            
                <%#"<img style='position:relative;top:2px;left:-2px;' src='" + umbraco.IO.IOHelper.ResolveUrl(((Umbraco.Courier.Core.ItemProvider)Container.DataItem).ProviderIcon) + "' /> " + ((Umbraco.Courier.Core.ItemProvider)Container.DataItem).Name %>
                <span class="itemCount"></span>
                <span style="float:right;position:relative;margin-left:20px;"><select name="whatToPackage<%# ((Umbraco.Courier.Core.ItemProvider)Container.DataItem).Id %>" style="background-color:#EEE;border:1px solid #AAA" class="WhatToPackage"><option value="0">Added + Dependencies</option><option value="1">Selected items only</option><option value="2">Selected + 1 Dependency level</option><option value="3">Selected + 2 Dependency levels</option><option value="4">Selected + 3 Dependency levels</option></select></span>
                <button class="addItems" style="font-size:12px;margin-top:-3px;" rel="<%# ((Umbraco.Courier.Core.ItemProvider)Container.DataItem).Id %>" onclick="return false;">Add</button>
            </div>
            <div class="providerItems hiddenItems" id="<%# ((Umbraco.Courier.Core.ItemProvider)Container.DataItem).Id %>providerItems">
            
            </div>
         </div>
            
        </ItemTemplate>
</asp:Repeater>

<div style="display:none;">
<asp:TextBox runat="server" ID="txtSelectedContentIDs"></asp:TextBox>
</div>
</div>

</asp:panel>
</umb:UmbracoPanel>

</asp:Content>