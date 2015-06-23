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

<!-- Don't change this but create a 'number' element in your -->
<!-- macro with the alias of 'numberOfItems' -->
<xsl:variable name="numberOfItems" select="/macro/numberOfItems"/>

<xsl:template match="/">

<h2>New Pages</h2>
<ul>
<xsl:for-each select="$currentPage//* [@isDoc and string(umbracoNaviHide) != '1']">
<xsl:sort select="@createDate" order="descending"/>
  <xsl:if test="position() &lt;= $numberOfItems">
    <li>
    <h3>
      <a href="{umbraco.library:NiceUrl(@id)}">
        <xsl:value-of select="@nodeName"/>
      </a>
    </h3>
    Created by Someone... 
    </li>
  </xsl:if>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>