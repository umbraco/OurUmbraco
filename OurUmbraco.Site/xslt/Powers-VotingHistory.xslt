<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:variable name="key" select="/macro/key"/>
<xsl:variable name="type" select="/macro/type"/>

<xsl:template match="/">

<xsl:variable name="votes" select="uPowers:History( number($key) , concat('powers',$type))"/>

<xsl:if test="count($votes//vote [number(points) != 0]) = 0">
	<p><em>No-one has voted on this item yet</em></p>
</xsl:if>

<ul class="votingHistory">
	<xsl:for-each select="$votes//vote">
	<xsl:sort select="date" order="descending" />	
	<xsl:if test="number(points) != 0">
	<li class="up">
		<xsl:if test="number(points) &lt; 0"><xsl:attribute name="class">down</xsl:attribute></xsl:if>
		
		<div>
		<xsl:choose>
		<xsl:when test="string-length(comment) &gt; 4"><xsl:value-of select="comment"/></xsl:when>
		<xsl:when test="number(points) = 100">Marked this item as a solution</xsl:when>
		<xsl:when test="number(points) = 1">Like this item</xsl:when>
		<xsl:when test="number(points) = -1">Do not like this item</xsl:when>		
		</xsl:choose>

		
		</div>		
		<a href="/member/{memberId}"><xsl:value-of select="umbraco.library:GetMemberName(memberId)"/></a> , <small><xsl:value-of select="uForum:TimeDiff(date)"/></small> 		
	</li>
	</xsl:if>
	</xsl:for-each>
</ul>
</xsl:template>

</xsl:stylesheet>