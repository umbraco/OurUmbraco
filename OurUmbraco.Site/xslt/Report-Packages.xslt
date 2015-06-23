<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications" xmlns:deli.library="urn:deli.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications deli.library ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<!-- start writing XSLT -->
  <xsl:variable name="startdate" select="'1/6/2008'"/>
  <xsl:variable name="enddate" select="'12/9/2012'"/>

  <p>Projects released after enddate: 
    <xsl:value-of select="count($currentPage//Project [umbraco.library:DateGreaterThan(@createDate, $enddate) = 'true' and projectLive = '1'])"/>
  </p>
  <p>Projects released after v5 release but before CG12: 
    <xsl:value-of select="count($currentPage//Project [umbraco.library:DateGreaterThan(@createDate, $startdate) = 'true' and umbraco.library:DateGreaterThan(@createDate, $enddate) != 'true' and projectLive = '1'])"/>
  </p>
  <h1>Releases</h1>
  <table>
    <tr>
      <th>Project</th>
      <th>Release Date</th>
    </tr>
    <xsl:for-each select="$currentPage//Project [umbraco.library:DateGreaterThan(@createDate, $startdate) = 'true' and umbraco.library:DateGreaterThan(@createDate, $enddate) != 'true' and projectLive = '1']">
    <tr>
      <td>
        <xsl:value-of select="@nodeName"/>
      </td>
      <td>
        <xsl:value-of select="umbraco.library:ShortDate(@createDate)"/>
      </td>
    </tr>
    </xsl:for-each>
  </table>
  
</xsl:template>

</xsl:stylesheet>