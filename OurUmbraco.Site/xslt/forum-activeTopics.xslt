<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">

	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:template match="/">
		<div id="forum">
			<table cellspacing="0" class="forumList">
				<thead>
					<tr>
						<th>Topic</th>
						<th class="replies">Replies</th>
						<th>Last post</th>
					</tr>
				</thead>
				<tbody>

					<xsl:for-each select ="uForum.raw:LatestTopics(50, 0)/topics/topic">
						<xsl:sort select ="updated" order ="descending"/>
						<tr id="topic{@id}">
							<xsl:if test="answer != 0">
								<xsl:attribute name="class">solved</xsl:attribute>
							</xsl:if>
							<th class="title">
								<a href="{uForum:NiceTopicUrl(id)}">
									<xsl:value-of select="title"/>
								</a>
								<small>
									Started by: <xsl:value-of select="umbraco.library:GetMemberName(memberId)" />
									- <span title="{umbraco.library:FormatDateTime(created, 'MMMM d, yyyy @ hh:mm')}">
                  <xsl:value-of select="uForum:TimeDiff(created)"/>
                </span>
									<xsl:if test="answer != 0">
										<em>&nbsp; - Topic has been solved</em>
									</xsl:if>
									<xsl:variable name="forum" select="umbraco.library:GetXmlNodeById(parentId)"/>
									<a class="forumLink" rel="forum" href="{umbraco.library:NiceUrl(parentId)}">Posted in forum: <strong><xsl:value-of select="$forum/@nodeName"/></strong></a>
								</small>
							</th>
							<td class="replies">
								<xsl:value-of select ="replies"/>
							</td>
							<td class="latestComment">
								<a href="{uForum:NiceTopicUrl(id)}" title="{umbraco.library:FormatDateTime(updated, 'MMMM d, yyyy @ hh:mm')}">
									<xsl:value-of select="uForum:TimeDiff(updated)"/>
								</a> by

								<xsl:choose>
									<xsl:when test="number(latestReplyAuthor) &gt; 0">
										<xsl:value-of select="umbraco.library:GetMemberName(latestReplyAuthor)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="umbraco.library:GetMemberName(memberId)"/>
									</xsl:otherwise>
								</xsl:choose>
							</td>
						</tr>
					</xsl:for-each>
				</tbody>
			</table>

		</div>
	</xsl:template>

</xsl:stylesheet>