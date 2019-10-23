#Extending Contour #

## Contour provider model ##
Most parts of Umbraco Contour uses a provider model, which makes it easy to add new parts to the application.
The model uses the notion that everything must have type to exist. The type defines the capabilities of the item. For instance a Textfield on a form has a FieldType, this particular field type enables it to render an input field and save simple text strings. The same goes for workflows, which has a workflow type, datasources which have datasource type and so on. Using the model you can seamlessly add new types and thereby extend the application.
In the current version it is possible to add new Field types, Data Source Types, Prevalue Source Types, Export Types, and Workflow Types.
### [Field types](Adding-a-Fieldtype.md) ###
A field type handles rendering of the UI for a field in a form. It renders a standard asp.net webcontrol and is able to return a list of values when the form is saved.

### Data Source Types ###
A data source type enables Contour to connect to a custom source of data. A datasource can consist of any kind of storage as long as it possible to return a list of fields Contour can map values to. For exemple: a Database data source can return a list of columns Contour can send data to, which enables Contour to map a form to a data source. A data source type is responsible for connecting
### Prevalue Source Types ###
A prevalue source type can connect to a 3rd party storage and retrieve a collection of values which can be used on fields which support prevalues. The prevalue source is responsible for connecting to the source and retrieving the collection of values. A prevalue sourc e type can also implement edit capabilities so new items can be added/updated/deleted directly from the form editor.
### [Workflow Types](Adding-a-Workflowtype.md) ###
A workflow can be executed each time a form changes state (when it is submitted for instance). A workflow is responsible for executing simple logic which can modify the record or notify 3rd party systems.
### Export Types ###
Export types are responsible for turning form records (which are xml) into any other data format, which is then returned as a file.
### Record Action Types ###
A Record action is an action that can be executed against a single record (like deleting a record, editing a record). These actions are available on the record viewer of each form.
### Recordset Action Types ###
A Recordset Action is an action that can be executed against a variable number of records. These actions are available in the record viewer of each form after selecting a number of records.
###Complete examples available
The [sourcecode](https://our.umbraco.com/projects/developer-tools/umbraco-contour-shared-source) for the default providers is available for download and shows you how our default components are build.

There are also several community addons that can be used for inspiration when extending Contour

* [Contour contrib](https://our.umbraco.com/projects/developer-tools/contour-contrib)
* [Contour strikes again](http://contourstrikesagain.codeplex.com/)

##[Adding a form template](Adding-a-Form-Template.md) ##
Learn how to extend the create dialog with new form templates

