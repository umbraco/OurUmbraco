<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditForm.ascx.cs" Inherits="Umbraco.Forms.UI.Usercontrols.EditForm" %>

<asp:PlaceHolder ID="phMain" runat="server">

<link rel="stylesheet" href="<%= MainPath %>css/style.css?version=<%= Umbraco.Forms.Core.Configuration.Version %>" type="text/css" media="screen" />
<link rel="stylesheet" href="<%= MainPath %>css/dd.css" type="text/css" media="screen" />

<!--[if IE 6]>
<style>
.field .handle
{
    left: -30px;
}
</style>
<![endif]-->

<script type="text/javascript">
<!--
    // Copy used in umbracoforms.js put here as global variables
    // so it can be localized easily
    var lang_deleteconfirm = "Are you sure you want to delete this item?";

    var lang_addpage = "Add page";
    var lang_updatepage = "Update page";
    var lang_addfieldset = "Add fieldset";
    var lang_updatefieldset = "Update fieldset";
    var lang_addfield = "Add field";
    var lang_updatefield = "Update field";

    var lang_update = "update";
    var lang_delete = "delete";
    var lang_copy = "copy";
    var lang_nopreview = "Preview not available";

    
    var currentStep = 0;
    <% if (Request["step"] != null) { %>
    currentStep = <%= Request["step"] %>
    <% }  %>
//-->
</script>

<script type="text/javascript" src="<%= MainPath %>../../../umbraco_client/<%= Umbraco.Forms.UI.Config.JqueryUI %>"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/jquery.simplemodal-1.2.3.js"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/jquery.validate.js"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/jquery.dd.js"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/jquery.jeditable.js"></script>



<!-- Nested sortable fix for IE -->
<!--[if IE]>
<script type="text/javascript">

    $.extend($.ui.sortable.prototype, (function(orig) {
        return {
            _mouseDown: function(event) {
                var result = orig.call(this, event);
                if ($.browser.msie) event.stopPropagation();
                return result;

            }
        };
    })($.ui.sortable.prototype["_mouseDown"]));

</script>
<![endif]-->

<script type="text/javascript" src="<%= MainPath %>scripts/umbracoforms.designsurface.js?version=<%= Umbraco.Forms.Core.Configuration.Version %>"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/umbracoforms.js?version=<%= Umbraco.Forms.Core.Configuration.Version %>"></script>

<script type="text/javascript" src="<%= MainPath %>scripts/umbracoforms.eventhandlers.js?version=<%= Umbraco.Forms.Core.Configuration.Version %>"></script>

