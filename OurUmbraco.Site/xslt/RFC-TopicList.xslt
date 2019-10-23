<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications ">


  <xsl:output method="html" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>

  <xsl:variable name="mem">
    <xsl:if test="umbraco.library:IsLoggedOn()">
      <xsl:value-of select="uForum:GetCurrentMember()/@id"/>
    </xsl:if>
  </xsl:variable>

  <xsl:variable name="forumNode" select="$currentPage/* [@isDoc][1]"/>
  <xsl:variable name="forumid" select="$forumNode/@id" />

  <xsl:template match="/">
    <xsl:choose>
    <xsl:when test="number($forumNode/forumAllowNewTopics) = 1">
      <xsl:variable name="treshold">
        <xsl:choose>
          <xsl:when test="umbraco.library:IsLoggedOn()">
            <xsl:value-of select="uForum:GetCurrentMember()/treshold" />
          </xsl:when>
          <xsl:otherwise>-10</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="p">
        <xsl:choose>
          <xsl:when test="string(number( umbraco.library:RequestQueryString('p') )) != 'NaN'">
            <xsl:value-of select="umbraco.library:RequestQueryString('p')"/>
          </xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Register RSS -->
      <xsl:value-of select="uForum:RegisterRssFeed( concat('https://our.umbraco.com/rss/forum?id=',$forumNode/@id), concat('New topics from the ',$forumNode/@nodeName ,' forum'), 'topicRss')"/>

      <xsl:variable name="items">15</xsl:variable>
      <xsl:variable name="pages" select="uForum:ForumPager($forumNode/@id, $items, $p)"/>

      <xsl:variable name="topics" select="uForum.raw:Topics($forumNode/@id, $items, $p)//topic"/>

      <xsl:choose>
      <xsl:when test="umbraco.library:IsLoggedOn()">
        <div class="rfcOptions">
              <a class="act addRfc" href="{umbraco.library:NiceUrl($forumNode/@id)}/NewTopic">Create a proposal</a> 
        <xsl:variable name="subscribed" select="Notifications:IsSubscribedToForum($forumid,$mem)" />


        <a href="#" class="act subscribe UnSubscribeForum" title="Unsubscribe from this forum" rel="{$forumid}" style="margin-left:15px">
          <xsl:if test="not($subscribed)">
            <xsl:attribute name="style">
              <xsl:text>display:none;</xsl:text>
            </xsl:attribute>
          </xsl:if>
          Unsubscribe
        </a>

        <a href="#" class="act subscribe SubscribeForum" title="Subscribe to this forum" rel="{$forumid}" style="margin-left:15px">
          <xsl:if test="$subscribed">
            <xsl:attribute name="style">
              <xsl:text>display:none;</xsl:text>
            </xsl:attribute>
          </xsl:if>
          Subscribe
        </a>
        </div>
      </xsl:when>
        <xsl:otherwise>
          <p>Login to comment</p>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:when>
      <xsl:otherwise>
        <p>Proposals are now closed for this release.</p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="topic">
    <xsl:param name="topic"/>

    <div id="topic{$topic/id}" class="rfcTopic">
      <xsl:if test="$topic/answer != 0">
        <xsl:attribute name="class">rfcTopic solved</xsl:attribute>
      </xsl:if>

      <strong>
        <a href="{uForum:NiceTopicUrl($topic/id)}">
          <xsl:value-of select="$topic/title"/>
        </a>
      </strong>
      <br/>
      <small>
        Started by:  <xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/>
        - <xsl:value-of select="uForum:TimeDiff($topic/created)"/>
        <xsl:if test="$topic/answer != 0">
          <br/><em>Proposal closed</em>
        </xsl:if>
      </small>
      <br/>

      <xsl:value-of select="$topic/replies"/>
        <small>&nbsp;replies</small>&nbsp;

        <xsl:value-of select="$topic/score"/>
        <small>&nbsp;votes</small>

      <!-- div class="latestComment">

        <xsl:choose>
          <xsl:when test="number($topic/latestComment) &gt; 0">
            <a href="{uForum:NiceCommentUrl($topic/id, $topic/latestComment, 10 )}">
              <xsl:value-of select="uForum:TimeDiff($topic/updated)"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <a href="{uForum:NiceTopicUrl($topic/id)}">
              <xsl:value-of select="uForum:TimeDiff($topic/updated)"/>
            </a>
          </xsl:otherwise>
        </xsl:choose>

        by

        <xsl:choose>
          <xsl:when test="number($topic/latestReplyAuthor) &gt; 0">
            <xsl:value-of select="umbraco.library:GetMemberName($topic/latestReplyAuthor)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/>
          </xsl:otherwise>
        </xsl:choose>
      </div -->
    </div>

  </xsl:template>



</xsl:stylesheet>