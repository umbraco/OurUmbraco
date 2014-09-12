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
        <xsl:variable name="mem" select="uForum:GetCurrentMember()"/>
        <xsl:variable name="memberTopics" select="uForum.raw:TopicsWithParticipation($mem/@id)"/>

        <div style="overflow: hidden; width: 480px; float: left; clear: none;">
            <div class="box">
                <h4><a style="display: block; float: right;" href="/rss/memberparticipation/{$mem/@id}"><img style="border: 0;" src="/css/img/rss.png" /></a>Active topics you are participating in </h4>
                <ul class="summary">
                    <xsl:choose>
                        <xsl:when test="count($memberTopics) &gt; 0">
                            <xsl:for-each select="$memberTopics//topic">
                                <li>
                                    <xsl:if test="answer &gt; 0">
                                        <img src="/css/img/icons/tick.png" alt="The topic has been solved" title="The topic has been solved" style="float: right; width: 16px; height: 16px;" />
                                    </xsl:if>
                                    <xsl:choose>
                                        <xsl:when test="replies &gt; 0">
                                            <a href="{uForum:NiceCommentUrl(id, latestComment, 10)}">
                                                RE: <xsl:value-of select="title" />
                                            </a>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <a href="{uForum:NiceTopicUrl(id)}">
                                                <xsl:value-of select="title" />
                                            </a>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                    <small>
                                        <xsl:value-of select="uForum:TimeDiff(updated)"/>.
                                        Posted in <xsl:value-of select="umbraco.library:GetXmlNodeById(parentId)/@nodeName" />
                                        by
                                        <xsl:choose>
                                            <xsl:when test="latestReplyAuthor = $mem/@id">you</xsl:when>
                                            <xsl:otherwise>
                                                <strong><xsl:value-of select="umbraco.library:GetMemberName(latestReplyAuthor)"/></strong>.
                                                <a href="{uForum:NiceCommentUrl(id, latestComment, 10)}" style="display: inline;">
                                                    Reply
                                                </a>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </small>
                                </li>
                            </xsl:for-each>
                        </xsl:when>
                        <xsl:otherwise>
                            <li>
                                It seems you are not participating in any topics.
                            </li>
                        </xsl:otherwise>
                    </xsl:choose>
                </ul>
            </div>
        </div>
    </xsl:template>

</xsl:stylesheet>