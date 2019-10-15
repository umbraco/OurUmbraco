<?xml version="1.0" encoding="utf-8"?>
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
 
  <xsl:variable name="section" select="umbraco.library:RequestQueryString('section')"/>
  <xsl:variable name="editor" select="umbraco.library:RequestQueryString('editor')"/>
  <xsl:variable name="usertype" select="umbraco.library:RequestQueryString('userType')"/>	  
  

  <!-- Update these variables to modify the feed -->
  <xsl:variable name="RSSNoItems" select="string('10')"/>
  <xsl:variable name="RSSTitle" select="$section"/>
  <xsl:variable name="SiteURL" select="string('https://our.umbraco.com')"/>
  <xsl:variable name="RSSDescription" select="concat(string('Recommended help for the section') , $RSSTitle)"/>

  
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
        <generator>umbraco</generator>
        <description>
          <xsl:copy-of select="$RSSDescription"/>
        </description>
        <language>en</language>

		<xsl:choose>
          <xsl:when test="$section = 'forms'">
            <item>
				<title>Creating a form</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/products/UmbracoForms/Editor/Creating-a-form/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/products/UmbracoForms/Editor/Creating-a-form/
				</guid>
				<description>This will show the basic steps of creating forms and adding them to your Umbraco site.</description>
			</item>
			
			<item>
				<title>Using workflows</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/products/umbracoforms/editor/Attaching-Workflows/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/products/umbracoforms/editor/Attaching-Workflows/
				</guid>
				<description>If you wish to attach extra functionality to your form you can do so by assigning 1 or multiple workflows (like sending an email).</description>
			</item>
			
			<item>
				<title>Viewing entries</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/products/umbracoforms/editor/Viewing-and-Exporting-Entries/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/products/umbracoforms/editor/Viewing-and-Exporting-Entries/
				</guid>
				<description>The entries viewer for each form can be found when expanding the form in the tree</description>
			</item>
			  
          </xsl:when>
          <xsl:otherwise>
			<item>
				<title>Backoffice overview</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/Using-Umbraco/Backoffice-Overview/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/Using-Umbraco/Backoffice-Overview/
				</guid>
				<description>Overview of sections and terminology</description>
			</item>
		  
			<item>
				<title>Editors UI forums</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/forum/using/ui-questions
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/forum/using/ui-questions
				</guid>
				<description>Ask and search for previous answers</description>
      		</item>
		  
		  	<item>
				<title>Extending Umbraco</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/Extending-Umbraco/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/Extending-Umbraco/
				</guid>
				<description>See where you can customize Umbraco</description>
      		</item>
		  
		  	<item>
				<title>Developers Reference</title>
				<link>
					<xsl:value-of select="$SiteURL"/>/documentation/Reference/
				</link>
				<guid>
					<xsl:value-of select="$SiteURL"/>/documentation/Reference/
				</guid>
				<description>API reference for developers</description>
      		</item>  
			  
          </xsl:otherwise>
        </xsl:choose>
		  
    	
		  
  </channel>
    </rss>

  </xsl:template>

  <xsl:template match="comment">
    <xsl:if test="position() &lt;= $RSSNoItems">
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