<script type="text/javascript">
<!--


    var previewfieldid = null;
    function GetFieldPreview(fieldid, fieldtype, prevalues) {

        previewfieldid = fieldid;
        UmbracoContour.Webservices.Designer.RenderFieldPreview(fieldtype, prevalues, PreviewSucces, PreviewFailure);
    }

    function PreviewSucces(preview) {
        $("#" + previewfieldid + " .fieldControl").prepend(preview);
    }

    function PreviewFailure(preview) {
        $("#" + previewfieldid + " .fieldControl").prepend("failed to get preview");
    }

    function ShowChanges() {

        var step = "";
        if (currentpage > 1)
        { step = '&step=' + currentpage; }

        window.location = 'editForm.aspx?guid=<%= Request["guid"] %>&save=true&rnd=' + Math.floor(Math.random() * 1001) + step;
    }

    function FieldTypeChange(value) {

        //also show additional fieldtype settings
        ShowFieldTypeSpecificSettings($("#fieldtype option:selected").val());

        $("#fieldtype option").each(function() {

            if ($(this).attr('value') == value) {
                if ($(this).attr('prevalues') == "1") {
                    if (!($('#fieldprevalues').is(':visible'))) {
                        $("#fieldprevalues").show('blind', {}, 500);

                        $("#fieldprevalueslist").sortable({
                            update: function() {
                            }
                        });
                    }
                }
                else {
                    if ($('#fieldprevalues').is(':visible')) {
                        $("#fieldprevalues").hide('blind', {}, 500);
                    }
                }


                if ($(this).attr('regex') == "1") {
                    
                        $("#fieldregexcontainer").show();
                        $("#fieldinvaliderrormessagecontainer").show();
                    
                }
                else {
                   
                        $("#fieldregexcontainer").hide();
                        $("#fieldinvaliderrormessagecontainer").hide();
                    
                }
            }
        });

    }




    function GetPrevalues(prevaluesource) {

        $('#addUpdateField').attr("disabled", "disabled");
        $('#prevalueloading').show();

        UmbracoContour.Webservices.Designer.GetPrevalues(prevaluesource, PrevalueSuccess, PrevalueFailure);
    }

    function PrevalueSuccess(prevalues) {

        $(prevalues).find('PreValue').each(function() {



            prevalueid = "prevalue_" + $(this).find('Id').text();
            rel = "rel='" + $(this).find('Id').text() + "'";


            var deleteaction = "<a href='javascript:DeletePrevalue(\"" + prevalueid + "\");' class='delete'>" + lang_delete + "</a>";

            $("#fieldprevalueslist").append("<li id='" + prevalueid + "' " + rel + "><span class='prevaluetext'>" + $(this).find('Value').text() + "</span> " + deleteaction + "</li>");

        });

        $('#prevalueloading').hide();
        $('#addUpdateField').removeAttr("disabled");


    }

    function PrevalueFailure(prevalues) {
        //$("#" + previewfieldid + " .fieldexample").append("failed to get preview");
    }



//-->
</script>

<div class="notification" id="notification" runat="server" visible="false">
    <asp:Literal ID="litNotificationCopy" runat="server" Text="This form is linked to a datasource, limited edit possibilities"></asp:Literal>
