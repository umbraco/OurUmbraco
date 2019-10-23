<?xml version="1.0" encoding="UTF-8"?>
<!--
	forum-overview.xslt
	
	Renders the front page of the forum with all the categories,
	or the front page of a category with all its topics.
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:uForum="urn:uForum"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets"
	exclude-result-prefixes="uForum msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets"
>

	<xsl:output method="html" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:variable name="forumFrontPageId" select="1053" />

	<xsl:variable name="isLoggedOn" select="umbraco.library:IsLoggedOn()"/>
	<xsl:variable name="currentMember" select="uForum:GetCurrentMember()"/>

	<xsl:template match="/">
		<xsl:if test="$isLoggedOn">
			<xsl:value-of select="uForum:RegisterRssFeed(concat('https://our.umbraco.com/rss/yourtopics?id=', $currentMember/@id), 'Your active topics', 'yourtopics')"/>
		</xsl:if>

		<xsl:choose>
			<xsl:when test="$currentPage/@id = $forumFrontPageId">
				<div id="options">
					<ul>
						<xsl:if test="$isLoggedOn">
							<li class="right">
								<a class="act yourtopics" href="/forum/your-topics">Topics you participated in</a>
							</li>
						</xsl:if>
						<li class="right">
							<a class="act topics" href="/forum/active-topics">Active topics</a>
						</li>
					</ul>
				</div>
				<xsl:call-template name="categories"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="forums">  
					<xsl:with-param name="parent" select="uForum:Forums($currentPage/@id, true())"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="categories">
		<xsl:for-each select="$currentPage/*[@isDoc][not(umbracoNaviHide = 1)]">
			<xsl:if test="*[@isDoc]">
				<h2><xsl:value-of select="@nodeName"/></h2>
				<xsl:call-template name="forums">  
					<xsl:with-param name="parent" select="uForum:Forums(@id, true())"/>
				</xsl:call-template>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="forums">
		<xsl:param name="parent"/>
		<xsl:if test="$parent/forum">
			<table class="forumList" cellspacing="0">
				<tbody>
					<xsl:for-each select="$parent/forum">
						<xsl:sort select="@SortOrder" />
						<tr>
							<th>
								<a href="{umbraco.library:NiceUrl(@id)}">
									<xsl:value-of select="title"/>
								</a>
								<div class="forumStats">
									<xsl:value-of select="concat(@TotalTopics, ' topics, ', (@TotalTopics + @TotalComments), ' posts')" />
								</div>
								<div class="forumDesc">
									<xsl:value-of select="description"/>
								</div>
							</th>
							<td class="forumLastPost">
								<xsl:if test="@LatestAuthor &gt; 0 and @LatestTopic &gt; 0">
									<a href="{uForum:NiceTopicUrl(@LatestTopic)}" title="{umbraco.library:FormatDateTime(@LatestPostDate, 'MMMM d, yyyy @ hh:mm')}">
										<xsl:value-of select="uForum:TimeDiff(@LatestPostDate)"/>
									</a>
									<xsl:text> by </xsl:text>
									<xsl:value-of select="umbraco.library:GetMemberName(@LatestAuthor)"/>
								</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</tbody>
			</table>
		</xsl:if>
	</xsl:template>
	
</xsl:stylesheet>