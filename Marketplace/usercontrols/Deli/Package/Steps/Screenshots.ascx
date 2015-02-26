<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Screenshots.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Package.Steps.Screenshots" %>
<asp:PlaceHolder ID="holder" runat="server" Visible="false">
<script type="text/javascript">
    var swfu;

    window.onload = function () {
        swfu = new SWFUpload({
            // Backend Settings
            upload_url: "/umbraco/project/upload.aspx",
            post_params: {
                "ASPSESSID": "<%=Session.SessionID %>",
                "NODEGUID": "<%= ProjectGuid %>",
                "USERGUID": "<%= MemberGuid %>",
                "FILETYPE": "screenshot"
            },

            // Flash file settings
            file_size_limit: "10 MB",
            file_types: "*.*", 		// or you could use something like: "*.doc;*.wpd;*.pdf",
            file_types_description: "All Files",
            file_upload_limit: "0",
            file_queue_limit: "1",

            // Event handler settings
            swfupload_loaded_handler: swfUploadLoaded,

            file_dialog_start_handler: fileDialogStart,
            file_queued_handler: fileQueued,
            file_queue_error_handler: fileQueueError,
            file_dialog_complete_handler: fileDialogComplete,

            //upload_start_handler : uploadStart,	// I could do some client/JavaScript validation here, but I don't need to.
            upload_progress_handler: uploadProgress,
            upload_error_handler: uploadError,
            upload_success_handler: uploadSuccess,
            upload_complete_handler: uploadComplete,

            // Button Settings
            button_image_url: "XPButtonUploadText_61x22.png",
            button_placeholder_id: "spanButtonPlaceholder",
            button_width: 161,
            button_height: 22,
            button_text: '<span class="button">Select file <span class="buttonSmall">(10 MB Max)</span></span>',
            button_text_style: '.button { font-family: Helvetica, Arial, sans-serif; font-size: 14px; font-weight: bold; color: #2244BB; text-decoration: underline; } .buttonSmall { font-size: 10pt; }',


            // Flash Settings
            flash_url: "/scripts/swfupload/swfupload.swf", // Relative to this file

            custom_settings: {
                progress_target: "fsUploadProgress",
                upload_successful: false
            },

            // Debug settings
            debug: false
        });
    }


	</script>


<div class="form simpleForm" id="registrationForm">
    

<asp:Repeater ID="rp_screenshots" OnItemDataBound="OnFileBound" Visible="false" runat="server">
<ItemTemplate>
<tr>
<td>
  <asp:Image runat="server" ID="img_image" />
</td>
<td>
  <asp:Literal ID="lt_date" runat="server" />
</td>
<td class="center">
   <asp:Button ID="bt_default" runat="server"  OnCommand="SetDefaultImage" /> 
   <asp:Literal ID="lt_default" runat="server" /> 
</td>
<td class="center">
  <asp:Button ID="bt_delete" runat="server" OnClientClick="return confirm('Are you sure you want to delete this image?')" OnCommand="DeleteFile" Text="Delete" />
</td>
</tr>
</ItemTemplate>

<HeaderTemplate>
<fieldset>
<legend>Current Screenshots</legend>
<p>
<table class="dataTable">
<thead>
<tr>
  <th>Image</th>
  <th>Uploaded</th>
  <th class="center">Default</th>
  <th class="center">Delete</th>
</tr>
</thead>
<tbody>
</HeaderTemplate>
<FooterTemplate>
</tbody>
</table>
</p>
</fieldset>
</FooterTemplate>

</asp:Repeater>

    
    <fieldset>
    <legend>Upload file</legend>
    
    <div id="swfu_container">
    
    <div id="swfu_controls">
    
         <p>
            <label class="inputLabel">Pick file:</label>

            <div> 
				<div> 
					<input type="text" id="txtFileName" disabled="true" class="title" /> <span id="spanButtonPlaceholder"></span>(10 MB max)
				</div>
                 
				<div class="flash" id="fsUploadProgress"> 
					<!-- This is where the file progress gets shown.  SWFUpload doesn't update the UI directly.
								The Handlers (in handlers.js) process the upload events and make the UI updates --> 
				</div> 
				<input type="hidden" name="hidFileID" id="hidFileID" value="" /> 
				<!-- This is where the file ID is stored after SWFUpload uploads the file and gets the ID back from upload.php --> 
			</div>  
        </p>

        <p>
            <input type="button" value="Upload file" id="btn_submit" class="button tiny"/>
        </p>

    </div>
    </div>
    </fieldset>
    
    
    <div class="buttons">
        <asp:linkbutton runat="server" Text="Previous" ID="MovePrevious" OnClick="MoveLast"/>&nbsp;
        <asp:Button runat="server" Text="Next" ID="MoveNext" OnClick="SaveStep"  CssClass="submitButton button tiny green" />
    </div>			       
 </div>
 </asp:PlaceHolder>