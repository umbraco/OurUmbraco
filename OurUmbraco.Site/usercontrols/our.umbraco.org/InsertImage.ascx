<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InsertImage.ascx.cs"
    Inherits="OurUmbraco.Our.usercontrols.InsertImage" %>


<script type="text/javascript">

    <asp:PlaceHolder runat="server" ID="InsertImageMarkdown1">
        var imageInserted = false;
        var imageInserting = false;
        var imageLink;

        window.onbeforeunload = function () {
            // The Markdown callback expects a single parameter: the path to the image as a string.
            // PageDown creates an overlay when opening the insert image popup. To remove that overlay, we need to trigger the PageDown callback even when just closing the popup. onbeforeunload seems to be the answer.
            if (imageInserted) {
                window.opener.forumInsertImageCallback(imageLink);
            } else if (!imageInserting) {
                window.opener.forumInsertImageCallback(null);
            }
        };
    </asp:PlaceHolder>
    
    jQuery(document).ready(function () {

        jQuery("#insert").click(function () {
            InsertImage();
        });

        jQuery("#cancel").click(function () {
            CloseDialog();
        });

        <asp:PlaceHolder runat="server" ID="InsertImageMarkdown2">
            jQuery("#<%= btnUpload.ClientID %>").click(function () {
                imageInserting = true;
            });
        </asp:PlaceHolder>

        if (jQuery("#<%= tb_url.ClientID %>").val().length > 0) {
            ShowImage();
        }
        else {
            jQuery("#insert").attr("disabled", "disabled");

        }
        $("#<%= tb_url.ClientID %>").blur(function () {
            if (jQuery("#<%= tb_url.ClientID %>").val().length > 0) {
                if (ValidExtension()) {
                    ShowImage();
                }
                else {
                    jQuery("#notvalid").show();
                    HideImage();
                }
            } else { HideImage(); }
        });

    });

    function ValidExtension() {
        var ext = jQuery('#<%= tb_url.ClientID %>').val().split('.').pop().toLowerCase();
        var allow = new Array('gif', 'png', 'jpg', 'jpeg', 'bmp', 'tif', 'tiff');
        if (jQuery.inArray(ext, allow) == -1) {
            return false;
        }
        else {
            return true;
        }

    }

    function HideImage() {
        jQuery('#imagepreviewcontainer').hide();
        jQuery("#insert").attr("disabled", "disabled");
    }
    function ShowImage() {
        jQuery("#noimage").hide();
        jQuery("#notvalid").hide();
        jQuery('#imagepreviewcontainer').show();
        jQuery('#imagepreview').attr('src', jQuery('#<%= tb_url.ClientID %>').val());

        jQuery("#insert").removeAttr("disabled");

    }

    function InsertImage() {
        <asp:PlaceHolder runat="server" ID="InsertImageRte">
            imglink = jQuery('#<%= tb_url.ClientID %>').val();
            origimglink = imglink.replace('rs/', '');

            img = "<a href='" + origimglink + "' target='_blank'><img border='0' src='" + imglink + "' /></a>";

            tinyMCEPopup.editor.execCommand("mceInsertContent", false, img);
            CloseDialog();
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="InsertImageMarkdown3">
            // When we insert a image, we basically just close the dialog and let the unbeforeunload function do the rest of the work.
            imageLink = jQuery('#<%= tb_url.ClientID %>').val();
            imageInserted = true;
            CloseDialog();
        </asp:PlaceHolder>
    }
    function CloseDialog() {
        tinyMCEPopup.close();
    }
</script>

<fieldset>
    <legend>Image</legend>
    <div id="noimage">
    <p>No image uploaded yet.</p>
    </div>
    <div id="imagepreviewcontainer" style="display: none;">
        <img src="" id="imagepreview" width="200" />
    </div>
    <p style="display:none;">
        <asp:Label ID="Label1" AssociatedControlID="tb_url" CssClass="inputLabel" runat="server">Url:</asp:Label>
        <asp:TextBox ID="tb_url" runat="server"></asp:TextBox>
        <div id="notvalid" style="display:none;">Only images are valid (gif,png,jpg,jpeg,bmp,tif,tiff)</div>
    </p>
</fieldset>
<fieldset>
    <legend>Upload Image</legend>
    <p>
         <asp:Label ID="Label2" AssociatedControlID="FileUpload1" CssClass="inputLabel" runat="server">Image:</asp:Label>
        <asp:FileUpload ID="FileUpload1" runat="server" />
    </p>

    <p>
        <asp:Button ID="btnUpload" runat="server" Text="Upload Image" OnClick="btnUpload_Click" />
        
        <asp:Label ID="lb_notvalid" runat="server" Text="Not a valid image" Visible="false"></asp:Label>
        
    </p>
</fieldset>

<div class="mceActionPanel">
    <div style="float: left">

    <input type="button" id="insert" name="insert" value="insert" />
    
    </div> 
    
    <div style="float: right">
		<input type="button" id="cancel" name="cancel" value="cancel" />
	</div>

</div>
