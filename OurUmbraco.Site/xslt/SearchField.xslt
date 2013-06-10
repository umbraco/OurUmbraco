<?xml version="1.0" encoding="UTF-8"?>
<!--
	SearchField.xslt
	
	Renders the search box
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
	<!ENTITY hellip "&#x2026;">
	<!ENTITY queryParam "q">
	<!ENTITY contentParam "content">
]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications"
>

	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:variable name="q" select="umbraco.library:RequestQueryString('&queryParam;')"/>
	<xsl:variable name="c" select="umbraco.library:RequestQueryString('&contentParam;')"/>
  
  <xsl:variable name="areas" select="umbraco.library:Split('Wiki|Forum|Projects|Documentation', '|')"/>
  <xsl:variable name="areaAlias" select="umbraco.library:Split('wiki|forum|project|documentation', '|')"/>
	
	<xsl:variable name="communityNodeId" select="1052" />

	<!-- Name of current section, if one of the $areas from above -->
	<xsl:variable name="currentArea" select="$currentPage/ancestor-or-self::*[@nodeName = $areas/value][@parentID = $communityNodeId]/@nodeName" />

	<xsl:variable name="noSpecificArea" select="not(normalize-space($c))" />
	<xsl:variable name="noCurrentArea" select="not(normalize-space($currentArea))" />

	<xsl:template match="/">
		<div id="searchBar">
			<fieldset id="selectsearch">
				<label id="searchlabel" class="visuallyhidden" for="searchField">Search for topics, documentation and packages...</label>
				<input type="text" id="searchField" name="&queryParam;" value="{$q}" placeholder="Search for topics, documentation and packages..." autocomplete="off" />
				<button id="searchbutton">Go</button>
			</fieldset>
		</div>
	</xsl:template>

</xsl:stylesheet>