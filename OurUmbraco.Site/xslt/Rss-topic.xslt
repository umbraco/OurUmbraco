<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:rssdatehelper="urn:rssdatehelper"
  xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:content="http://purl.org/rss/1.0/modules/content/"
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">

  <xsl:output method="xml" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>
 
  <xsl:variable name="t" select="uForum:Topic(/macro/topic)"/>
  <xsl:variable name="cs" select="uForum.raw:CommentsByDate(/macro/topic, 1000, 0, 'ASC')"/>  

  <!-- Update these variables to modify the feed -->
  <xsl:variable name="RSSNoItems" select="string('10')"/>
  <xsl:variable name="RSSTitle" select="$t/topic/title"/>
  <xsl:variable name="SiteURL" select="string('https://our.umbraco.com')"/>
  <xsl:variable name="RSSDescription" select="concat(string('Latest replies on the topic ') , $RSSTitle)"/>

  <!-- This gets all news and events and orders by updateDate to use for the pubDate in RSS feed -->
  <xsl:variable name="pubDate">
    <xsl:for-each select="$cs/comments/comment">
      <xsl:sort select="created" data-type="text" order="descending" />
      <xsl:if test="position() = 1">
        <xsl:value-of select="created" />
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <xsl:template match="/">
    <!-- change the mimetype for the current page to xml -->
    <xsl:value-of select="umbraco.library:ChangeContentType('text/xml')"/>

    <xsl:text disable-output-escaping="yes">&lt;?xml version="1.0" encoding="UTF-8"?&gt;</xsl:text>
    <rss version="2.0"
    xmlns:content="http://purl.org/rss/1.0/modules/content/"
    xmlns:wfw="http://wellformedweb.org/CommentAPI/"
    xmlns:dc="http://purl.org/dc/elements/1.1/">

      <channel>
        <title>
          <xsl:value-of select="$RSSTitle"/>
        </title>
        <link>
          <xsl:value-of select="$SiteURL"/>
        </link>
        <pubDate>
          <xsl:value-of select="$pubDate"/>
        </pubDate>
        <generator>umbraco</generator>
        <description>
          <xsl:copy-of select="$RSSDescription"/>
        </description>
        <language>en</language>

        <xsl:apply-templates select="$cs/comments/comment">
            <xsl:sort select="created" order="descending" />
        </xsl:apply-templates>
    
  </channel>
    </rss>

  </xsl:template>

  <xsl:template match="comment">
    <xsl:if test="position() &lt;= $RSSNoItems and ./isSpam != 'true'">
      <item>
        <title>
          Reply from: <xsl:value-of select="umbraco.library:GetMemberName(memberId)"/>
        </title>
        <link>
          <xsl:value-of select="$SiteURL"/><xsl:value-of select="uForum:NiceCommentUrl(topicId, id, 15)"/>
        </link>
        <pubDate>
          <xsl:value-of select="umbraco.library:FormatDateTime(created,'r')" />
        </pubDate>
        <guid>
          <xsl:value-of select="$SiteURL"/> <xsl:value-of select="uForum:NiceCommentUrl(topicId, id, 15)"/>
        </guid>
        <content:encoded>
          <xsl:value-of select="concat('&lt;![CDATA[ ', body,']]&gt;')" disable-output-escaping="yes"/>
        </content:encoded>
      </item>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>