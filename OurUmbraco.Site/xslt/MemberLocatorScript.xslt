<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:MemberLocator="urn:MemberLocator" xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh MemberLocator">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:variable name="zoomlevel">
	<xsl:choose>
	<xsl:when test="number(/root/location/@radius) = 1000">
		<xsl:value-of select="5"/>
	</xsl:when>
	<xsl:when test="number(/root/location/@radius) &lt; 1000 and number(/root/location/@radius) &gt;= 300">
		<xsl:value-of select="6"/>
	</xsl:when>
	<xsl:when test="number(/root/location/@radius) &lt; 300 and number(/root/location/@radius) &gt;= 200">
		<xsl:value-of select="7"/>
	</xsl:when>
	<xsl:when test="number(/root/location/@radius) &lt; 200 and number(/root/location/@radius) &gt;= 100">
		<xsl:value-of select="8"/>
	</xsl:when>
	<xsl:when test="number(/root/location/@radius) &lt; 100 and number(/root/location/@radius) &gt;= 50">
		<xsl:value-of select="9"/>
	</xsl:when>
	<xsl:otherwise>
		<xsl:value-of select="5"/>
	</xsl:otherwise>
</xsl:choose>

</xsl:variable>
<xsl:template match="/">

<xsl:choose>


<xsl:when test="count(/root/member [@id != umbraco.library:GetCurrentMember()/@id]) = 0">
	document.getElementById('memberlocatorextra').style.display = 'block';
</xsl:when>
<xsl:otherwise>

    <xsl:text disable-output-escaping="yes">
    <![CDATA[

          if (GBrowserIsCompatible()) {

            var map = new GMap2(document.getElementById("map_canvas"));
	    map.addControl(new GLargeMapControl());

    ]]>
    </xsl:text>
   
    map.setCenter(new GLatLng(<xsl:value-of select="/root/location"/>), <xsl:value-of select="$zoomlevel"/>);
        
	<xsl:for-each select="/root/member [@id != umbraco.library:GetCurrentMember()/@id]">
	<xsl:sort select="./data [@alias = 'distance']" data-type="number"/>	
	<xsl:if test="string-length(./@name) &gt; 0">

	<xsl:variable name="avatar">
	
	<xsl:choose>
		<xsl:when test="string-length(./data [@alias = 'avatar']) &gt; 0">
			<xsl:value-of select="./data [@alias = 'avatar']"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:text>/media/avatar/defaultavatar.png</xsl:text>			
		</xsl:otherwise>
	</xsl:choose>
	
	</xsl:variable>

		var latlng<xsl:value-of select="@id"/> = new GLatLng(<xsl:value-of select="./data [@alias = 'location']"/>);
 		 var marker<xsl:value-of select="@id"/> = new GMarker(latlng<xsl:value-of select="@id"/>)

	 GEvent.addListener(marker<xsl:value-of select="@id"/>, "click", function() {

	var theMemberDiv = document.createElement('div');
	var theImageDiv = document.createElement('div');
	theImageDiv.setAttribute('style','float:left');

	var theImage = document.createElement('img');
	theImage.setAttribute('style','height: 40px; width: 40px;');
	theImage.setAttribute('src','<xsl:value-of select="$avatar"/>');

	theImageDiv.appendChild(theImage);

	var theTextDiv = document.createElement('div');
	theTextDiv.setAttribute('style','float:left;margin-left:5px;');

	var theBoldBit = document.createElement('b');
	var theLink = document.createElement('a');
	theLink.setAttribute('href','/member/' + '<xsl:value-of select="@id"/>');
	var theText1 = document.createTextNode("<xsl:value-of select="umbraco.library:HtmlEncode(./@name)"/>");
	var theText2 = document.createTextNode('Karma: ' + '<xsl:value-of select="./@karma"/>');
	theLink.appendChild(theText1);
	theBoldBit.appendChild(theLink);
	theTextDiv.appendChild(theBoldBit);
	theTextDiv.appendChild(document.createElement('br'));
	theTextDiv.appendChild(theText2);

	
	theMemberDiv.appendChild(theImageDiv);
	theMemberDiv.appendChild(theTextDiv);

  	map.openInfoWindowHtml(latlng<xsl:value-of select="@id"/>,  theMemberDiv);

  		      });


	map.addOverlay(marker<xsl:value-of select="@id"/>);
	</xsl:if>
	</xsl:for-each>

      <xsl:text disable-output-escaping="yes">
      <![CDATA[     

////////////////////////// circle///////////////////////////////
	
function drawCircle(center, radius, nodes, liColor, liWidth, liOpa, fillColor, fillOpa)
{
var bounds = new GLatLngBounds();


	//calculating km/degree
	var latConv = center.distanceFrom(new GLatLng(center.lat()+0.1, center.lng()))/100;
	var lngConv = center.distanceFrom(new GLatLng(center.lat(), center.lng()+0.1))/100;

	//Loop 
	var points = [];
	var step = parseInt(360/nodes)||10;
	for(var i=0; i<=360; i+=step)
	{
	var pint = new GLatLng(center.lat() + (radius/latConv * Math.cos(i * Math.PI/180)), center.lng() + 
	(radius/lngConv * Math.sin(i * Math.PI/180)));
	points.push(pint);
	bounds.extend(pint); //this is for fit function
	}
	points.push(points[0]); // Closes the circle, thanks Martin
	fillColor = fillColor||liColor||"#FF6E0E";
	liWidth = liWidth||2;
	var poly = new GPolygon(points,"#FF6E0E",liWidth,liOpa,fillColor,fillOpa);
	map.addOverlay(poly);
}


/////////////////////////////////////////////////////////////////////

      ]]>
      </xsl:text>

	drawCircle( map.getCenter(), <xsl:value-of select="round(number(/root/location/@radius) * 1.025)"/>, 50);

	document.getElementById('memberlocatorextra').style.display = 'block';
	document.getElementById('memberlocatorsearch').style.display = 'block';

	//topictiny();
    <xsl:text disable-output-escaping="yes">
    <![CDATA[

            }



      ]]>
      </xsl:text>

</xsl:otherwise>
</xsl:choose>
</xsl:template>

</xsl:stylesheet>