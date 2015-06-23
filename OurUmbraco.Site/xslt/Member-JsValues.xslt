<?xml version="1.0" encoding="UTF-8"?>
<!--
	Member-JsValues.xslt

	Renders the currently logged in Member's id, name, email and avatar
	as JavaScript variables for use in scripts on the page.
-->
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library"
	xmlns:uForum="urn:uForum"
	exclude-result-prefixes="msxml umbraco.library uForum"
>

	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>
	
	<xsl:variable name="isLoggedOn" select="umbraco.library:IsLoggedOn()"/>

	<xsl:template match="/">
		<xsl:if test="$isLoggedOn">
		<xsl:variable name="member" select="uForum:GetCurrentMember()"/>
			<script type="text/javascript">
				var umb_member_guid = '<xsl:value-of select="$member/@id"/>';
				var umb_member_name = '<xsl:value-of select="$member/@nodeName"/>';
				var umb_member_email = '<xsl:value-of select="$member/@email"/>';
				var umb_member_icon = '<xsl:value-of select="$member/avatar"/>';
			</script>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>