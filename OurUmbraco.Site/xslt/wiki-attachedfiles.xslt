<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki ">

<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">
<xsl:variable name="files" select="uWiki:GetAttachedFiles($currentPage/@id)" />

<xsl:if test="count($files//file)">
<h3>Attached files</h3>
<ul id="attachedFiles">
<xsl:for-each select="$files//file">
	<xsl:if test="boolean(./current)"> 
	<li class="{type}">
		<a class="fileName" href="{path}"><xsl:value-of select="name" /></a>
		<small><xsl:value-of select="umbraco.library:GetDictionaryItem(type)"/> 
		- uploaded <xsl:value-of select="umbraco.library:ShortDate(createDate)"/>
		by <a href="/member/{createdBy}"><xsl:value-of select="umbraco.library:GetMemberName(createdBy)"/></a>
		</small>
	</li>
	</xsl:if>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>
</xsl:stylesheet>