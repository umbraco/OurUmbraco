<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library"
  exclude-result-prefixes="msxml umbraco.library">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<xsl:if test="umbraco.library:IsLoggedOn()">
<xsl:variable name="mem" select="umbraco.library:GetCurrentMember()"/>

<script type="text/javascript">
var umb_member_guid = '<xsl:value-of select="$mem/@id"/>'; 
var umb_member_name = '<xsl:value-of select="$mem/@nodeName"/>'; 
var umb_member_email = '<xsl:value-of select="$mem/@email"/>'; 
var umb_member_icon = '<xsl:value-of select="$mem/avatar"/>'; 
</script>

</xsl:if>

</xsl:template>

</xsl:stylesheet>