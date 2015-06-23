#Creating a form
This will show the basic steps of creating forms in Contour and adding them to your Umbraco site. 
##Navigate to the Contour section
Managing forms happens in the Contour section of the Umbraco backoffice so first navigate to this section by selecting it (you'll need to have Contour installed and have access to this section in order to see it).

![Contour Section](ContourSection.png)

##Right click the forms tree
Next similar to most create actions in the Umbraco backoffice you'll have to right click the forms tree/folder and select create

![Contour tree](ContourFormsTree.png)

##Name the form
After selecting create you should see the following dialog

![Contour create dialog](ContourCreateDialog.png)

Where you'll need to supply a name for the form and have the option to start from a template (the template will already have some fields definied so you don't start from a completely empty form).

Hitting create will obviously create your new form.

##Design the form

The next screen you should see after hitting the create button is the form designer

![Contour create dialog](ContourFormDesignerStart.png)

There is already a page and a fieldset available the rest of the form has to be added using the UI(unless you started from a template then you alreayd get some fields).

###Setting the page title

The page title can be placed in edit mode by simply clicking it.

![Contour page title edit](ContourFormDesignerPageTitleEdit.png)

###Setting the fieldset legend

The same applies for the fieldset legend, clicking it will place it in edit mode.

![Contour fieldset legend edit](ContourFormDesignerFieldsetLegendEdit.png)

###Adding fields

To add a new field hit the add field button in the fieldset where you wish to add the extra field.

![Contour add field](ContourFormDesignerAddFieldpng.png)

That should display the following form

![Contour add field form](ContourFormDesignerAddField.png)

Where you'll need to supply a caption for the new field (will be used on the field) and choose the type. By default the type is set to textfield but other options are available.

- Checkbox
- Checkboxlist
- Datepicker
- Dropdownlist
- File upload
- Hidden field
- Password field
- Radiobuttonlist
- Recaptcha (from v3.0.7)
- Textarea
- Textfield

Caption and type are the only 2 mandatory fields you need to provide for adding a new field to your form. Once those are supplied the field can be added.

![Contour add field form](ContourFormDesignerAddFieldAdd.png)

It's also possible to provide additional settings (these will be dependant on the chosen fieldtype, to show the extra settings hit the additional settings link.

![Contour additional settings](ContourFormDesignerAddFieldAdditionalSettings.png)

Once the field has been added the create field form should disapear and you get a preview of your new field in the form designer.

![Contour new field added](ContourFormDesignerFieldAdded.png)

You can then repeat this step untill you end up with the form you desire.

##Saving the form
Once you are satisfied with the form you can save the design by hitting the save button in the toolbar

![Contour save form](ContourFormDesignerSave.png)

##Previewing and testing the form
If you wish the test the form you just created just hit the preview button in the toolbar and you will be taken to a fully functional preview of your form (changes must be saved before they can be previewed).

![Contour preview form](ContourFormDesignerPreview.png)

##Adding the form to the Umbraco site

###Select page

Navigate to the content section and select the content page where you want to insert the form (this page should have an RTE field)

![Contour content page](ContentPage.png)

###Add Contour macro

Hit the add macro button in the toolbar

![Contour content page add macro](ContentPageMacroButton.png)

Select the insert form from Umbraco contour macro

![Contour content page add macro](ContentPageAddMacroDialog.png)

Select the form you want to insert and hit Ok

![Contour content page add form](ContentPageAddMacroDialogChooseForm.png)

The form should be inserted now and all that's needed to put it on your site is a save and publish of the content page!
































