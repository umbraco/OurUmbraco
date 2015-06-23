#Form settings
Besides the form design you can also setup some settings to customize how your form will behave.

The full settings tab looks like this

![Form settings](FormSettings.png)

##General
![Form settings general](FormSettingsGeneral.png)
###Name
The name of your form, this name will appear in the forms tree of the Contour section and also in the form picker datatype and form macro.
###Guid
The id of the form, this is a readonly value that might be usefull to the developers
###Manual Approval
The option to set a form to manual approval, by default a form record will be placed in the approved state after it has been submitted. If you wish to leave it in the submitted state and manually place it in the approves state then check this option (might be usefull if there are workflows that happen on approval).

##Styling
![Form settings stylesheet](FormSettingsStylesheet.png)
###Disable default stylesheet
More site design orientated (so probably for the developer) but enabling this option will prevent a default stylesheet being added to the pages where the Contour form is placed.

##Field indicators
Should mandatory fields or optional fields be marked with a certain indicator?

![Form settings field indicators](FormSettingsFieldIndicators.png)
###Mark fields
You can choose to not mark any fields or only mark mandatory or optional fields.
###Indicator
The indicator that will be used, a typical indicator is just a *

##Validation
What should be displayed when a field is mandatory and a value isn't supplied or the value isn't valid.
![Form settings validation](FormSettingsValidation.png)
###Required error message
The error message that will be displayed for a field if a field is mandatory but a value isn't provided. This setting can be overwritten on a field level. {0} will be replaced with the field caption.
###Invalid error message
The error message that will be displayed for a field if a field isn't valid (a regular expression has been setup but the input doesn't match). This setting can be overwritten on a field level. {0} will be replaced with the field caption.
###Show validation summary
Enable this option If you wish to display a summary of all error messages on top of the form.
###Hide field validation labels
Enable this option if you wish the hide idividual field error messages from being displayed.

##Submitting the form
What happens when the form has been submitted, there are 2 options that can be setup.
![Form settings submitting the form](FormSettingsSubmitting.png)
###Message on submit
Display some text (staying on the same page)
###Send to page
Selecting a page where the user will be redirected after submitting the form

##Setting current settings as default

If you wish to store the current settings as default you can do so by hitting the associated button in the toolbar. Doing so will result in new forms having the same settings.

![Form settings as default](FormSettingsSetAsDefault.png)