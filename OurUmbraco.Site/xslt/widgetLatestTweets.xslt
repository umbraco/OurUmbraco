<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"

	xmlns:google="http://base.google.com/ns/1.0" xmlns:openSearch="http://a9.com/-/spec/opensearch/1.1/" xmlns:twitter="http://api.twitter.com/"
	xmlns:atom="http://www.w3.org/2005/Atom"
	xmlns:thr="http://purl.org/syndication/thread/1.0"
	xmlns="http://www.w3.org/2005/Atom"

	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">

<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:template match="/">

<xsl:variable name="maxItems" select="/macro/maxItems" />
<xsl:variable name="tweets" select="umbraco.library:GetXmlDocumentByUrl('http://search.twitter.com/search.atom?q=%23umbraco', 1000)"/>

<ul class="twitter summary">
<xsl:for-each select="$tweets//atom:entry">
<xsl:sort select="published" order="descending"/>
<xsl:if test="position() &lt; $maxItems">
	<li>
		<small><xsl:value-of select="atom:content" disable-output-escaping="yes"/></small>
		<a href="{atom:link/@href}">
			<xsl:value-of select="atom:author/atom:name"/>
		</a>
	</li>
</xsl:if>
</xsl:for-each>
</ul>


</xsl:template>

</xsl:stylesheet>