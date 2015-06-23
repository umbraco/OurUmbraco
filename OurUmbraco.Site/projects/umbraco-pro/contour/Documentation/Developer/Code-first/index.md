#Designing forms in Visual studio (code first)
Since Contour 3.0 it has been possible to use the code first feature to design your forms in visual studio (no need to touch the UI). Making it possible to have source controlled Contour forms and automated deployments of those forms.
##Add references
To get started you'll need to reference

* Umbraco.Forms.CodeFirst
* Umbraco.Forms.Core
* Umbraco.Forms.Core.Providers

Once those are in place you can start designing forms.
##Create new form class
Add new class to your visual studio project and inherit from FormBase, it's also necesary to mark the class with the Form attribute (providing it with a name for your form.

	[Form("Registration")]
	public class Registration: FormBase
	{
	}
##Adding fields
Adding fields to your form can be done by adding properties and marking those with the Field attribute

	[Field("Page","FieldSet")]
	public string Name { get; set; }
When adding the field you'll need to provide the caption of both the page and the fieldset the field should appear on this can be provided as strings or its also possible to setup enums like in the following example

	public enum FormPages
	{
	    Registration
	}
	 
	public enum FormFieldsets
	{
	    Details
	}  

	[Field(FormPages.Registration,FormFieldsets.Details))]
	public string Name { get; set; }

Additional settings (like mandatory, regular expressions, ...) are also possible to setup these are just properties on the Field attribute

    [Field(FormPages.Registration,FormFieldsets.Details,
        Mandatory= true)]
    public string Name { get; set; }
 
    [Field(FormPages.Registration, FormFieldsets.Details,
        Mandatory = true,
        Regex = @"(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})")]
    public string Email { get; set; }

##Custom validation
An advantage of this code first approach is that it's also super easy to add in additional validation rules. That can be done by overriding the Validate method. For example if you have the following fields

    [Field(FormPages.Registration, FormFieldsets.Details,
        Mandatory = true,
        Regex = @"(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})")]
    public string Email { get; set; }
 
    [Field(FormPages.Registration, FormFieldsets.Details, 
        Type = typeof(Password),
        Mandatory = true)]
    public string Password { get; set; }
 
    [Field(FormPages.Registration, FormFieldsets.Details, 
        Type = typeof(Password),
        Mandatory = true)]
    public string RepeatPassword { get; set; }

and the form will be used to create a new member in umbraco so we'll have to make sure the email is unique and the password and repeat password field match.

	public override IEnumerable<Exception> Validate()
	{
	    var e = new List<Exception>();
	    //checks if email isn't in use
	    if(Member.GetMemberFromLoginName(Email) != null)
	        e.Add(new Exception("Email already in use"));
	    //makes sure the passwords are identical
	    if (Password != RepeatPassword)
	        e.Add(new Exception("Passwords must match"));
	 
	    return e;
	}

##Attaching extra functionality
It's possible to hook up worflows but with this code first approach there is also a Submit method that can be overridden and used

	public const string MemberTypeAlias = "Member";
	public const string MemberGroupName = "Authenticated";
	 
	public override void Submit()
	 {
	     //get a membertype by its alias
	     var mt = MemberType.GetByAlias(MemberTypeAlias); //needs to be an existing membertype
	     //get the user(0)
	     var user = new User(0);
	     //create a new member with Member.MakeNew
	     var member = Member.MakeNew(Name, mt, user);
	     //assign email, password and loginname
	     member.Email = Email;
	     member.Password = Password;
	     member.LoginName = Email;
	     //asign custom properties
	     if(!string.IsNullOrEmpty(Avatar))
	         member.getProperty("avatar").Value = Avatar;
	     //asssign a group, get the group by name, and assign its Id
	     var group = MemberGroup.GetByName(MemberGroupName); //needs to be an existing MemberGroup
	     member.AddGroup(group.Id);
	     //generate the member xml with .XmlGenerate
	     member.XmlGenerate(new System.Xml.XmlDocument());
	     //add the member to the website cache to log the member in
	     Member.AddMemberToCache(member);
	     //redirect to another page
	     HttpContext.Current.Response.Redirect("/");
	 }
##Deploying the form
After compiling the project and dropping the assembly in the bin directory of your umbraco instance the code should sync with Contour and a new form will appear

##Complete examples
Some complete examples can be found on [nibble.be](http://www.nibble.be/?p=205)
