#Custom markup
Since Contour 3 it's possible to customize the outputted markup of a Contour form. This option is only available for the razor version of the Contour macro.

##Customizing the default views
The way the razor macro works it that it uses some razor views to output the form (1 for each fieldtype, 1 for the scripts and 1 for the rest of the form). The views are available for edit so you can customize them to your needs.

The views can be found in the ~\Umbraco\plugins\umbracoContour\Views directory

###Form.cshtml

This is the main view responsable for rendering the form markup.

Default contents of the view:

	@inherits WebViewPage<Umbraco.Forms.Mvc.Models.FormViewModel>      
	@using Umbraco.Forms.Core
	@using Umbraco.Forms.Mvc.Models
	@using Umbraco.Forms.Mvc.BusinessLogic
	@using Umbraco.Forms.MVC.Extensions
	
	@if (Model.SubmitHandled)
	{
	    <p class="contourMessageOnSubmit">@Model.MessageOnSubmit</p>
	}
	else
	{
    	Html.EnableClientValidation();
	    Html.EnableUnobtrusiveJavaScript();
	
	    @Html.Partial(FormViewResolver.GetScriptView(Model.FormId), Model)
	    
	    if (!Model.DisableDefaultStylesheet)
	    {
	        <link rel="stylesheet" type="text/css" href="@Url.Content("~/umbraco/plugins/umbracocontour/css/jquery-ui-1.8.18.custom.css")" />
	        <link rel="stylesheet" type="text/css" href="@Url.Content("~/umbraco/plugins/umbracocontour/css/defaultform.css")" />
	    }                                                                   
	
	    <div id="contour" class="@Model.CssClass">
	        @using (Html.BeginForm("ContourForm", "FormRender", FormMethod.Post, new { enctype = "multipart/form-data" }))
	        {
	            @Html.AntiForgeryToken()
	            @Html.HiddenFor(x => Model.FormId)
	
	            <input type="hidden" name="FormStep" id="FormStep" value="@Model.FormStep"/>
	            <input type="hidden" name="RecordId" id="RecordId" value="@Model.RecordId"/>
	            <input type="hidden" name="PreviousClicked" id="PreviousClicked" value="@Model.PreviousClicked"/>
	          
	         
	            <div class="contourPage">
	                <h4 class="contourPageName">@Model.CurrentPage.Caption</h4>
	                @if (Model.ShowValidationSummary)
	                {
	                    @Html.ValidationSummary(false)
	                }
	                @foreach (FieldsetViewModel fs in Model.CurrentPage.Fieldsets)
	                {
	                    <fieldset class="contourFieldSet">
	                        @if (!string.IsNullOrEmpty(fs.Caption))
	                        {   
	                            <legend>@fs.Caption</legend>
	                        }
	                        @foreach (FieldViewModel f in fs.Fields)
	                        {
	                            bool hidden = f.HasCondition && f.ConditionActionType == FieldConditionActionType.Show;
	                            <div class="@f.CssClass" @{if (hidden){<text> style="display: none"</text>}}>
	                                @if(!f.HideLabel){<label for="@f.Id" class="fieldLabel">@f.Caption @if (f.ShowIndicator){<span class="contourIndicator">@Model.Indicator</span>}</label>}
	                                @Html.Partial(FieldViewResolver.GetFieldView(Model.FormId, f.FieldTypeName, f.Caption), f)
	                                @if (Model.ShowFieldValidaton){@Html.ValidationMessage(f.Id)}
	                            </div>
	                        }
	                    </fieldset>
	                }
	                <div style="display:none">
	                    <input type="text" id="HoneyPot"/>
	                </div>
	            </div>
	
	            <div class="contourNavigation">
	                @if (Model.IsMultiPage)
	                {
	                    if (!Model.IsFirstPage)
	                    {
	                        <input class="cancel" type="submit" value="@Model.PreviousCaption"/>
	                    }
	                    if (!Model.IsLastPage)
	                    {
	                         <input type="submit" value="@Model.NextCaption"/>
	                    }
	                }
	                @if (Model.IsLastPage)
	                {
	                    <input type="submit" value="@Model.SubmitCaption"/>
	                }
	            </div>                        
	            
	        }
	    </div>
	}

The view is seperated in 2 parts, 1 is the actual form and the other part is what will be shown if the form is submitted.

This view can be customized, if you do so it will be customized for all your forms.

###Script.cshtml
This view renders the javascript that will take care of the conditional logic, custimization won't be needed here...

###FieldType.*.cshtml
The rest of the views start with FieldType. like FieldType.Textfield.cshtml and those will output the fields (so there is a view for each default fieldtype like textfield, textarea, checkbox...

Contents of the  FieldType.Textfield.cshtml view:

	@model Umbraco.Forms.Mvc.Models.FieldViewModel
	<input type="text" name="@Model.Name" id="@Model.Id" class="text" value="@Model.Value" maxlength="500"
	@{if(Model.Mandatory || Model.Validate){<text>data-val="true"</text>}}
	@{if (Model.Mandatory) {<text> data-val-required="@Model.RequiredErrorMessage"</text>}}
	@{if (Model.Validate) {<text> data-val-regex="@Model.InvalidErrorMessage" data-regex="@Model.Regex"</text>}}
	/>

By default the form makes uses of jquery validate and jquery validate unobtrosive that's why you see attribute like data-val and data-val-required again this can be customized but it's important to keep the id of the control to @Model.Id since that is used to match the value to the form field.

##Customizing for a specific form
If you wish to customize the markup for a specific form and not all your forms that is also possible.

For a specific form you'll need to create the following folder:
~\umbraco\plugins\umbracoContour\Views\Forms\{FormId}\ (FormId needs to be an existing form id, you can view the id of the form on the settings tab of the form designer)

As an example if your form id is 85514c04-e188-43d0-9246-98b34069750c then you can overwrite the form view by adding the Form.cshtml file to the directory
First copying the default one and then making your changes is the best way to get started
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c/Form.cshtml

You can also overwrite views for 1 or more fieldtypes by adding the views to the folder (again if you first copy the default one and then make your changes...)
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c\Fieldtype.Textfield.cshtml

##Customize for a specific field an a form
A final option is to overwrite the view for a specific field on a form (if you want to target a specific field but not all fields of this type)
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c\FieldNameWithoutSpaces.cshtml