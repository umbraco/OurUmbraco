<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications" xmlns:deli.library="urn:deli.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uPowers uEvents MemberLocator umbracoTags.library our.library Notifications deli.library ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">
  
  <xsl:variable name="licensePage" select="umbraco.library:NiceUrl(/macro/licenses)" />
  <xsl:variable name="favPage" select="umbraco.library:NiceUrl(/macro/favs)" />
  <xsl:variable name="searchPage" select="umbraco.library:NiceUrl(/macro/search)" />
  
  
    <div id="profile">
      
      <xsl:if test="umbraco.library:IsLoggedOn()">
      <xsl:variable name="mem" select="umbraco.library:GetCurrentMember()"/>
  
      You are logged in as: 
      <xsl:value-of select="$mem/@nodeName" />
      |
      <a href="{$favPage}">Favorites</a>
      |
      <a href="{$licensePage}">Licenses</a>
      </xsl:if>
      <!--
      <input type="text" id="tb_search" /><button onClick="goSearch('#tb_search','{$searchPage}'); return false;">Search</button> 
      -->
    </div>
     
  
</xsl:template>

</xsl:stylesheet>