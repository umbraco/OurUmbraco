<?xml version="1.0" encoding="UTF-8"?>
<!--
	member-notifications.xslt
	
	Renders a list of forums and topics the member has subscribed to
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications"
>

	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>
	
	<xsl:variable name="isLoggedOn" select="umbraco.library:IsLoggedOn()" />
	<xsl:variable name="member" select="umbraco.library:GetCurrentMember()" />

	<xsl:template match="/">
		<xsl:choose>
			<xsl:when test="$isLoggedOn and $member[bugMeNot = 1]">
				<div class="alert" style="width:600px">
					Currently you will not receive any notifications since your profile preference is set
					to 'Do not send me any notifications or newsletters from our.umbraco.org'.
					To change this, please update the setting on your profile.
				</div>
			</xsl:when>
			<xsl:otherwise>
				<h2>Forum subscriptions</h2>
				<small>You get notified whenever a new topic is added to the subscribed forums.</small>
				
				<xsl:variable name="forumsubscriptions" select="Notifications:SubscribedForums($member/@id)" />
				<xsl:choose>
					<xsl:when test="not($forumsubscriptions//forum)">
						<p>Currently no active subscriptions.</p>
					</xsl:when>
					<xsl:otherwise>
						<ul>
							<xsl:apply-templates select="$forumsubscriptions//forum" />
						</ul>
					</xsl:otherwise>
				</xsl:choose>

				<h2>Topic subscriptions</h2>
				<small>You get notified whenever a new reply is added to the subscribed topics.</small>

				<xsl:variable name="topicsubscriptions" select="Notifications:SubscribedTopics($member/@id)//topic" />
				<xsl:choose>
					<xsl:when test="not($topicsubscriptions)">
						<p>Currently no active subscriptions.</p>
					</xsl:when>
					<!-- Show absolutely everything if the pagerParam is "all" (e.g.: p=all) -->
					<xsl:when test="translate($options[@key = $pagerParam], 'AL', 'al') = 'all'">
						<xsl:apply-templates select="$topicsubscriptions">
							<xsl:sort select="@updated" data-type="text" order="descending" />
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>
						<ul class="MemberTopicNotifications">
							<xsl:call-template name="PaginateSelection">
								<xsl:with-param name="selection" select="$topicsubscriptions" />
								<xsl:with-param name="perPage" select="50" />
								<xsl:with-param name="sortBy" select="'@updated DESC'" />
							</xsl:call-template>
						</ul>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:include href="_PaginationHelper.xslt" />
	
	<xsl:template match="forum">
		<li>
			<a href="{umbraco.library:NiceUrl(@id)}">
				<xsl:value-of select="title" />
			</a>
			<xsl:text> - </xsl:text>
			<a href="#" rel="{@id}" class="NotificationForumUnsubscribe">Unsubscribe</a>
		</li>
	</xsl:template>
	
	<xsl:template match="topic">
		<li>
			<xsl:if test="not(@answer = 0)"><xsl:attribute name="class">TopicSolved</xsl:attribute></xsl:if>
			<a href="{uForum:NiceTopicUrl(@id)}">
				<xsl:value-of select="title" />
			</a>
			<xsl:text> - </xsl:text>
			<a href="#" rel="{@id}" class="NotificationTopicUnsubscribe">Unsubscribe</a>
		</li>
	</xsl:template>

</xsl:stylesheet>