<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:geo="http://www.w3.org/2003/01/geo/wgs84_pos#" xmlns:content="http://purl.org/rss/1.0/modules/content/" xmlns:media="http://search.yahoo.com/mrss/" 
	xmlns:yt="http://gdata.youtube.com/schemas/2007"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
	exclude-result-prefixes="geo content media yt msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">
<xsl:variable name="maxItems" select="/macro/maxItems" />

<xsl:variable name="feedcache" select="document('../App_Data/communityblogs.xml')"/>

<xsl:variable name="blogs" select="$feedcache">

</xsl:variable>


<ul class="blogs summary">
<xsl:for-each select="$blogs/rss/channel/item">
<xsl:sort select="umbraco.library:FormatDateTime(pubDate, 'yyyyMMdd')" order="descending"/>

<xsl:if test="position() &lt;= number($maxItems)">
	<li><a href="{link}"><xsl:value-of select="title"/></a>
		<small><xsl:value-of select="umbraco.library:TruncateString( umbraco.library:StripHtml(description) , 100, '..')" /></small>
	</li>
</xsl:if>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>