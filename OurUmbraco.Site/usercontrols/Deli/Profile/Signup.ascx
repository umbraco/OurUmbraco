<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Signup.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Profile.Signup" %>
<%@ Register Assembly="Marketplace" TagPrefix="Deli" Namespace="Marketplace.Controls" %>

<asp:PlaceHolder runat="server" ID="ProfileNavigation">
<ul class="stepNavigation">
    <li runat="server"><asp:linkbutton ID="profileNav" runat="server" OnClick="ChangeProfile" CommandArgument="Basic">Basic Profile</asp:linkbutton></li>
    <li runat="server"><asp:linkbutton ID="vendorNav" runat="server" OnClick="ChangeProfile" CommandArgument="Vendor">Vendor Profile</asp:linkbutton></li>
</ul>
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="SignupBasicProfile">
    <asp:panel runat="server" defaultbutton="bt_submit">
    <div class="form simpleForm" id="registrationForm">
    <fieldset>
    <legend>Basic Information</legend>
      <p>
      We just need the most basic information from you.
      </p>
      <p>
        <asp:label ID="Label1" AssociatedControlID="tb_name" CssClass="inputLabel" runat="server">Name</asp:label>
        <asp:TextBox ID="tb_name" runat="server" ToolTip="Please enter your name" CssClass="required title"/>
      </p>
      <p>
        <asp:label ID="Label2" AssociatedControlID="tb_company" CssClass="inputLabel" runat="server">Company</asp:label>
        <asp:TextBox ID="tb_company" runat="server" ToolTip="Please enter your company name" CssClass="title"/>
      </p>
      <p>
        <asp:label ID="Label3" AssociatedControlID="tb_email" CssClass="inputLabel"  runat="server">Email</asp:label>
        <asp:TextBox ID="tb_email" runat="server" onBlur="lookupEmail(this);" ToolTip="Please enter your email address" CssClass="required email title"/>
      </p>
      <p>
        <asp:label ID="Label4" AssociatedControlID="tb_password" CssClass="inputLabel" runat="server">Password</asp:label>
        <asp:TextBox ID="tb_password" runat="server" ToolTip="Please enter a password, minimum 5 characters" TextMode="Password" CssClass="password title"/>
      </p>
      <p>
        <asp:label ID="Label10" AssociatedControlID="tb_bio" CssClass="inputLabel" runat="server">Bio<br /><small>No html allowed</small></asp:label>
        <asp:TextBox ID="tb_bio" runat="server" TextMode="MultiLine" CssClass="title noHtml"/>
      </p>  
    </fieldset>

    <fieldset>
    <legend>Billing Information</legend>
      <p>
     If you are planning on buying products from the Deli, to speed up the checkout process fill in as much of the following as possible
      </p>
      <p>
        <asp:label AssociatedControlID="tb_companyVatNumber" CssClass="inputLabel" runat="server">VAT Number</asp:label>
        <asp:TextBox ID="tb_companyVatNumber" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_companyAddress" CssClass="inputLabel" runat="server">Address</asp:label>
        <asp:TextBox ID="tb_companyAddress" TextMode="MultiLine" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="dd_companyCountry" CssClass="inputLabel" runat="server">Country</asp:label>
        <Deli:CountryDropDownList ID="dd_companyCountry" runat="server" CssClass="title" />
      </p>
      <p>
        <asp:label AssociatedControlID="tb_companyBillingEmail" CssClass="inputLabel" runat="server">Billing Email</asp:label>
        <asp:TextBox ID="tb_companyBillingEmail" runat="server" CssClass="email title"/>
      </p>
      
    </fieldset>

    <fieldset>
    <legend>Services</legend>
      <p>
      <em>Share your ideas, topics and photos related to umbraco.</em>
      </p>    
      <p>
        <asp:label ID="Label5" AssociatedControlID="tb_twitter" CssClass="inputLabel" runat="server">Twitter Alias</asp:label>
        <asp:TextBox ID="tb_twitter" runat="server" onBlur="lookupTwitter(this);" CssClass="title"/>
      </p>
      <p>
        <asp:label ID="Label6" AssociatedControlID="tb_flickr" CssClass="inputLabel" runat="server">Flickr Alias</asp:label>
        <asp:TextBox ID="tb_flickr" runat="server" onBlur="lookupFlickr(this);" CssClass="title"/>
      </p>
      <p style="display: none !Important;"> 
        <asp:TextBox ID="tb_emailConfirm" runat="server" />
      </p>
    </fieldset>

    <fieldset>
    <legend>Newsletters and treshold</legend>
    <p>
    <em>Treshold is a way to control what items are displayed to you. Any item with a score <u>lower</u> then your set treshold, will not be displayed in the forum. </em>
    </p>
    <p>
        <asp:label ID="Label7" AssociatedControlID="tb_treshold" CssClass="inputLabel" runat="server">Treshold</asp:label>
        <asp:TextBox ID="tb_treshold" runat="server" Text="-10" ToolTip="Please enter a minimum score" TextMode="SingleLine" CssClass="number title"/>
    </p>

    <p>
      <asp:label ID="Label9" AssociatedControlID="cb_bugMeNot" CssClass="inputLabel" runat="server">Email</asp:label>
      <asp:CheckBox ID="cb_bugMeNot" runat="server" /> <asp:Label ID="Label8" AssociatedControlID="cb_bugMeNot" runat="server">Do not send me any notifications or newsletters from our.umbraco.org</asp:Label>
    </p>

    </fieldset>


    <fieldset>
    <legend>Where do you live?</legend>
     <p>
     <em>Tell us where you live, you can leave out streetnames, but city, zip-code and country is mandatory. When your location is displayed correctly on the map below,
     enough information has been provided.</em>
     </p>
 
     <p>
        <asp:TextBox ID="tb_location" runat="server" ToolTip="Please enter an address google maps can find" CssClass="title required" style="clear: both; width: 470px;"/> 
        <input type="button" class="submitButton" value="look up" onclick="lookupAddress(jQuery('#<%= tb_location.ClientID %>').val());" />
    
        <asp:HiddenField ID="tb_lat" runat="server" />
        <asp:HiddenField ID="tb_lng" runat="server" />
     </p>
 
     <div id="googleMap" style="width: 500px; height: 460px;"></div>
 
     <br />
    </fieldset>



    <div class="buttons">
    <asp:Button ID="bt_submit" Text="Sign up" CssClass="submitButton" OnClick="createMember" runat="server" />
    </div>

    </div>

    </asp:panel>

    
