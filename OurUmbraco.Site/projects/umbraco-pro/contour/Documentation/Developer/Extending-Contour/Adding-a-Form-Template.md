#Adding a form template
Contour comes with a complete XML schema to represent a form. This XML can be exported and imported as .ucf files which contains a full XML representation of the form.
However, these .ucf files can also be placed in the form templates directory to make them reusable by end-users.
##Adding an existing form as a template
Export the form you wish to use as a template, this will download a .ucf file to your local machine.

Copy or ftp the file to the /umbraco/plugins/umbracoContour/templates/forms directory, there is nothing that needs to be renamed or modified.

Right click the forms folder in the contour tree and select create, your form should now be available as a template.