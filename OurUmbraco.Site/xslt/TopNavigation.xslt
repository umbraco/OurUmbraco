<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets ">

  <xsl:output method="xml" omit-xml-declaration="yes" />

  <xsl:param name="currentPage"/>

  <!-- Input the documenttype you want here -->
  <xsl:variable name="level" select="1"/>

  <xsl:template match="/">

      <xsl:for-each select="$currentPage/ancestor-or-self::* [@level=$level]/* [@isDoc and string(umbracoNaviHide) != '1']">
        <li>
          <xsl:attribute name="class">
            <xsl:if test="(umbraco.library:LastIndexOf(umbraco.library:RequestServerVariables('URL'), 'download') = -1) and (@id = $currentPage/ancestor-or-self::*[@isDoc]/@id)">
              <xsl:text>current</xsl:text>
            </xsl:if>
            <xsl:if test="umbraco.library:LastIndexOf(umbraco.library:RequestServerVariables('URL'), 'download') != -1 and @nodeName = 'Download'">
              <xsl:text>current</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <a href="{umbraco.library:NiceUrl(@id)}">
            <xsl:value-of select="@nodeName"/>
          </a>
        </li>
      </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>