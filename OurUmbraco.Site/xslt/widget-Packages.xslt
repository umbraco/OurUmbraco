<?xml version="1.0" encoding="UTF-8"?>
<!--
	widget-Packages.xslt
	
	Renders the list of projects
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt" 
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh"
>

	<xsl:output method="xml" omit-xml-declaration="yes" />

	<xsl:param name="currentPage"/>

	<!-- Don't change this, but add a 'contentPicker' element to -->
	<!-- your macro with an alias named 'source' -->
	<xsl:variable name="source" select="/macro/source"/>
	<xsl:variable name="maxitems" select="/macro/maxitems"/>

	<xsl:template match="/">
		<ul class="projects summary">
			<xsl:for-each select="umbraco.library:GetXmlNodeById(1113)/descendant::Project">
				<xsl:sort select="@nodeName" order="ascending"/>
				<li>
					<a href="https://our.umbraco.com{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="@nodeName"/></a>
					<xsl:value-of select="concat('Version: ', version, ', by ', umbraco.library:GetMemberName(owner))"/>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>

</xsl:stylesheet>