</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="VendorProfile" Visible="false">
    <div class="form simpleForm" id="vendorRegistrationForm">
    <fieldset>
    <legend>Vendor Information</legend>
      <p>
        This is the information for your Vendor profile that will be displayed to users.
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorCompany" CssClass="inputLabel" runat="server">Vendor Name</asp:label>
        <asp:TextBox ID="tb_vendorCompany" runat="server" ToolTip="Please enter your company name" CssClass="required title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorDescription" CssClass="inputLabel" runat="server">Company Bio</asp:label>
        <asp:TextBox ID="tb_vendorDescription" runat="server" ToolTip="Enter a short bio for your company" TextMode="MultiLine" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorUrl" CssClass="inputLabel" runat="server">Company Url</asp:label>
        <asp:TextBox ID="tb_vendorUrl" runat="server" ToolTip="Please enter your company url" CssClass="required title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorSupportUrl" CssClass="inputLabel" runat="server">Support Url</asp:label>
        <asp:TextBox ID="tb_vendorSupportUrl" runat="server" ToolTip="Please enter your support url" CssClass="required title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorBillingEmail" CssClass="inputLabel" runat="server">Billing email address</asp:label>
        <asp:TextBox ID="tb_vendorBillingEmail" runat="server" ToolTip="Please enter your billing email address" CssClass="required title email"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorSupportEmail" CssClass="inputLabel" runat="server">Support email address</asp:label>
        <asp:TextBox ID="tb_vendorSupportEmail" runat="server" ToolTip="Please enter your billing email address" CssClass="required title email"/>
      </p>
      <p>
        <asp:label AssociatedControlID="dd_vendorCountry" CssClass="inputLabel" runat="server">Country</asp:label>
        <Deli:CountryDropDownList runat="server" ID="dd_vendorCountry" CssClass="required title" ToolTip="Please select your country" />
      </p>
      </fieldset>
      <fieldset>
      <legend>Banking & Tax Details</legend>
      <p>Please fill in as much of these details as possible to make it easier for us to pay you</p>
      <p>
        <asp:label AssociatedControlID="tb_vendorBaseCurrency" CssClass="inputLabel" runat="server">Your Currency</asp:label>
        <asp:TextBox ID="tb_vendorBaseCurrency" runat="server" ToolTip="Please enter your currency" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorIban" CssClass="inputLabel" runat="server">IBAN</asp:label>
        <asp:TextBox ID="tb_vendorIban" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorSwift" CssClass="inputLabel" runat="server">SWIFT</asp:label>
        <asp:TextBox ID="tb_vendorSwift" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorBsb" CssClass="inputLabel" runat="server">BSB</asp:label>
        <asp:TextBox ID="tb_vendorBsb" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorAccount" CssClass="inputLabel" runat="server">Bank Account No.</asp:label>
        <asp:TextBox ID="tb_vendorAccount" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorPayPal" CssClass="inputLabel" runat="server">PayPal Account</asp:label>
        <asp:TextBox ID="tb_vendorPayPal" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorTaxId" CssClass="inputLabel" runat="server">Tax Id</asp:label>
        <asp:TextBox ID="tb_vendorTaxId" runat="server" CssClass="title"/>
      </p>
      <p>
        <asp:label AssociatedControlID="tb_vendorVatNumber" CssClass="inputLabel" runat="server">VAT Number</asp:label>
        <asp:TextBox ID="tb_vendorVatNumber" runat="server" CssClass="title"/>
      </p>
    </fieldset>
    <fieldset>
    <legend>Deli Vendor Terms & Conditions</legend>
    <p>By ticking the following box you are stating that you agree to the terms and conditions set out by Umbraco in the <a href="/wiki/deli/deli-vendor-terms">Deli Vendor Terms & Conditions document</a></p>
    <p>
      <asp:CheckBox ID="cb_vendorTerms" runat="server" class="required title"/> <asp:Label AssociatedControlID="cb_vendorTerms" runat="server">Do you Accept the Deli Vendor Terms & Conditions?</asp:Label>
    </p>
    </fieldset>

    <div class="buttons">
    <asp:Button ID="bt_vendorSubmit" Text="Save" CssClass="submitButton" OnClick="updateVendor" runat="server" />
    </div>
        </div>
