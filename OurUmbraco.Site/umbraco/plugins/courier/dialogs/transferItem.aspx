<%@ Page MasterPageFile="../MasterPages/CourierDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="transferItem.aspx.cs" Inherits="Umbraco.Courier.UI.Dialogs.transferItem" %>
<%@ Register Src="../usercontrols/SystemItemSelector.ascx" TagName="SystemItemSelector" TagPrefix="courier" %>
<%@ Register Src="../usercontrols/DependencySelector.ascx" TagName="DependencySelector" TagPrefix="courier" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        jQuery(document).ready(function () {

            var page = "<%= Umbraco.Courier.UI.UIConfiguration.UpdateDialog %>";
            var id = "<%= Request["itemid"] %>";
            var provider = "<%= Request["providerguid"] %>";

            jQuery("button.go").click(function(){
                
                if (jQuery("#rb_extract").attr("checked"))
                    page = "<%= Umbraco.Courier.UI.UIConfiguration.ExtractDialog %>";
            
                var p = page + "?providerGuid=" + provider + "&itemid=" + id;
                window.location.href = p;

                return false;
            });        
        });
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    
    <umb:Pane runat="server" ID="paneNoRepositoriesAvailable" Text="No Repositories available" Visible="false">
        <p>Please set a valid reposity (target for the transfer), without repositories you can't transfer an item</p>
    </umb:Pane>

    <!-- Step 1 Select Items -->
    <umb:Pane runat="server" ID="paneSelectItems" Text="What do you want to do?">

       <div>
        <span style="float: left; margin: 10px;"><input id="rb_extract" type="radio" name="mode" value="extract" checked="checked"></span>
        <div style="float: left; padding: 0px 10px 10px 10px;">
            <h3 style="padding-top: 0px;">Deploy</h3>
           <p>Extract the contents of this item and deploy it <strong>to</strong> another location</p>
        </div>

        <br style="clear: both" />

        <span style="float: left; margin: 10px;"><input id="rb_update" type="radio" name="mode" value="update"></span>
        <div style="float: left; padding: 0px 10px 10px 10px;">
            <h3 style="padding-top: 0px;">Update</h3>
            <p>Update the contents of this item with data <strong>from</strong> another location</p>
        </div>

       </div>     
    </umb:Pane>

    <p>
        <button class="go">Continue</button> <em> or </em> <a href="#" onclick="<% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;">Cancel</a>
    </p>
</asp:Content>
