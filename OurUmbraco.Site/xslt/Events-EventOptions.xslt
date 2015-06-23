<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">
<xsl:if test="not(umbraco.library:IsLoggedOn())">
  <div class="box">
    <h4>Wanna join?</h4>
    <p style="padding-left: 10px; font-size: 11px;">
      Please <a href="/member/login?redirectUrl={umbraco.library:NiceUrl($currentPage/@id)}">login</a> to signup for this event  
    </p>
  </div>
</xsl:if>

<xsl:if test="umbraco.library:IsLoggedOn()">
  <div class="box" style="text-align: center">
  <xsl:variable name="mem" select="uForum:GetCurrentMember()"/>
  
  <xsl:if test="true and number($mem/@id) = number($currentPage/owner)">
    <small>Notify event participants</small>
    <a href="/events/notify?id={$currentPage/@id}" class="signUpButton" rel="{$currentPage/@id}">Send notifications</a>          
  </xsl:if>
  
  <xsl:choose>
  <xsl:when test="uEvents:isSignedUp($currentPage/@id, $mem/@id)">
    <small>You are already signed up</small>
    <a href="{umbraco.library:NiceUrl($currentPage/@id)}" id='eventSignup' class="signUpButton eventcancel" rel="{$currentPage/@id}">I have to cancel</a>  
  </xsl:when>
  <xsl:otherwise>

  <xsl:choose>

  <xsl:when test="uEvents:isFull($currentPage/@id)">
    <small>All seats are taken!</small>
    <a href="{umbraco.library:NiceUrl($currentPage/@id)}" id='eventSignup' class="signUpButton eventwaiting" rel="{$currentPage/@id}">Put me on the waitinglist!</a>
  </xsl:when>
  <xsl:otherwise>
    <a href="{umbraco.library:NiceUrl($currentPage/@id)}" id='eventSignup' class="signUpButton eventsignup" rel="{$currentPage/@id}">Yes, I'll be there!</a>  
  </xsl:otherwise>
  </xsl:choose>
  
  </xsl:otherwise>
  </xsl:choose>


  <xsl:if test="$currentPage/owner = $mem/@id">
    <a href="/events/create?id={$currentPage/@id}" class="signUpButton eventedit">Edit event information</a>
  </xsl:if>
  </div>
</xsl:if>

</xsl:template>

</xsl:stylesheet>