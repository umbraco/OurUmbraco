<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:variable name="maxItems" select="/macro/maxItems" />
    
<xsl:template match="/">
<ul class="projects summary">
<xsl:for-each select="uPowers:PopularItems('powersProject')//item">
<xsl:sort select="number(score)" order="descending" data-type="number"/>

<xsl:variable name="n" select="umbraco.library:GetXmlNodeById(id)"/>

<xsl:if test="position() &lt;= $maxItems">
  <li><a href="https://our.umbraco.com{umbraco.library:NiceUrl($n/@id)}">
    <xsl:value-of select="umbraco.library:TruncateString($n/@nodeName, 60, '...')"/></a>
  <small>Version: <xsl:value-of select="$n/version"/>,  by <xsl:value-of select="umbraco.library:GetMemberName($n/owner)"/> </small>
  </li>
</xsl:if>

</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>