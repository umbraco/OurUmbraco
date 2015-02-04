<?xml version="1.0" encoding="UTF-8"?>
<!--
	Breadcrumb.xslt

	Renders the breadcrumb trail
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
	<!ENTITY separator "&#187;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uWiki="urn:uWiki"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uWiki"
>

	<xsl:output method="html" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:variable name="minLevel" select="1"/>
	<xsl:variable name="linkToCurrent" select="/macro/linkToCurrent"/>

	<xsl:template match="/">
    <xsl:if test="$currentPage/@level &gt; $minLevel">
      <ul id="breadcrumb">
        <li>
          <a href="/">Our</a>
        </li>
        <xsl:for-each select="$currentPage/ancestor::*[@isDoc and @level &gt; $minLevel][not(umbracoNaviHide = 1)]">
          <li>
            <a href="{umbraco.library:NiceUrl(@id)}">
              <xsl:value-of select="@nodeName"/>
            </a>
            <xsl:if test="$linkToCurrent = 1 or position() &lt; last()">&separator;</xsl:if>
          </li>
        </xsl:for-each>
        <!-- print currentpage? -->
        <xsl:if test="$linkToCurrent = 1">
          <li>
            <a href="{umbraco.library:NiceUrl($currentPage/@id)}">
              <xsl:value-of select="$currentPage/@nodeName"/>
            </a>
          </li>
        </xsl:if>
      </ul>
    </xsl:if>
	</xsl:template>

</xsl:stylesheet>