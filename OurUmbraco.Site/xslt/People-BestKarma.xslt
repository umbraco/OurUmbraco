<?xml version="1.0" encoding="UTF-8"?>
<!--
	People-BestKarma.xslt
	
	Renders the 25 most top rated people within a given number of weeks.
-->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator"
>

	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:template match="/">
		<xsl:variable name="weeks" select="/macro/weeks"/>
		<xsl:variable name="timespan" select="/macro/timespan"/>
		<xsl:variable name="people" select="uPowers:MemberKarma(number($weeks), 25)"/>

		<ul class="forumTopics summary">
			<xsl:for-each select="$people//score">
				<xsl:sort select="totalPoints" order="descending" data-type="number"/>
				<li>
					<img src="/media/avatar/{memberId}.jpg" alt="Topic author image"/>
					<a href="/member/{memberId}">
						<xsl:value-of select="umbraco.library:GetMemberName(memberId)"/>
					</a>
					<small>
						<xsl:text>Received </xsl:text>
						<em><xsl:value-of select="totalPoints" /></em>
						<xsl:text>karma points the last </xsl:text>
						<xsl:value-of select="$timespan"/>
					</small>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>

</xsl:stylesheet>