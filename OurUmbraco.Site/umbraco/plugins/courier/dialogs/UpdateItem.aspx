<%@ Page MasterPageFile="../MasterPages/CourierDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="UpdateItem.aspx.cs" Inherits="Umbraco.Courier.UI.Dialogs.UpdateItem" %>
<%@ Register Src="../usercontrols/SystemItemSelector.ascx" TagName="SystemItemSelector" TagPrefix="courier" %>
<%@ Register Src="../usercontrols/DependencySelector.ascx" TagName="DependencySelector" TagPrefix="courier" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    
    <script type="text/javascript">

        var cb_all;
        var selectedChildItems;

        jQuery(document).ready(function () {

        /*
            cb_all = jQuery("#<%= cbTransferAllChildren.ClientID %>");
            selectedChildItems = jQuery("#sssd");
                        
            jQuery(".itemchb").change(function () {
                SetSelectedChildren();
            });*/

            jQuery("#buttons input").click(function () {
                jQuery("#buttons").hide();

                jQuery(".bar").show();
                var msg = jQuery("#loadingMsg").html();

                if (msg != '')
                    jQuery(".bar").find("small").text(msg);
            });
           
        });
                
        function SetSelectedChildren() {
            var ids = "";

            jQuery(".itemchb::checked").each(function () {
                ids += jQuery(this).attr("id") + ";";
            });
            selectedChildItems.val(ids);
        }
    </script>

    <style>
        #loadingMsg{display: none; !Important};
        .bar{text-align: center; padding-top: 10px;}
        .bar small{color: #999; display: block; padding: 10px; padding-top: 5px; text-align: center;}
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <umb:Pane runat="server" ID="paneNoRepositoriesAvailable" Text="No Repositories available" Visible="false">
        <p>Please set a valid location (target for the transfer), without any locations configured you can't transfer an item</p>
    </umb:Pane>

    <umb:Pane runat="server" ID="paneNoUpdatesAvailable" Text="No updates available" Visible="false">
        <div class="error">
            <p>Courier could not find any updates to this item at the selected location</p>
        </div>
    </umb:Pane>

    <asp:hiddenfield id="selectedDestination" runat="server" />

    <!-- Step 1 Select Destination -->
    <umb:Pane runat="server" ID="paneSelectDestination" Text="Select location" Visible="false">
        <p>Where should Courier get the updates from?</p>
        <asp:dropdownlist runat="server" ID="ddDestination"></asp:dropdownlist>
        <br />

        <div id="loadingMsg">Connecting to location...</div>
    </umb:Pane>

    <!-- Step 2 Select Items -->
    <umb:Pane runat="server" ID="paneSelectItems" Text="Update items" Visible="false" >
        <asp:placeholder runat="server" ID="phChildSelection" visible="false">
            
            <div class="notice">
                <p>
                    The item you have selected, has child items available, do you want to transfer all of them or only this item?
                </p>
            </div>

            <br />

            <!-- Only needed when the item has children -->
             <asp:checkbox runat="server" ID="cbTransferAllChildren" Text="Transfer this item and all it's children" Checked="true"></asp:checkbox>
        </asp:placeholder>

        <div id="loadingMsg">Checking dependencies and resources...</div>
    </umb:Pane>


     <!-- Step 3 Select Dependencies -->
    <umb:Pane runat="server" ID="paneSelectDependencies" Visible="false" Text="Should anything be overwritten?">

        <p>Should dependencies and resources be updated at the same time?</p>
        
        <p>
        <asp:checkbox runat="server" ID="cbOverWriteDeps" Text="Overwrite existing dependencies" Checked="true"></asp:checkbox>
        </p>

        <p>
        <asp:checkbox runat="server" ID="cbOverWriteFiles" Text="Overwrite existing files" Checked="true"></asp:checkbox>
        </p>

        <div id="loadingMsg">Downloading changes from location, this could take some time...</div>
    </umb:Pane>


     <!-- Step 4 run the update -->
    <umb:Pane runat="server" ID="paneRunUpdate" Visible="false" Text="Ready to update your selection">
        <p>
            Courier have collected all the required items from the selected location. Click the <strong>continue</strong>
            button below to update your installation.
        </p>

        <div id="loadingMsg">Updating your installation...</div>
    </umb:Pane>

     <!-- Step 4 run the update -->
    <umb:Pane runat="server" ID="paneDone" Visible="false" Text="Items have been updated">
        <p>
            Courier has extracted all the selected items, and updated those affected by your selection. You can now close this window
            and refresh your editor pages to view the changes.
        </p>
    </umb:Pane>

    <asp:placeholder runat="server" id="phButtons">
    <div id="buttons">
        <asp:button runat="server" text="Continue" onclick="go"/>
        <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="<% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;"><%= umbraco.ui.Text("cancel") %></a>
    </div>

    <div class="bar" style="display: none" align="center">
        <img src="/umbraco_client/images/progressbar.gif" alt="loading" />
        <small></small>
    </div>
    </asp:placeholder>


    
</asp:Content>