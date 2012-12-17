<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileUpload.ascx.cs" Inherits="uWiki.usercontrols.FileUpload" %>



<div class="notice" id="notLoggedIn" visible="false" runat="server"> 
<h4 style="text-align: center;"> 
Please <a href="/member/login">login</a> or <a href="/member/signup">sign up</a> to manage wiki attachments
</h4> 
</div> 

<asp:PlaceHolder ID="holder" runat="server" Visible="false">
<script type="text/javascript">
    var swfu;

    window.onload = function () {
        swfu = new SWFUpload({
            // Backend Settings
            upload_url: "/umbraco/wiki/upload.aspx",
            post_params: {
                "ASPSESSID": "<%=Session.SessionID %>",
                "NODEGUID": "<%= VersionGuid %>",
                "USERGUID": "<%= MemberGuid %>",
                "FILETYPE": jQuery("#wiki_fileType").val(),
                "UMBRACOVERSION": jQuery("#wiki_version").val()
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


    jQuery(document).ready(function () {
        jQuery("#wiki_fileType").change(function () {
            swfu.setPostParams({ ASPSESSID: "<%=Session.SessionID %>", NODEGUID: "<%= VersionGuid %>", USERGUID: "<%= MemberGuid %>", FILETYPE: jQuery("#wiki_fileType").val(), "UMBRACOVERSION": jQuery("#wiki_version").val() });
        })
        jQuery("#wiki_version").change(function () {
            swfu.setPostParams({ ASPSESSID: "<%=Session.SessionID %>", NODEGUID: "<%= VersionGuid %>", USERGUID: "<%= MemberGuid %>", FILETYPE: jQuery("#wiki_fileType").val(), "UMBRACOVERSION": jQuery("#wiki_version").val() });
        })
    });

	</script>


  <div class="form simpleForm" id="registrationForm">
    

<asp:Repeater ID="rp_files" OnItemDataBound="OnFileBound" Visible="false" runat="server">
<ItemTemplate>
<tr>
<td>
  <asp:Literal ID="lt_name" runat="server"></asp:Literal>
</td>
<td>
  <asp:Literal ID="lt_type" runat="server" />
</td>
<td>
  <asp:Literal ID="lt_version" runat="server" />
</td>
<td>
  <asp:Literal ID="lt_date" runat="server" />
</td>
<td>
  <asp:Button ID="bt_delete" runat="server" OnClientClick="return confirm('Are you sure you want to delete this file?')" OnCommand="DeleteFile" Text="Delete" />
</td>
</tr>
</ItemTemplate>

<HeaderTemplate>
<fieldset>
<legend>Current project files</legend>
<p>
<table style="width: 600px">
<thead>
<tr>
  <th>File</th>
  <th>Type</th>
  <th>Compatible Version</th>
  <th>Uploaded</th>
  <th>Archive</th>
  <th>Delete</th>
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
    
    <div id="swfu_container" style="margin: 0px 10px;">
    
    <div id="swfu_controls">
    
         <p>
            <label class="inputLabel">Pick file:</label>

            <div> 
				<div> 
					<input type="text" id="txtFileName" disabled="true" class="title" /> <span id="spanButtonPlaceholder"></span>
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
              <label class="inputLabel">Choose filetype</label>
            
              <select id="wiki_fileType" class="title">
                <option value="package">Package</option>
                <option value="docs">Documentation</option>
                <option value="source">Source Code</option>
              </select>
        </p>
                
        <p id="pickVersion">
              <label class="inputLabel">Choose umbraco version</label>
            
              <select id="wiki_version" class="title">
                <asp:literal ID="lt_versions" runat="server" />
              </select>              
        </p>
        
        <p>
            <input type="button" value="Upload file" id="btn_submit" />
        </p>

    </div>
    </div>
    </fieldset>
    
    
    
    			       
 </div>
 </asp:PlaceHolder>
	