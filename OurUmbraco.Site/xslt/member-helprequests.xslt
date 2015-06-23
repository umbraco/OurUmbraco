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

    <xsl:variable name="helpBase" select="'/wiki/umbraco-help'" />
    
<xsl:template match="/">

<!-- start writing XSLT -->

  <h2>Content</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'content'" />
  </xsl:call-template>
  <h2>Media</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'media'" />
  </xsl:call-template>
  <h2>Users</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'users'" />
  </xsl:call-template>
  <h2>Settings</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'settings'" />
  </xsl:call-template>
  <h2>Developer</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'developer'" />
  </xsl:call-template>
  <h2>Members</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'member'" />
  </xsl:call-template>
  <h2>Translation</h2>
  <xsl:call-template name="outputSectionRequests">
    <xsl:with-param name="section" select="'translation'" />
  </xsl:call-template>
  
</xsl:template>
    
<xsl:template name="outputSectionRequests">
    <xsl:param name="section" />
  

  
    <xsl:variable name="requests" select="uWiki:GetWikiHelpRequests($section)" />
  
     
    <xsl:choose>
      <xsl:when test="count($requests//requests)">
          <ul>
              <xsl:for-each select="$requests//requests">
                <li><xsl:value-of select="./applicationPage"/> - <a href="{$helpBase}/{$section}/{./applicationPage}?wikiEditor=y" rel="{./applicationPage}">Create</a></li>
            </xsl:for-each>
          </ul>
      </xsl:when>
      <xsl:otherwise>
        <p>Currently there are no missing help pages for this section.</p>
      </xsl:otherwise>
    </xsl:choose>
   
  
</xsl:template>

</xsl:stylesheet>