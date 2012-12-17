<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>


<xsl:variable name="attendees" select="
		umbraco.library:GetRelatedNodesAsXml(6801) | 
		umbraco.library:GetRelatedNodesAsXml(6795) | 
		umbraco.library:GetRelatedNodesAsXml(6805) | 
		umbraco.library:GetRelatedNodesAsXml(6806) |
		umbraco.library:GetRelatedNodesAsXml(6807) |
		umbraco.library:GetRelatedNodesAsXml(6822) |
		umbraco.library:GetRelatedNodesAsXml(6824) |
		umbraco.library:GetRelatedNodesAsXml(6825) | 
		umbraco.library:GetRelatedNodesAsXml(7254)"/>

<xsl:variable name="attending" select="$attendees//relation [@typeName = 'event']"/>

<xsl:template match="/"><xsl:value-of select="count($attending)"/></xsl:template>

</xsl:stylesheet>