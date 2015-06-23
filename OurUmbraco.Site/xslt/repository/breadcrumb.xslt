<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [  <!ENTITY nbsp "&#x00A0;">]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uWiki="urn:uWiki"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uWiki ">

  <xsl:output method="html" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>
    
  <xsl:variable name="id" select="umbraco.library:RequestQueryString('id')"/>
  <xsl:variable name="q" select="umbraco.library:RequestQueryString('q')"/>
  <xsl:variable name="version" select="umbraco.library:RequestQueryString('version')"/>
  <xsl:variable name="useLegacySchema" select="umbraco.library:RequestQueryString('useLegacySchema')"/>

  <xsl:variable name="root" select="umbraco.library:GetXmlNodeById(1113)"/>
  <xsl:variable name="cb" select="umbraco.library:Request('callback')"/>

    <!-- here we build the standard QS to attach to all links -->
  <xsl:variable name="qs" select="concat('callback=',$cb,'&amp;version=',$version,'&amp;useLegacySchema=', $useLegacySchema, '&amp;repo=true')"/>

      
  <xsl:template match="/">   
    
 <ul id="breadcrumb">
   <li><a href="/deli/?{$qs}">Umbraco Package Repository</a></li>

   
   <xsl:for-each select="$currentPage/parent::* [@isDoc]">
     <li>
      <xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
      <a href="{umbraco.library:NiceUrl(@id)}?{$qs}">
       <xsl:value-of select="@nodeName"/>
      </a>
     </li>
  </xsl:for-each>
     
<li>
    <xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
  <a href="?{$qs}">
      <xsl:value-of select="$currentPage/@nodeName" />
    </a>
    </li>
  </ul>

  </xsl:template>
</xsl:stylesheet>