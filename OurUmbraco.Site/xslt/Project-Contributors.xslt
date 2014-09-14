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

<xsl:template match="/">
<xsl:if test="umbraco.library:IsLoggedOn()">
  
   <h2>Collaboration</h2>
  <xsl:choose>
    <xsl:when test="umbraco.library:GetXmlNodeById(umbraco.library:Request('id'))/openForCollab = '1'">
      
      <p>The project is currently marked as 'open for collab'.
      <a href="#" rel="{umbraco.library:Request('id')}" id="closeCollab" class="remove">Remove</a>
      </p>
    </xsl:when>
    <xsl:otherwise>
      
        <p>Let people know they can contribute to the project.
       <a href="#" rel="{umbraco.library:Request('id')}" id="openCollab">Mark your project as 'open for collab'</a>.
       </p>
    </xsl:otherwise>
  </xsl:choose>
  <p id="confirmMessage" style="display:none">Collaboration status changed</p>
  
  <xsl:variable name="contri" select="our.library:ProjectContributors(umbraco.library:Request('id'))" />
  
  <xsl:if test="count($contri//memberId) &gt; 0">
  <h2>Current contributors</h2>
  <ul>
    <xsl:for-each select="$contri//memberId">
      <xsl:sort select="umbraco.library:GetMemberName(.)" />
    <li>
      <a href="/member/{.}"><xsl:value-of select="umbraco.library:GetMemberName(.)"/></a> - <a rel="{.}" class="removeContributor remove" href="#" >Remove</a>
    </li>
    </xsl:for-each>
  </ul>
  </xsl:if>

  
  <script type="text/javascript">
    function ChangeCollabStatus(projectid,status)
    {
    $.get("/umbraco/api/Community/ChangeCollabStatus/?projectId=" + projectid + "&amp;status=" + status);
    }
    
    $(document).ready(function() {
      $("#openCollab").click(function() {
        
          ChangeCollabStatus($(this).attr("rel"),true);
          $(this).parent().hide('fast');
           $("#confirmMessage").show('fast');
      });
    
      $("#closeCollab").click(function() {
        
          ChangeCollabStatus($(this).attr("rel"),false);
          $(this).parent().hide('fast');
          $("#confirmMessage").show('fast');
      });
    });
  </script>
</xsl:if>

</xsl:template>

</xsl:stylesheet>