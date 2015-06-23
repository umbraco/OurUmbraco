<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:variable name="mem"><xsl:if test="umbraco.library:IsLoggedOn()"><xsl:value-of select="uForum:GetCurrentMember()/@id"/></xsl:if></xsl:variable>
<xsl:variable name="forumid" select="$currentPage/@id" />

<xsl:template match="/">

<xsl:if test="umbraco.library:IsLoggedOn()">  

<xsl:if test="number($currentPage/forumAllowNewTopics) = 1">

<xsl:variable name="subscribed" select="Notifications:IsSubscribedToForum($forumid,$mem)" />
  
<a href="#" class="UnSubscribeForum notification" title="Unsubscribe from this forum" rel="{$forumid}">
  <xsl:if test="not($subscribed)">
    <xsl:attribute name="style"><xsl:text>display:none;</xsl:text></xsl:attribute>
  </xsl:if>
  Unsubscribe
</a>

<a href="#" class="SubscribeForum  notification" title="Subscribe to this forum" rel="{$forumid}">
  <xsl:if test="$subscribed">
    <xsl:attribute name="style"><xsl:text>display:none;</xsl:text></xsl:attribute>
  </xsl:if>
  Subscribe
</a>

</xsl:if>
</xsl:if>

</xsl:template>

</xsl:stylesheet>