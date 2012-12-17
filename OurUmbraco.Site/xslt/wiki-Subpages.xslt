<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum ">


<xsl:output method="xml" omit-xml-declaration="yes" />

<xsl:param name="currentPage"/>

<xsl:template match="/">
<div id="wiki_subpages">
<xsl:choose>
<xsl:when test="count($currentPage/* [@isDoc]) &gt; 0"> 
  <xsl:call-template name="drawNodes"> 
    <xsl:with-param name="parent" select="$currentPage"/>
  </xsl:call-template>
</xsl:when>
<xsl:otherwise>
  <xsl:call-template name="drawNodes"> 
    <xsl:with-param name="parent" select="$currentPage/.."/>
  </xsl:call-template>
</xsl:otherwise>
</xsl:choose>  
</div>
</xsl:template>

<xsl:template name="drawNodes">
<xsl:param name="parent"/> 
<xsl:if test="umbraco.library:IsProtected($parent/@id, $parent/@path) = 0 or (umbraco.library:IsProtected($parent/@id, $parent/@path) = 1 and umbraco.library:IsLoggedOn() = 1)">
<ul>
<xsl:for-each select="$parent/* [@isDoc and string(umbracoNaviHide) != '1']"> 
<li>  
<a href="{umbraco.library:NiceUrl(@id)}">
<xsl:value-of select="@nodeName"/></a>  
<xsl:if test="count(./* [@isDoc and string(umbracoNaviHide) != '1']) &gt; 0">   
<xsl:call-template name="drawNodes">    
<xsl:with-param name="parent" select="."/>    
</xsl:call-template>  
</xsl:if> 
</li>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>
</xsl:stylesheet>