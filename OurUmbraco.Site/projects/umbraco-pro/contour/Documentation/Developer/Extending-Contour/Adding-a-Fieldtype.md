# Adding a field type to Umbraco Contour #

*This builds on the "[adding a type to the provider model](Adding-a-Type.md)" chapter*

Add a new class to the visual studio solution and make it inherit from Umbraco.Forms.Core.FieldType and override the Editor property.

In the empty constructor add the following information:

	public Textfield() { 
		//Provider 
		this.Id = new Guid("D6A2C406-CF89-11DE-B075-55B055D89593 "); 
		this.Name = "Textfield"; 
		this.Description = "Renders a html input fieldKey"; //FieldType 
		this.Icon = "textfield.png";
	}

In the constructor we specify the standard provider information (remember to set the ID to a unique ID).

And then we set the field type specific information. In this case a preview Icon for the form builder UI and what kind of data it will return, this can either be string, longstring,integer,datetime or boolean.

Then we will start building the editor and the values it returns

	public System.Web.UI.WebControls.TextBox tb; 
	public List<Object> _value; 

	public override WebControl Editor { 
		get { 
			tb.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine; 
			tb.CssClass = "text"; 
			if (_value.Count > 0) 
				tb.Text = _value[0].ToString(); 
			
			return tb; 
		} 
		set { 
			base.Editor = value; 
		} 
	}
The editor simply takes care of generating the UI control and setting its value. The List&lt;object> is what is later returned by the field type.

## Complete example with sourcecode
A complete example of a custom fieldtype can be found on [nibble.be](http://www.nibble.be/?p=89).