<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator umbracoTags.library our.library ">

<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<!-- update this variable on how deep your site map should be -->
<xsl:variable name="self" select="umbraco.library:RequestQueryString('target')"/>

<xsl:template match="/">
<div id="sitemap"> 
<xsl:call-template name="drawNodes">  
  <xsl:with-param name="parent" select="umbraco.library:GetXmlNodeById(1054)"/>  
</xsl:call-template>
</div>
</xsl:template>

<xsl:template name="drawNodes">
<xsl:param name="parent"/> 

<xsl:if test=" (umbraco.library:IsProtected($parent/@id, $parent/@path) = 0 or (umbraco.library:IsProtected($parent/@id, $parent/@path) = 1 and umbraco.library:IsLoggedOn() = 1))">

<ul>
<xsl:if test="@level &gt; 1">
  <xsl:attribute name="style">display: none;</xsl:attribute>
</xsl:if>
<xsl:for-each select="$parent/* [@isDoc and string(umbracoNaviHide) != '1']"> 
<xsl:sort select="@nodeName"/>

<xsl:if test="number(@id) != number($self)">
<li>  
<a class="WikiMoveDo" href="{umbraco.library:NiceUrl(@id)}" rel="{@id}" id="{$self}">
<xsl:value-of select="@nodeName"/></a>
 
<xsl:if test="count(./* [@isDoc and string(umbracoNaviHide) != '1'] ) &gt; 0">   

<span class="toggle">Expand</span>

<xsl:call-template name="drawNodes">    
<xsl:with-param name="parent" select="."/>    
</xsl:call-template>  
</xsl:if> 

</li>
</xsl:if>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>
</xsl:stylesheet>