</div>
<!-- Design Surface -->
<div id="designsurface">
    <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="RenderPage">
        <ItemTemplate>
            <div rel="<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>" id="<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>"
                class="page" style="display: none">
                <div class="pageheader">
                    <strong class="pagename editable"><%# GetEditableCaption(((Umbraco.Forms.Core.Page)Container.DataItem).Caption) %></strong> 
                    <a class="iconButton update"
                            style="display: none" href="javascript:ShowUpdatePageDialog('<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>');">
                            update</a> <a class='iconButton add' href="#" onclick="javascript:ShowAddFieldsetDialog('<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>');">
                                add fieldset</a> <a class="iconButton delete" href="#" onclick="javascript:DeletePage('<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>');">
                                    delete</a> <span class="handle" style="display: none">handle</span>
                </div>
                <div class='pageeditcontainer'>
                </div>
                <div id="fieldsetcontainer<%# ((Umbraco.Forms.Core.Page)Container.DataItem).Id %>"
                    class="fieldsetcontainer">
                    <asp:Repeater ID="rptFieldsets" runat="server" OnItemDataBound="RenderFieldset">
                        <ItemTemplate>
                            <div rel="<%# ((Umbraco.Forms.Core.FieldSet)Container.DataItem).Id %>" id="<%# ((Umbraco.Forms.Core.FieldSet)Container.DataItem).Id %>"
                                class="fieldset">
                                <div class="fieldsetheader">
                                    <strong class="fieldsetname editable"><%# GetEditableCaption(((Umbraco.Forms.Core.FieldSet)Container.DataItem).Caption) %></strong> 
                                    <a class="iconButton update"
                                            style="display: none" href="#" onclick="javascript:ShowUpdateFieldsetDialog('<%# ((Umbraco.Forms.Core.FieldSet)Container.DataItem).Id %>');">
                                            update</a> <a class="iconButton delete" href="#" onclick="javascript:DeleteFieldset('<%# ((Umbraco.Forms.Core.FieldSet)Container.DataItem).Id %>');">
                                                delete</a> <span class="handle">Move</span>
                                </div>
                                <div class='fieldseteditcontainer'>
                                </div>
                                <div class="fieldcontainer">
                                    <asp:Repeater ID="rptFields" runat="server" OnItemDataBound="RenderField">
                                        <ItemTemplate>
                                            <div rel="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>" id="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>"
                                                class="field" fieldcaption="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Caption %>"
                                                fieldtype="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).FieldType.Id %>"
                                                fieldmandatory="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Mandatory %>"
                                                fieldregex="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).RegEx %>" fieldsupportsprevalues="<%# Convert.ToInt32(((Umbraco.Forms.Core.Field)Container.DataItem).FieldType.SupportsPrevalues) %>"
                                                fieldsupportsregex="<%# Convert.ToInt32(((Umbraco.Forms.Core.Field)Container.DataItem).FieldType.SupportsRegex) %>"
                                                fieldtooltip="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).ToolTip %>"
                                                fielddatasourcefield="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).DataSourceFieldKey %>"
                                                fieldrequirederrormessage="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).RequiredErrorMessage %>"
                                                fieldinvaliderrormessage="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).InvalidErrorMessage %>"
                                                fieldenablecondition="<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Condition.Enabled %>"
                                                fieldconditionactiontype="<%#  (int)((Umbraco.Forms.Core.Field)Container.DataItem).Condition.ActionType %>"
                                                fieldconditionlogictype="<%# (int)((Umbraco.Forms.Core.Field)Container.DataItem).Condition.LogicType %>">
                                                <a class="iconButton update" href="#" onclick="javascript:ShowUpdateFieldDialog('<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>');">
                                                    update</a> <a class="iconButton copy" href="#" onclick="javascript:CopyField('<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>');">
                                                       copy</a> <a class="iconButton delete" href="#" onclick="javascript:DeleteField('<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>');">
                                                        delete</a> <span class="handle">handle</span>
                                                <div class="fieldprevalues" style="display: none;" <%# GetPrevalueAttributes(((Umbraco.Forms.Core.Field)Container.DataItem)) %>>
                                                    <asp:Repeater ID="rptPrevalues" runat="server">
                                                        <ItemTemplate>
                                                            <div class="prevalue" rel="<%# ((Umbraco.Forms.Core.PreValue)Container.DataItem).Id.ToString()%>"><%# ((Umbraco.Forms.Core.PreValue)Container.DataItem).Value%></div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                                <div class="fieldadditionalsettings" style="display: none;">
                                                    <asp:Repeater ID="rptAdditionalSettings" runat="server">
                                                        <ItemTemplate>
                                                            <div class="additionalsetting" rel="<%#Eval("key") %>"><%#Eval("value") %></div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                                <div class="fieldconditionrules" style="display: none;">
                                                    <asp:Repeater ID="rptConditionRules" runat="server">
                                                        <ItemTemplate>
                                                            <div class="conditionrule" field="<%# ((Umbraco.Forms.Core.FieldConditionRule)Container.DataItem).Field %>" operator="<%# (int)((Umbraco.Forms.Core.FieldConditionRule)Container.DataItem).Operator %>"><%# ((Umbraco.Forms.Core.FieldConditionRule)Container.DataItem).Value %></div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                                <div class="fieldexample">
                                                    <label class="fieldexamplelabel">
                                                        <span>
                                                            <%# ((Umbraco.Forms.Core.Field)Container.DataItem).Caption %></span>
                                                        <asp:Label ID="lblFieldMandatory" runat="server" Text="" CssClass="mandatory"></asp:Label>
                                                    </label>
                                                    <div class="fieldControl">
                                                        <%# RenderFieldPreview(((Umbraco.Forms.Core.Field)Container.DataItem)) %>
                                                        <div class="fieldeditprevalues" <%# PreValueEdit(((Umbraco.Forms.Core.Field)Container.DataItem)) %>>
                                                            <a href="#" onclick="javascript:ShowUpdatePrevaluesDialog('<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>')">
                                                                Edit items</a>
                                                        </div>
                                                    </div>
                                                    <br style="clear: both;" />
                                                </div>
                                                <div style="clear: both;"></div>
                                            </div>
                                            <div class='fieldeditcontainer' id='fieldeditcontainer<%# ((Umbraco.Forms.Core.Field)Container.DataItem).Id %>'>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                                <button class="addfield" onclick="javascript:ShowAddFieldDialog('<%# ((Umbraco.Forms.Core.FieldSet)Container.DataItem).Id %>'); return false;">
                                    <img src="css/img/add_small.png" style="vertical-align: middle" />
                                    <span>Add Field</span></button>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<div id="stepsnavigation">
    <a href="#" id="stepnavprev">Previous</a> <a href="#" id="stepnavnext">Next</a>
    <a href="#" id="stepnavnew">Create new form step</a>
