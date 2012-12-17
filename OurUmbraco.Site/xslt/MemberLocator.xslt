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

<div id="memlocresults">
<xsl:choose>


<xsl:when test="count(/root/member [@id != umbraco.library:GetCurrentMember()/@id]) = 0">
<div class="error">
	<xsl:choose>
	<xsl:when test="string-length(/root/location/@name) &gt; 0">
	<h4>No members found</h4>
	<p>Looks like there or no members within <xsl:value-of select="/root/location/@radius"/>&nbsp;<xsl:value-of select="/root/location/@unit"/> of <xsl:value-of select="/root/location/@name"/></p>
	</xsl:when>
	<xsl:otherwise>
	<h4>Location not found</h4>

	</xsl:otherwise>
	</xsl:choose>
</div>
</xsl:when>
<xsl:otherwise>






<div class="mapresult">
 

<div id="resultlist" style="float:right;width:265px;text-align:left;">


<div class="box">
<h4><xsl:value-of select="count(/root/member [@id != umbraco.library:GetCurrentMember()/@id and string-length(@name) &gt; 0]) "/> members found within <xsl:value-of select="/root/location/@radius"/>&nbsp;KM of <xsl:value-of select="/root/location/@name"/>:</h4>
<ul class="forumTopics summary" style="height:535px;overflow:auto;">
<xsl:for-each select="/root/member [@id != umbraco.library:GetCurrentMember()/@id]">
	<xsl:sort select="./data [@alias = 'distance']" data-type="number"/>	
	<xsl:variable name="member" select="." />
	<xsl:if test="string-length($member/@name) &gt; 0">
	
	<li>
		
	
	
	<xsl:choose>
		
		<xsl:when test="string-length($member/data [@alias = 'avatar']) &gt; 0">
			<img src="{$member/data [@alias = 'avatar']}" alt="avatar of {$member/@name}" style="height:32px;width:32px"/>
		</xsl:when>
		<xsl:otherwise>
			<img src="/media/avatar/defaultavatar.png" alt="avatar of {$member/@name}" style="height:32px;width:32px"/>
		</xsl:otherwise>
	</xsl:choose>
	
	<a href="javascript:GEvent.trigger(marker{@id},'click');"><xsl:value-of select="$member/@name"/> </a>
	<small><xsl:value-of select="round(./data [@alias = 'distance'])"/>&nbsp;<xsl:value-of select="/root/location/@unit"/></small>
	</li>

</xsl:if>
</xsl:for-each>
</ul>
</div>


</div>

<div id="map_canvas" style="width: 700px; height: 600px"></div>

</div>








</xsl:otherwise>
</xsl:choose>
</div>
</xsl:template>

</xsl:stylesheet>