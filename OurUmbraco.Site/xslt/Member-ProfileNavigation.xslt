<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:deli.library="urn:deli.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki deli.library">

<xsl:output method="xml" omit-xml-declaration="yes" />

<xsl:param name="currentPage"/>

<!-- Don't change this, but add a 'contentPicker' element to -->
<!-- your macro with an alias named 'source' -->
<xsl:variable name="source" select="umbraco.library:GetXmlNodeById(/macro/source)"/>
<xsl:variable name="mem" select="umbraco.library:GetCurrentMember()"/>

<xsl:template match="/">

<ul>
<li>
  <xsl:if test="$currentPage/@id = $source/@id">
    <xsl:attribute name="class">current</xsl:attribute>
  </xsl:if>
  <a href="{umbraco.library:NiceUrl($source/@id)}"><xsl:value-of select="$source/@nodeName"/></a>
</li>
<xsl:for-each select="$source/* [@isDoc and umbracoNaviHide != '1']">
  <xsl:if test="umbraco.library:HasAccess(@id,@path)">
      <xsl:if test="deli.library:HasVendorAccess(@id)">
        <li>
          <xsl:if test="contains($currentPage/@path,@id)">
            <xsl:attribute name="class">current</xsl:attribute>
          </xsl:if>
          
          <a href="{umbraco.library:NiceUrl(@id)}">
            <xsl:value-of select="@nodeName"/>
          </a>
        </li>
      </xsl:if>
  </xsl:if>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>