</div>


<div id="ftSpecificSettingsContainer" style="display:none;">
    <asp:Repeater ID="rptFieldTypeSettings" runat="server" OnItemDataBound="RenderFieldTypeSettings">
        <ItemTemplate>
            <div class="ftSpecificSettings" id="ftSettingsContainer<%# ((Umbraco.Forms.Core.FieldType)Container.DataItem).Id %>">
                <asp:Repeater ID="rptFieldTypeSpecificSettings" runat="server" OnItemDataBound="RenderFieldTypeSpecificSettings">
                    <ItemTemplate>
                        <div class="ftAdditionalSetting fieldContainer" rel="<%# ((System.Collections.Generic.KeyValuePair<string, Umbraco.Forms.Core.Attributes.Setting>)Container.DataItem).Key %>">
                            
                            <asp:Label ID="label" runat="server" CssClass="fieldexamplelabel"></asp:Label>
                           
                            <div class="formControl leftPad10">
                                 <asp:PlaceHolder ID="placeholder" runat="server" />
                            </div>

                            <br style="clear: both;">
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>

</asp:PlaceHolder>


<asp:PlaceHolder ID="phModals" runat="server">
<!-- Modals -->
<div id="modals" style="display: none;" >
    <!-- dummy form, placed here because first form gets removed -->
    <form id="dummyform">
    </form>
    <div id="PageModal" style="display: none;" class="modal">
        <h1>
            Add Page</h1>
        <form id="pageform" class="modalform" action="">
        <fieldset>
            <p>
                <label for="NewPageName">
                    Name</label>
                <input type="text" id="NewPageName" class="required" />
            </p>
            <p>
                <input class="submit" type="submit" value="Add Page" />
                or <a href="#" class="CancelModal" id="cancelpageaction">Cancel</a>
            </p>
        </fieldset>
        </form>
    </div>
    <div id="FieldsetModal" style="display: none;" class="modal">
        <h1>
            Add Fieldset</h1>
        <form id="fieldsetform" class="modalform" action="">
        <fieldset id="fieldsetpageselectcontainer">
            <p>
                <label for="pageid">
                    Page</label>
                <select id="pageselect">
                </select>
            </p>
        </fieldset>
        <fieldset>
            <p>
                <label for="NewFieldsetName">
                    Name</label>
                <input type="text" id="NewFieldsetName" class="required" />
            </p>
            <p>
                <input class="submit" type="submit" value="Add Fieldset" />
                or <a href="#" class="CancelModal" id="cancelfieldsetaction">Cancel</a>
            </p>
        </fieldset>
        </form>
    </div>
    <div id="FieldModal" style="display: none;" class="modal">
        <h1>
            Add Field</h1>
        <form id="fieldform" class="modalform" action="">
        <fieldset id="fieldfieldsetselectcontainer">
            <p>
                <label for="fieldpageselect">
                    Page</label>
                <div class="formControl">
                    <select id="fieldpageselect">
                    </select></div>
            </p>
            <p>
                <label for="fieldfieldsetselect">
                    Fieldset</label>
                <div class="formControl">
                    <select id="fieldfieldsetselect">
                    </select><span id="fieldsetid"></span>
                </div>
            </p>
        </fieldset>
        <fieldset>
            <div class="fieldContainer">
                <label for="fieldcaption" class="fieldexamplelabel">
                    Caption</label>
                <div class="formControl leftPad10">
                    <input type="text" id="fieldcaption" class="required textfield" />
                </div>
                <br style="clear: both" />
            </div>
            <div class="fieldContainer">
                <label for="fieldtype" class="fieldexamplelabel">
                    Type</label>
                <div class="formControl leftPad10">
                    <div id="fieldtypecontainer">
                        <select id="fieldtype" style="width: 244px;" onchange="FieldTypeChange(this.value)">
                            <asp:Repeater ID="rptFieldTypes" runat="server">
                                <ItemTemplate>
                                    <option value="<%# ((Umbraco.Forms.Core.FieldType)Container.DataItem).Id %>" prevalues="<%# Convert.ToInt32(((Umbraco.Forms.Core.FieldType)Container.DataItem).SupportsPrevalues)%>"
                                        regex="<%# Convert.ToInt32(((Umbraco.Forms.Core.FieldType)Container.DataItem).SupportsRegex)%>"
                                        title="images/fieldtypes/<%# ((Umbraco.Forms.Core.FieldType)Container.DataItem).Icon  %>">
                                        <%# ((Umbraco.Forms.Core.FieldType)Container.DataItem).Name %>
                                    </option>
                                </ItemTemplate>
                            </asp:Repeater>
                        </select>
                    </div>
                    <br style="clear: both" />
                </div>
                <br style="clear: both" />
            </div>
            <div class="fieldContainer">
                <label for="fieldmandatory" class="fieldexamplelabel">
                    Mandatory</label>
                <div class="formControl leftPad10">
                    <input type="checkbox" id="fieldmandatory" />
                </div>
                <br style="clear: both" />
            </div>
            <p style="clear: both" id="toggleadditionalsettings">
                Additional Settings</p>
            <div id="fieldadditionalsettings">
            

                <div id="fieldregexcontainer" class="fieldContainer">
                    <label for="fieldregex" class="fieldexamplelabel">
                        Regex</label>
                    <div class="formControl leftPad10">
                        <input type="text" id="fieldregex" class="textfield" />
                    </div>
                    <br style="clear: both" />
                </div>
                <div class="fieldContainer" id="fieldinvaliderrormessagecontainer">
                    <label for="fieldinvaliderrormessage" class="fieldexamplelabel">
                        Invalid Error Message</label>
                    <div class="formControl leftPad10">
                        <input type="text" id="fieldinvaliderrormessage" class="textfield" />
                    </div>
                    <br style="clear: both" />
                </div>
                
                <div class="fieldContainer">
                    <label for="fieldtooltip" class="fieldexamplelabel">
                        Tooltip</label>
                    <div class="formControl leftPad10">
                        <input type="text" id="fieldtooltip" class="textfield" />
                    </div>
                    <br style="clear: both" />
                </div>
                <div class="fieldContainer" id="fieldrequirederrormessagecontainer">
                    <label for="fieldrequirederrormessage" class="fieldexamplelabel">
                        Required Error Message</label>
                    <div class="formControl leftPad10">
                        <input type="text" id="fieldrequirederrormessage" class="textfield" />
                    </div>
                    <br style="clear: both" />
                </div>
                <div id="fieldprevalues" style="display: none; clear: both;">
                    <div id="prevaluetypeselection" class="fieldContainer">
                        <label for="prevaluestype" class="fieldexamplelabel">
                            Prevalue Type</label>
                        <div id="prevaluetypeselectionselect" class="formControl leftPad10">
                            <select id="prevaluestype">
                                <option crud="1" value="">standard</option>
                                <asp:Repeater ID="rptPrevalueTypes" runat="server">
                                    <ItemTemplate>
                                        <option crud="<%# Convert.ToInt32(SupportsCrud(((Umbraco.Forms.Core.FieldPreValueSource)Container.DataItem))) %>"
                                            value="<%# ((Umbraco.Forms.Core.FieldPreValueSource)Container.DataItem).Id %>">
                                            <%# ((Umbraco.Forms.Core.FieldPreValueSource)Container.DataItem).Name %>
                                        </option>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </select>
                            <span id="prevalueloading" style="display:none;">loading...</span>
                        </div>
                    </div>
                    <ul id="fieldprevalueslist" style="display: none">
                        <li></li>
                    </ul>
                    <p id="prevalueadd" style="display: none">
                        <input type="text" id="fieldnewprevalue" />
                        <a href="#" id="addprevalue">add value</a>
                    </p>
                </div>
                 <div class="fieldContainer">
                     <label for="fieldenableconditions" class="fieldexamplelabel">
                            Enable Conditions </label>
                     <div class="formControl leftPad10">   
                        <input type="checkbox" id="fieldenableconditions"/>
                     
                     <div id="fieldconditions">
               
                        <select id="fieldconditionactiontype">
                            <asp:Repeater ID="rptFieldConditionActionTypes" runat="server">
                                <ItemTemplate>
                                    <option value="<%# ((int)Container.DataItem) %>">
                                    <%# ((Umbraco.Forms.Core.FieldConditionActionType)Container.DataItem) %>
                                    </option>
                                </ItemTemplate>
                            </asp:Repeater>
                         </select>
                         this field if
                         <select id="fieldconditionlogictype">
                            <asp:Repeater ID="rptFieldConditionLogicTypes" runat="server">
                                <ItemTemplate>
                                    <option value="<%# ((int)Container.DataItem) %>">
                                    <%# ((Umbraco.Forms.Core.FieldConditionLogicType)Container.DataItem)%>
                                    </option>
                                </ItemTemplate>
                            </asp:Repeater>
                         </select> 
                         of the following match: 
                         <div id="ruleadd">
                            <select id="fieldruleadd" class="fieldruleselect">
                                <option value="3">Email</option>
                                <option value="4">Who Would You Like to Contact?</option>
                            </select>
                            <select id="fieldruleaddoperator" class="fieldruleoperator">
                            <asp:Repeater ID="rptFieldConditionRuleOperators" runat="server">
                                <ItemTemplate>
                                    <option value="<%# ((int)Container.DataItem) %>">
                                    <%# ((Umbraco.Forms.Core.FieldConditionRuleOperator)Container.DataItem)%>
                                    </option>
                                </ItemTemplate>
                            </asp:Repeater>
                            </select>
                            <input type="text" placeholder="Enter a value" class="fieldrulevalue" id="fieldruleaddvalue" />
                            <img src="css/img/add_grey.png" class="addfieldrule" title="add another rule" alt="add another rule" style="cursor:pointer; margin:0 3px;">
                          </div>
                          <div id="fieldrules">
                          </div>
                    </div>                    
                    </div>
                 </div>
            </div>
           

            <p style="clear: both;">
                <input class="submit" type="submit" value="Add Field" id="addUpdateField" />
                or <a href="#" class="CancelModal" id="cancelfieldaction">Cancel</a>
            </p>
        </fieldset>
        </form>
    </div>
    <div id="PreValueModal" style="display: none;" class="modal">
        <form id="prevalueform" class="modalform" action="">
        <fieldset>
            <h1>
                Edit Items</h1>
            <ul id="editprevaluelist">
                <li></li>
            </ul>
            <p style="clear:both;">
                <input type="text" id="editnewprevalue" />
                <a href="#" id="editaddprevalue">add value</a>
            </p>
            <p>
                <input class="submit" type="submit" value="Update items" />
                <a href="#" class="CancelModal" id="cancelprevalueaction">cancel</a>
            </p>
        </fieldset>
        </form>
    </div>
</div>




</asp:PlaceHolder>

