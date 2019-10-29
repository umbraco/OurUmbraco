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
  <xsl:variable name="forumid" select="$currentPage/@id" />

  <xsl:template match="/">

    
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
      <xsl:value-of select="uForum:RegisterRssFeed( concat('https://our.umbraco.com/rss/forum?id=',$currentPage/@id), concat('New topics from the ',$currentPage/@nodeName ,' forum'), 'topicRss')"/>

      <xsl:variable name="items">15</xsl:variable>
      <xsl:variable name="pages" select="uForum:ForumPager($currentPage/@id, $items, $p)"/>
      <xsl:variable name="topics" select="uForum.raw:Topics($currentPage/@id, $items, $p)//topic"/>


      <xsl:if test="umbraco.library:IsLoggedOn()">
        <div id="options">
          <ul>
            <xsl:variable name="subscribed" select="Notifications:IsSubscribedToForum($forumid,$mem)" />
            <li>
              <a href="#" class="act subscribe UnSubscribeForum" title="Unsubscribe from this forum" rel="{$forumid}">
                <xsl:if test="not($subscribed)">
                  <xsl:attribute name="style">
                    <xsl:text>display:none;</xsl:text>
                  </xsl:attribute>
                </xsl:if>
                Unsubscribe
              </a>

              <a href="#" class="act subscribe SubscribeForum" title="Subscribe to this forum" rel="{$forumid}">
                <xsl:if test="$subscribed">
                  <xsl:attribute name="style">
                    <xsl:text>display:none;</xsl:text>
                  </xsl:attribute>
                </xsl:if>
                Subscribe
              </a>
            </li>
            <xsl:if test="number($currentPage/forumAllowNewTopics) = 1">
              <li class="right">
                <a class="act add" href="{umbraco.library:NiceUrl($currentPage/@id)}/NewTopic" style="font-weight: bold;">Create a new topic</a>
              </li>
            </xsl:if>
          </ul>
        </div>
      </xsl:if>

      
      <xsl:if test="count($topics) &gt; 0">
        <table class="forumList" cellspacing="0">
          <tbody>

            <xsl:for-each select="$topics">
              <xsl:sort select="updated" order="descending" />

              <xsl:if test="score &gt;= $treshold">
                <xsl:call-template name="topic">
                  <xsl:with-param name="collaps" select="false()" />
                  <xsl:with-param name="topic" select="." />
                </xsl:call-template>
              </xsl:if>

            </xsl:for-each>
          </tbody>
        </table>

        <xsl:if test="$pages[page]">
          <ul class="pager">
            <xsl:for-each select="$pages/page">
              <li>
                <xsl:if test="@current = 'true'">
                  <xsl:attribute name="class">current</xsl:attribute>
                </xsl:if>
                <a href="?p={@index}">
                  <xsl:value-of select="@index + 1"/>
                </a>
              </li>
            </xsl:for-each>
          </ul>
        </xsl:if>

      </xsl:if>

  </xsl:template>

  <xsl:template name="collapsedTopic">
    <xsl:param name="topic"/>

    <tr class="toggleTopic" id="collapsedtopic{$topic/id}">
      <td colspan="4">
        The topic <em>
          <xsl:value-of select="$topic/title"/>
        </em> by <em>
          <xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/>
        </em>
        has a very low score and been hidden, <a class="forumToggleTopic" rel="topic{$topic/id}" href="#">Show it anyway</a>
      </td>
    </tr>

    <xsl:call-template name="topic">
      <xsl:with-param name="collaps" select="true()" />
      <xsl:with-param name="topic" select="$topic" />
    </xsl:call-template>

  </xsl:template>

  <xsl:template name="topic">
    <xsl:param name="collaps"/>
    <xsl:param name="topic"/>

    <tr id="topic{$topic/id}">
      <xsl:if test="$collaps">
        <xsl:attribute name="class">hidden</xsl:attribute>
      </xsl:if>
      <xsl:if test="not($collaps) and $topic/answer != 0">
        <xsl:attribute name="class">solved</xsl:attribute>
      </xsl:if>

      <th class="title">

        <a href="{uForum:NiceTopicUrl($topic/id)}">
          <xsl:value-of select="$topic/title"/>
        </a>
        <small>
          Started by:  <xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/>
   - <span title="{umbraco.library:FormatDateTime($topic/created, 'MMMM d, yyyy @ hh:mm')}">
  <xsl:value-of select="uForum:TimeDiff($topic/created)"/>
</span>
              <xsl:if test="$topic/answer != 0"><em>&nbsp; - Topic has been solved</em></xsl:if>  

        </small>
      </th>

      <td class="replies">
        <xsl:if test="$topic/replies &gt; 0">
          <xsl:attribute name="class">replies replied</xsl:attribute>
        </xsl:if>

        <xsl:value-of select="$topic/replies"/>
        <small>replies</small>
      </td>

      <td class="votes">
        <xsl:if test="$topic/score &gt; 0">
          <xsl:attribute name="class">votes voted</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="$topic/score"/>
        <small>votes</small>
      </td>

      <td class="latestComment">

        <xsl:choose>
          <xsl:when test="number($topic/latestComment) &gt; 0">
 <a href="{uForum:NiceCommentUrl($topic/id, $topic/latestComment, 10 )}" title="{umbraco.library:FormatDateTime($topic/updated, 'MMMM d, yyyy @ hh:mm')}">
              <xsl:value-of select="uForum:TimeDiff($topic/updated)"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
 <a href="{uForum:NiceTopicUrl($topic/id)}" title="{umbraco.library:FormatDateTime($topic/updated, 'MMMM d, yyyy @ hh:mm')}">
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
      </td>
    </tr>

  </xsl:template>



</xsl:stylesheet>