<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>

<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:rssdatehelper="urn:rssdatehelper"
  xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:content="http://purl.org/rss/1.0/modules/content/"
  xmlns:georss="http://www.georss.org/georss/"
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">

  <xsl:output method="xml" omit-xml-declaration="yes"/>


  <xsl:param name="currentPage"/>
 
  <xsl:variable name="f" select="uForum:Forum( /macro/forum , false() )"/>
  <xsl:variable name="ts" select="uForum.raw:Topics( /macro/forum , 10, 0)"/>  

  <!-- Update these variables to modify the feed -->
  <xsl:variable name="RSSNoItems" select="string('10')"/>
  <xsl:variable name="RSSTitle" select="$f/title"/>
  <xsl:variable name="SiteURL" select="string('https://our.umbraco.com')"/>
  <xsl:variable name="RSSDescription" select="concat(string('Latest topics in the forum ') , $RSSTitle)"/>

  <!-- This gets all news and events and orders by updateDate to use for the pubDate in RSS feed -->
  <xsl:variable name="pubDate" select="$f/@latestPostDate"/>

  <xsl:template match="/">
    <!-- change the mimetype for the current page to xml -->
    <xsl:value-of select="umbraco.library:ChangeContentType('text/xml')"/>

    <xsl:text disable-output-escaping="yes">&lt;?xml version="1.0" encoding="UTF-8"?&gt;</xsl:text>
    <rss version="2.0"
    xmlns:content="http://purl.org/rss/1.0/modules/content/"
    xmlns:wfw="http://wellformedweb.org/CommentAPI/"
    xmlns:dc="http://purl.org/dc/elements/1.1/"
    xmlns:georss="http://www.georss.org/georss/">

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
        
  <xsl:apply-templates select="$ts/topics/topic">
          <xsl:sort select="created" order="descending" />
        </xsl:apply-templates>
      </channel>
    </rss>

  </xsl:template>

  <xsl:template match="topic">
    <xsl:if test="position() &lt;= $RSSNoItems">

      <xsl:variable name="mem" select="umbraco.library:GetMember(memberId)"/>
      <xsl:variable name="url" select="concat($SiteURL, uForum:NiceTopicUrl(id))" />
       
      <item>
    <title><xsl:value-of select="title"/></title>
    <link><xsl:value-of select="$url"/></link>

    <content:encoded>
          <xsl:value-of select="concat('&lt;![CDATA[ ', body,']]&gt;')" disable-output-escaping="yes"/>
        </content:encoded>

    <dc:creator><xsl:value-of select="$mem/@nodeName"/></dc:creator>
    <dc:date> <xsl:value-of select="umbraco.library:FormatDateTime(created,'r')" /> </dc:date>
    <georss:point><xsl:value-of select="$mem/latitude"/><xsl:text> </xsl:text><xsl:value-of select="$mem/longitude"/></georss:point>

      </item>


    </xsl:if>
  </xsl:template>

</xsl:stylesheet>