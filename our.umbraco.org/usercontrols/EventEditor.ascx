<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EventEditor.ascx.cs" Inherits="our.usercontrols.EventEditor" %>
<%@ Register TagPrefix="our" Assembly="our.umbraco.org" Namespace="our.controls" %>
<link rel="stylesheet" media="all" type="text/css" href="/css/ui-lightness/jquery-ui-1.8.16.custom.css" />

<script src="/scripts/libs/jquery-ui-timepicker-addon.js" type="text/javascript"></script>


<script type="text/javascript">

    jQuery(function ($) {
        $("#<%= dp_startdate.ClientID %>").datetimepicker();
        $("#<%= dp_enddate.ClientID %>").datetimepicker();
    });

</script>
<asp:Panel ID="eventForm" runat="server">
    <div class="form simpleForm eventForm">
    
    <fieldset>
    <legend>Event Information</legend>
      <p>
        Tell us about the event, the venue and the kind of people you would like to see participating.
      </p>
      
      <p>
        <asp:label ID="Label1" AssociatedControlID="tb_name" CssClass="inputLabel" runat="server">Name of event</asp:label>
        <asp:TextBox ID="tb_name" runat="server" ToolTip="Please enter the event name" CssClass="required title" />
      </p>
      
      <p>
        <asp:label ID="Label2" AssociatedControlID="tb_desc" CssClass="inputLabel" runat="server">Agenda</asp:label>
        <asp:TextBox ID="tb_desc" runat="server" TextMode="MultiLine" ToolTip="Please enter a description, explaining what the event is about" CssClass="title "/>
      </p>
      
      <p>
        <asp:label ID="Label6" AssociatedControlID="tb_capacity" CssClass="inputLabel" runat="server">Maximum number of attendees</asp:label>
        <asp:TextBox ID="tb_capacity" runat="server" ToolTip="Please enter the max number of attendees" CssClass="title required number"/>
      </p>
      
     </fieldset>
     
      <fieldset>
      <legend>Time</legend>     
      <p>When will it start? when will it end?</p>
       
      <p>
        <asp:label ID="Label3" AssociatedControlID="dp_startdate" CssClass="inputLabel"  runat="server">Start</asp:label>
        
        <div class="datepicker">
            <asp:textbox id="dp_startdate" ToolTip="Please select the start date and time"  runat="server" CssClass="required title"/>
        </div>
     
      </p>
      
      
      <p>
        <asp:label ID="Label4" AssociatedControlID="dp_enddate" CssClass="inputLabel" runat="server">End</asp:label>
        
        <div class="datepicker">
            <asp:TextBox id="dp_enddate" runat="server" ToolTip="Please select the end date and time" CssClass="required title"/>
        </div>
        
      </p>
    
    </fieldset>
     
     <fieldset> 
      <legend>Location</legend>
      <p>Where will the event be? , please enter a full address that google maps can show correctly.</p>
       
       
       <p>
        <asp:Label ID="Label5" AssociatedControlID="tb_venue" CssClass="inputLabel" runat="server">Venue</asp:Label>
        
        <table>
        <tr>
            <td style="vertical-align: top">
                <asp:TextBox ID="tb_venue" runat="server" TextMode="MultiLine" ToolTip="Please enter the complete address of the event venue" CssClass="title required"/> <br />
                <input type="button" class="submitButton" value="look up" onclick="lookupAddress(jQuery('#<%= tb_venue.ClientID %>').val());" />
            </td>
            <td style="vertical-align: top">
                <div id="googleMap" style="width: 320px; height: 270px;"></div>
            </td>
        </tr>
        </table>
        
        <asp:HiddenField ID="tb_lat" runat="server" />
        <asp:HiddenField ID="tb_lng" runat="server" />
     </p>
     
     
     
     </fieldset>
    
    <div class="buttons">
        <asp:Button ID="bt_submit" Text="Save" CssClass="submitButton" OnClick="createEvent" runat="server" />
    </div>

    </div>
    
    
    <script type="text/javascript">
      var map = null;
      var t_lat = null;
      var t_lng = null;
      var latlng = null;
      var markersArray = [];
  
      $(document).ready(function() {
          t_lat = jQuery('#<%= tb_lat.ClientID %>');
          t_lng = jQuery('#<%= tb_lng.ClientID %>');
          
          
           $.validator.addMethod("onlyValidLatLng",
           function(value, element) {
             return (t_lat.val() != "" && t_lng.val() != "");
             "Please enter an address google maps can find"
           });
          
          
          $("form").validate();
          

              if (t_lat.val() != "" && t_lng.val() != "") {
                  latlng = new google.maps.LatLng(t_lat.val(), t_lng.val());
              }
              else{
                  latlng = new google.maps.LatLng(37.4419, -122.1419);
              }

              var mapopts = {
                zoom: 8,
                center: latlng,
                mapTypeId: google.maps.MapTypeId.ROADMAP
              }

                map = new google.maps.Map(document.getElementById("googleMap"),mapopts);

              if (t_lat.val() != "" && t_lng.val() != "") {

                var marker = new google.maps.Marker({
                  map: map,
                  position: latlng
                });

                markersArray.push(marker);
              }

           // map.setUIToDefault();
          
      });

      tinyMCE.init({
        // General options
        mode: "exact",
        elements: "<%= tb_desc.ClientID %>",
        content_css: "/css/fonts.css",
        auto_resize: true,
        theme: "simple",
        remove_linebreaks: false
      });
    
      
function lookupAddress(address){
  
  var geocoder = new google.maps.Geocoder();
  
  if (geocoder) {
    geocoder.geocode({ 'address': address.replace(/(\r\n|\n|\r)/gm," ")},function(results, status) {
      if (status == google.maps.GeocoderStatus.OK) {
        map.setCenter(results[0].geometry.location);
        clearOverlays();
        var marker = new google.maps.Marker({
          map: map,
          position: results[0].geometry.location
        });
        var infoWindow = new google.maps.InfoWindow();
        infoWindow.setContent(address.replace(/(\r\n|\n|\r)/gm,"<br//>"));
        infoWindow.open(map, marker);
        markersArray.push(marker);
        t_lat.val(results[0].geometry.location.lat());
        t_lng.val(results[0].geometry.location.lng());
      } else {
        alert("Unable to find addres for the following reason: " + status);
      }
    });
  }

  /*var geocoder = new GClientGeocoder();
  


  if (geocoder) {
    geocoder.getLatLng(
          address,
          function(point) {
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
      }*/
 }  

 function clearOverlays() {
  if (markersArray) {
    for (i in markersArray) {
      markersArray[i].setMap(null);
    }
  }
}
      
</script>

<script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyBofhf4tHWAJ_X3NfirX-hgnlrgcBeyrSo&sensor=false" type="text/javascript"></script>
        
</asp:Panel>