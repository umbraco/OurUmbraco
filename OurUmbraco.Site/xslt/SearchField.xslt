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

	<xsl:variable name="areas" select="umbraco.library:Split('Wiki|Forum|Projects', '|')"/>
	<xsl:variable name="areaAlias" select="umbraco.library:Split('wiki|forum|project', '|')"/>
	
	<xsl:variable name="communityNodeId" select="1052" />

	<!-- Name of current section, if one of the $areas from above -->
	<xsl:variable name="currentArea" select="$currentPage/ancestor-or-self::*[@nodeName = $areas/value][@parentID = $communityNodeId]/@nodeName" />

	<xsl:variable name="noSpecificArea" select="not(normalize-space($c))" />
	<xsl:variable name="noCurrentArea" select="not(normalize-space($currentArea))" />

	<xsl:template match="/">
		<div id="searchBar">
			<fieldset id="selectsearch">
				<label id="searchlabel" for="searchField">What were you looking for in&hellip;</label>
				<input type="text" id="searchField" name="&queryParam;" value="{$q}" />
				<span id="sectionspan">
					<label>All</label>
				</span>
				<div id="sections" class="clearfix">
					<label class="sectiontab">All</label>
					<p>Search In</p>
					<xsl:for-each select="$areas/value">
						<xsl:variable name="index" select="position()"/>
						<xsl:variable name="alias" select="$areaAlias/value[$index]"/>
						<input type="checkbox" id="s_{.}" name="contentType" value="{$alias}">
							<!-- If no area was specified and we're inside a specific area, select only that -->
							<xsl:if test="$noSpecificArea and current() = $currentArea"><xsl:attribute name="checked">checked</xsl:attribute></xsl:if>

							<!-- If this section was specifically searched in, select it -->
							<xsl:if test="contains($c, $alias)"><xsl:attribute name="checked">checked</xsl:attribute></xsl:if>

							<!-- If we're not in any specific area and nothing was specified, select it so we search everything -->
							<xsl:if test="$noCurrentArea and $noSpecificArea"><xsl:attribute name="checked">checked</xsl:attribute></xsl:if>
						</input>
						<label for="s_{.}">
							<xsl:value-of select="."/>
						</label>
					</xsl:for-each>
				</div>
				<button id="searchbutton">Go</button>
			</fieldset>
		</div>
	</xsl:template>

</xsl:stylesheet>