</asp:PlaceHolder>

<script type="text/javascript">
    var map = null;
    var t_lat = null;
    var t_lng = null;
    var bio = null;

    $(document).ready(function () {
        t_lat = jQuery('#<%= tb_lat.ClientID %> ');
        t_lng = jQuery('#<%= tb_lng.ClientID %> ');
        bio = jQuery('#<%= tb_bio.ClientID %> ');

        jQuery.validator.addClassRules({
            password: {
                required: true,
                minlength: 5
            }
        });

        $.validator.addMethod("onlyValidLatLng",
               function (value, element) {
                   return (t_lat.val() != "" && t_lng.val() != "");
                   "Please enter an address google maps can find"
               });

        $.validator.addMethod("noHtml",
            function (value, element) {
                return (!bio.val().match(/<(\w+)((?:\s+\w+(?:\s*=\s*(?:(?:"[^"]*")|(?:'[^']*')|[^>\s]+))?)*)\s*(\/?)>/));
            },
                "No HTML allowed in this field"
            );

        // connect it to a css class
        jQuery.validator.addClassRules({
            noHtml: { noHtml: true }
        });

        $("form").validate({
            invalidHandler: function (f, v) {
                var errors = v.numberOfInvalids();
                if (errors.length > 0) {
                    validator.focusInvalid();
                }
            }
        });


        if (GBrowserIsCompatible()) {
            map = new GMap2(document.getElementById("googleMap"));

            if (t_lat.val() != "" && t_lng.val() != "") {
                var point = new GLatLng(t_lat.val(), t_lng.val());
                var marker = new GMarker(point);

                map.setCenter(point, 13);
                map.addOverlay(marker);
            } else {
                map.setCenter(new GLatLng(37.4419, -122.1419), 13);
            }

            map.setUIToDefault();
        }
    });

    function lookupAddress(address) {
        var geocoder = new GClientGeocoder();

        if (geocoder) {
            geocoder.getLatLng(
              address,
              function (point) {
                  if (!point) {

                      alert(address + " not found");

                      t_lat.val('');
                      t_lng.val('');

                  } else {

                      map.setCenter(point, 13);
                      t_lat.val(point.lat());
                      t_lng.val(point.lng());

                      var marker = new GMarker(point);
                      map.addOverlay(marker);
                      marker.openInfoWindowHtml(address);
                  }
              }
            );
        }
    }  
    </script>

    <script src="//maps.google.com/maps?file=api&amp;v=2&amp;sensor=false&amp;key=ABQIAAAA0NU1XDEzOML2eyLWhmJ9LBSxfxjTTu64lrS209cfOxNPw1orBxShNTRVj48sdN3ldWVic17nG0GLeA" type="text/javascript"></script>