<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Signup.ascx.cs" Inherits="our.usercontrols.Signup" %>

<asp:panel ID="MemberExists" Visible="False" runat="server">
    <p>There is already an account with this e-mail address! You can <a href="/member/login">login</a> or <a href="/member/forgot-password">reset your password</a>.</p>
</asp:panel>

<asp:panel ID="Panel1" runat="server" defaultbutton="bt_submit">
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
    <asp:label ID="Label10" AssociatedControlID="tb_bio" CssClass="inputLabel" runat="server">Bio<br/><small>No html allowed</small></asp:label>
    <asp:TextBox ID="tb_bio" runat="server" TextMode="MultiLine" CssClass="title noHtml"/>
  	
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
<legend>Newsletters and threshold</legend>
<p>
<em>Threshold is a way to control what items are displayed to you. Any item with a score <u>lower</u> than your set threshold, will not be displayed in the forum. </em>
</p>
<p>
    <asp:label ID="Label7" AssociatedControlID="tb_treshold" CssClass="inputLabel" runat="server">Threshold</asp:label>
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

 <style type="text/css">.comment-text { display: none; }</style>
 <span class="comment-text">
  <asp:TextBox runat="server" TextMode="MultiLine" ID="CommentBody"></asp:TextBox>
 </span>

</fieldset>


<div class="buttons">
<asp:Button ID="bt_submit" Text="Sign up" CssClass="submitButton" OnClick="CreateMember" runat="server" />
</div>

</div>

</asp:panel>

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



        $("form").validate();


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