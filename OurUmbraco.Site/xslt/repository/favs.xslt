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

  <xsl:if test="umbraco.library:IsLoggedOn()">
    
    <xsl:variable name="items" select="uPowers:ItemsVotedFor(uForum:GetCurrentMember()/@id, 'powersProject')"/>
    <ul>
    <xsl:for-each select="$items/items/item/id">
      
    <xsl:variable name="node" select="umbraco.library:GetXmlNodeById(.)"/>
      <xsl:if test="$node/@nodeName != ''">  
    <li class="clearfix">
        <div class="deliPackage">
            <div class="brief">
              <a href="packageLink" class="packageIcon" style="background:url(/umbraco/imagegen.aspx?image={$node/defaultScreenshotPath}&amp;altImage=/css/img/package.png&amp;pad=true&amp;width=50&amp;height=50) no-repeat top left;">Package</a>
              <h3><a href="{umbraco.library:NiceUrl($node/@id)}">
                  <xsl:value-of select="$node/@nodeName" />  
                </a></h3>
            </div>
            <div class="hiLite">
                <p><xsl:value-of select="umbraco.library:Replace(umbraco.library:TruncateString(umbraco.library:StripHtml($node/description), 160, '...'),'==','')"/></p>
            </div>
            <div class="category">By <a href="profilelink"><xsl:value-of select="umbraco.library:GetMemberName($node/owner)"/></a></div>
        </div>
    </li>
      </xsl:if>
    </xsl:for-each>
      </ul>
  </xsl:if>
  
</xsl:template>

</xsl:stylesheet>