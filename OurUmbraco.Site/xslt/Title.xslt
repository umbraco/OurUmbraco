<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library"
	exclude-result-prefixes="msxml umbraco.library">
	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:variable name="topic" select="umbraco.library:ContextKey('topicTitle')"/>

	<xsl:template match="/">
		<xsl:if test="normalize-space($topic)">
			<xsl:value-of select="$topic"/>
			<xsl:text> - </xsl:text>
		</xsl:if>
		<xsl:value-of select="$currentPage/@nodeName"/>
		<xsl:text> - our.umbraco.com</xsl:text>
	</xsl:template>

</xsl:stylesheet>