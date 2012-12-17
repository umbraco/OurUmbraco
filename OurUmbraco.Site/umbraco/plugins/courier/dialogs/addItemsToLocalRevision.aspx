<%@ Page MasterPageFile="../MasterPages/CourierDialog.Master"  Language="C#" AutoEventWireup="true" CodeBehind="addItemsToLocalRevision.aspx.cs" Inherits="Umbraco.Courier.UI.Dialogs.addItemsToLocalRevision" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<%@ Register Src="../usercontrols/SystemItemSelector.ascx" TagName="SystemItemSelector" TagPrefix="courier" %>


<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript">

        $(document).ready(function () {


            $("#addItems").attr("disabled", "false");

            if ($(".item .item").length == 0) {

                $("#selectChildrenOption").hide();
            }

            SetSelectedProvider();
            ToggleSelectionTools();

            $(".itemchb").change(function () {

                //check if we need to check children
                if ($("#selectChildren").is(':checked')) {

                    if ($('.itemchb:first', this).is(':checked')) {

                        $('.itemchb:not(:first)', this).attr('checked', true);
                    }

                } else {

                    //remove include children from parent
                    $('.itemchb', $(this).parents("* [parent = '<%= ddProviders.SelectedValue  %>']")).removeAttr("includeChildren");
                }


                $(this).parent().attr('rel', $(this).is(':checked'));

                if (!$(this).is(':checked') && $("#selectAll").is(':checked')) {
                    $("#selectAll").attr('checked', false);
                } else {
                    if ($('.itemchb').length == $('.itemchb:checked').length) {
                        $("#selectAll").attr('checked', true);
                    }
                }

                SetSelectedItems();
            });

            $("#selectChildren").change(function () {
                var self = $(this);
                if (self.is(':checked')) {
                    var selector = self.closest("#providerItemsSelector");
                    selector
                        .find(".item .itemchb:checked")
                        .attr("includeChildren", 1)
                        .parent()
                        .find(".itemchb:checkbox")
                        .attr("checked", true);

                    $("#selectAll").attr('checked', selector.find('.itemchb:not(:checked)').length == 0);
                }
                SetSelectedItems();
            });

            $("#selectAll").change(function () {
                $('.itemchb').attr('checked', $(this).is(':checked'));
                SetSelectedItems();
            });

        });

        function ToggleSelectionTools() {
            if ($(".item").length == 0) {
                $("#selectionOptions").hide();
            } else {
                $("#selectionOptions").show();
            }
           
        }
         
         function SetSelectedProvider() {

             $("#selectedProvider").val($("#<%= ddProviders.ClientID  %>").val());
         }


         var SelectedItems;

         function SetSelectedItems() {
             $("#selectedItems").val("");

             var ids = "";

             SelectedItems = [];

             $(".item").each(function () {

                 if ($('.itemchb:first', this).is(':checked')) {

                     var itemName = $('.itemchb', this).attr("rel");
                     var itemID = $('.itemchb', this).attr("id");
                     var parentID = $('.itemchb', this).attr("parent");
                     var includeChildren = false;

                     if($(this).children('.item').length > 0 &&
                        $(this).children('.item').length == $(this).children(".item [rel = 'true']").length)
                    {
                        includeChildren = true;
                    }

                     SelectedItems.push({
                         "Name": itemName,
                         "Id": itemID,
                         "ParentId": parentID,
                         "IncludeChildren": includeChildren
                     });

                     ids += itemID + ";";
                 }

             });

             if (ids != "") {
                 $("#selectedItems").val(ids);

                 $("#addItems").removeAttr("disabled");
             } else {
                 $("#addItems").attr("disabled", "false");
             }
         }


         

         function SubmitSelection() {
             var providerId = $("#selectedProvider").val();
             //var selectedItems = $("#selectedItems").val();

             var selectAll = $('#selectAll').is(':checked');

             //submit to parent
             parent.AddItemsToPackage(providerId,selectAll, SelectedItems);
         }

    </script>


</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">

    <umb:Pane runat="server" Text="Select a provider and then which item(s) to include">

    <div id="providerSelection">
    
    <umb:PropertyPanel runat="server" Text="Provider">
    <asp:DropDownList ID="ddProviders" runat="server" AutoPostBack="true"
            onselectedindexchanged="ddProviders_SelectedIndexChanged">
    </asp:DropDownList>

    </umb:PropertyPanel>

    </div>

    <br style="clear:both" />


    <div id="providerItemsSelector">
        <div id="itemList">

            <courier:SystemItemSelector ID="SystemItemSelector" runat="server" />
            
        </div>
        <div id="selectionOptions">
            <div id="selectAllOption">
                 <input type="checkbox" id="selectAll" value=""/> Select all
            </div>
            <div id="selectChildrenOption">
                <input type="checkbox" id="selectChildren" value=""/> Add selected items and their children
            </div>
        </div>    
    </div>


    </umb:Pane>

   

    <div id="selectionActions">
    
        <input type="submit" value="Add" id="addItems" onclick="SubmitSelection();return false" />
        or 
        <a href="#" onclick="parent.CloseModal(); return false;">cancel</a>
    
    </div>




    <input type="hidden" id="selectedProvider" />
    <input type="hidden" id="selectedItems" />
    
</asp:Content>