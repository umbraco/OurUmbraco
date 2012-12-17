<%@ Page MasterPageFile="../MasterPages/CourierDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="CommitItem.aspx.cs" Inherits="Umbraco.Courier.UI.Dialogs.CommitItem" %>
<%@ Register Src="../usercontrols/SystemItemSelector.ascx" TagName="SystemItemSelector" TagPrefix="courier" %>
<%@ Register Src="../usercontrols/DependencySelector.ascx" TagName="DependencySelector" TagPrefix="courier" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        
        #resourceList ul, #selectList ul {list-style: none; padding-left: 5px; margin: 0px;}
        #resourceList ul li,  #selectList ul li{display: block; background: no-repeat 2px 2px; padding: 3px; padding-left: 20px; min-height: 12px;}
        
        #resourceList ul li img.expand{background-image: url(/umbraco/images/arrowDown.gif); width: 20px; height: 7px}
        #selectList ul li.file, #resourceList ul li.file{background-image: url(/umbraco/images/umbraco/developerScript.gif);}
        #selectList ul li.folder, #resourceList ul li.folder{background-image: url(/umbraco/images/umbraco/folder.gif);}
        
        #resourceList ul li a, #selectList ul li a{text-decoration: none; color: #333;}
        #selectList ul li{padding-left: 0px;}
        #selectList ul li a{display: block; padding-left: 20px;}
        #selectList ul li a:hover{background: no-repeat 0px 0px url(/umbraco/images/delete.small.png);}
        
        .resourcePicker{text-decoration: none; color: #333; display: block; padding: 10px; height: 16px; padding-left: 25px; background: no-repeat 3px 7px url(/umbraco/images/new.png); }
        
         #loadingMsg{display: none; !Important};
        .bar{text-align: center; padding-top: 10px;}
        .bar small{color: #999; display: block; padding: 10px; padding-top: 5px; text-align: center;}
    </style>
    
    <script type="text/javascript">

        var cb_all;
        var selectedChildItems;
        
        var tb_ChildItemIDs
        var ul_selectTedList

        jQuery(document).ready(function () {

            tb_ChildItemIDs = jQuery("input.tbResources");
            ul_selectTedList = jQuery("#selectList ul");

            cb_all = jQuery("#<%= cbTransferAllChildren.ClientID %>");
            selectedChildItems = jQuery("#<%= txtChildItemIDs.ClientID %>");


            cb_all.change(function () {
             
                if (!cb_all.attr("checked"))
                    jQuery("#specificChildSelection").show();
                else
                    jQuery("#specificChildSelection").hide();

                jQuery(this).blur();

            });

            jQuery(".itemchb").change(function () {
                SetSelectedChildren();
            });

            jQuery("#buttons input").click(function () {
                jQuery("#buttons").hide();

                jQuery(".bar").show();
                var msg = jQuery("#loadingMsg").html();

                if (msg != '')
                    jQuery(".bar").find("small").text(msg);
            });


            jQuery(".resourcePicker").click(function () {
                jQuery("#resourceListWrapper").show();
                updateResources("~/", jQuery("#resourceList"));
                return false;
            });
        });

        function SetSelectedChildren() {
            var ids = "";
            jQuery(".itemchb::checked").each(function () {
                ids += jQuery(this).attr("id") + ";";
            });
            selectedChildItems.val(ids);
        }


        function updateResources(s_root, parentDom) {
            jQuery.ajax({
                type: "POST",
                url: "CommitItem.aspx/GetFilesAndFolders",
                data: "{'root':'" + s_root + "'}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",

                success: function (meh) {

                    parentDom.children().remove();

                    var list = "<ul>";

                    jQuery(meh.d).each(function (index, domEle) {
                        var html = "";
                        if (domEle.Type == "file")
                            html = "<li class='file'><a href='#' onClick='addFile(\"" + domEle.Path + "\",\"" + domEle.Name + "\"); return false;'>" + domEle.Name + "</a></li>";
                        else
                            html = "<li class='folder'><a href='#'onClick='addFolder(\"" + domEle.Path + "\",\"" + domEle.Name + "\"); return false;'>" + domEle.Name + "</a><span><img class='expand' onclick='updateResources(\"" + domEle.Path + "\", jQuery(this.parentNode)); return false;' src='/umbraco/images/nada.gif'/></span></li>";

                        list += html;
                    });
                    
                    list += "</ul>";

                    parentDom.append(list);
                }
            });
        }



        function addFile(path, name) {

            var currentList = tb_ChildItemIDs.val();

            if (currentList.indexOf("|"+path) < 0) {

                var html = "<li class='file'><a href='#' onClick='remove(this,\"" + path + "\"); return false;'>" + name + "</a></li>";
                ul_selectTedList.append(html);
                tb_ChildItemIDs.val(tb_ChildItemIDs.val() + "|" + path);
            }

        }

        function addFolder(path, name) {

            var currentList = tb_ChildItemIDs.val();

            if (currentList.indexOf("|" + path) < 0) {
                var html = "<li class='folder'><a href='#' onClick='remove(this,\"" + path + "\"); return false;'>" + name + "</a></li>";
                ul_selectTedList.append(html);
                tb_ChildItemIDs.val(tb_ChildItemIDs.val() + "|" + path);
            }

        }

        function remove(elm, path) {
            var list = tb_ChildItemIDs.val().replace(path, "").replace("||", "|");
            tb_ChildItemIDs.val(list);
            jQuery(elm).parent().remove();
        }
    </script>

</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">

    <umb:Pane runat="server" ID="paneNoRepositoriesAvailable" Text="No Locations available" Visible="false">
        <p>Please set a valid reposity (target for the transfer), without repositories you can't transfer an item</p>
    </umb:Pane>

    <!-- Step 1 Select Items -->

    <umb:Pane runat="server" ID="paneSelectItems" Text="Deploy items" Visible="false">

    <div style="border-bottom: 1px solid #ccc">
        <asp:placeholder runat="server" ID="phSelectDestination" visible="false">
            
            <script type"text/javascript">
                jQuery(document).ready(function () {
                    jQuery(".submitButton").attr('disabled', 'disabled');

                    jQuery(".destinationDDL").change(function () {
                        var ddl = jQuery(this);
                        if (ddl.val() != '')
                            jQuery(".submitButton").removeAttr('disabled');
                        else
                            jQuery(".submitButton").attr('disabled', 'disabled');
                    });
                });        
                </script>

            <p>Please select where you wish to transfer this item to:</p> 
            <p><asp:dropdownlist runat="server" id="stepOneDDl" cssclass="destinationDDL" /></p>
        </asp:placeholder>
        
        <asp:placeholder runat="server" ID="phdefaultDestination" visible="false">
            <p>You are about to transfer this item to: <strong><asp:literal id="ltDestination" runat="server" /></strong></p>
        </asp:placeholder>
    </div>

        <asp:placeholder runat="server" ID="phChildSelection" visible="false">
            <p>
               The item you have selected, has child items available, do you want to transfer all of them or select specific items to transfer?
            </p>
           
            <!-- Only needed when the item has children -->


             <asp:checkbox runat="server" ID="cbTransferAllChildren" Text="Yes, transfer all children as well" Checked="true"></asp:checkbox>
             

             <div id="specificChildSelection" style="display: none">
                <div id="itemList">
                    <courier:SystemItemSelector ID="SystemItemSelector" runat="server" />
                </div>

                <div style="display:none;">
                    <asp:textbox runat="server" ID="txtChildItemIDs"></asp:textbox>
                </div>

                </div>

                <div id="loadingMsg">Connecting and transfering, please hold...</div>

        </asp:placeholder>
    </umb:Pane>


    <asp:placeholder runat="server" id="stepOneButtons" visible="false">
    <br />
    <div id="buttons">
        <asp:button runat="server" text="Deploy" cssclass="submitButton" onclick="oneSteptransfer"/>
        <em><%= umbraco.ui.Text("or") %></em> 
        <asp:linkbutton onclick="transfer" runat="server" text="Go to advanced settings" />
    </div>

    <div class="bar" style="display: none" align="center">
        <img src="/umbraco_client/images/progressbar.gif" alt="loading" />
        <small></small>
    </div>

    </asp:placeholder>

    

    
    <!-- Step 2 Select Dependencies -->
    <umb:Pane runat="server" ID="paneSelectDependencies" Visible="false" Text="Should anything be overwritten?">
        <p>The item you have selected has dependencies, should courier override existing items on the target
                or only deploy those that doesn't exist already.
        </p>
        
        <br />

        <p>
            <asp:checkbox runat="server" ID="cbOverWriteItems" Text="Overwrite existing items" Checked="true"></asp:checkbox>
        </p>

        <p>
            <asp:checkbox runat="server" ID="cbOverWriteDeps" Text="Overwrite existing dependencies" Checked="true"></asp:checkbox>
        </p>

        <p>
            <asp:checkbox runat="server" ID="cbOverWriteFiles" Text="Overwrite existing files" Checked="true"></asp:checkbox>
        </p>

        <div id="loadingMsg">Determining dependencies and resources...</div>
    </umb:Pane>
    
    <!-- Step 3 Select ´Resources -->
    <umb:Pane runat="server" ID="paneSelectResources" Visible="false" Text="Add additional files?">
        
        <p>If you know of any additional files required by this change, place add them below </p>
        
        <a href="#" class="resourcePicker">Add files or folders to deployment</a>

        <div style="position: relative; height: 270px; display: none; border: 1px solid #efefef;" id="resourceListWrapper">
            <div id="resourceList" style="position: absolute; top: 0px; left: 0px; border-left: #efefef 1px solid;  height: 250px; width: 290px; padding: 5px; padding-right: 0px; overflow: auto;"></div>
            
            <div id="selectList" style="position: absolute; top: 0px; right: 0px;  height: 250px; width: 250px; padding: 5px; overflow: auto;">
                <ul>
                   <li style="display: none;">.</li>
                </ul>
            </div>
        </div>

        <div style="display: none">
            <asp:textbox id="tb_Resources" class="tbResources" runat="server" />
        </div>

        <div id="loadingMsg">Adding additional resources...</div>
    </umb:Pane>
    
    <!-- Step 3 Select Destination -->
    <umb:Pane runat="server" ID="paneSelectDestination" Visible="false" Text="Select destination">
        <script type"text/javascript">
            jQuery(document).ready(function () {
                jQuery(".submitButton").attr('disabled', 'disabled');

                jQuery(".destinationDDL").change(function () {
                    var ddl = jQuery(this);
                    if(ddl.val() != '')
                        jQuery(".submitButton").removeAttr('disabled');
                    else
                        jQuery(".submitButton").attr('disabled', 'disabled');
                });
            });        
        </script>

        <p>Where should the items be transfered to?</p>
        <p>
            <asp:dropdownlist runat="server" ID="ddDestination" cssclass="destinationDDL" ></asp:dropdownlist>
        </p>
        
       


        <div id="loadingMsg">Connecting and transfering, please hold...</div>

    </umb:Pane>


    <!-- Step 4 Progress/status -->
    <umb:Pane runat="server" ID="paneStatus" Visible="false" Text="Done">
    <div class="success">
        <p>The item has been transfered</p>
    </div>
     <br />
     <a href="#" onclick="<% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;">Close</a>
    </umb:Pane>

    <br />

    <asp:placeholder runat="server" id="phButtons">

    <div id="buttons">
        <asp:button runat="server" text="Continue" cssclass="submitButton" onclick="transfer"/>
        <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="<% if (Umbraco.Courier.UI.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;"><%= umbraco.ui.Text("cancel") %></a>
    </div>

    
     <div class="bar" style="display: none" align="center">
        <img src="/umbraco_client/images/progressbar.gif" alt="loading" />
        <small></small>
    </div>

    </asp:placeholder>

</asp:Content>