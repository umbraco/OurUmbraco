<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library"
	xmlns:uEvents="urn:uEvents"
	exclude-result-prefixes="msxml umbraco.library uEvents">
	<xsl:output method="xml" omit-xml-declaration="yes"/>

	<xsl:param name="currentPage"/>

	<xsl:template match="/">
		<xsl:apply-templates select="$currentPage" />
	</xsl:template>

	<xsl:template match="Events">
		<div id="googlemap" style="width:978px;height:350px;">Loading map, please wait...</div>
		<div class="box">
			<p>
				<table class="list" id="eventList" style="width: 100%;">
					<thead>
						<tr>
							<th>Name</th>
							<th>Location</th>
							<th>Time</th>
						</tr>
					</thead>
					<tbody>
						<xsl:apply-templates select="Event[@isDoc and normalize-space(start) and umbraco.library:DateGreaterThanOrEqualToday(start)]">
							<xsl:sort select="start" order="ascending" data-type="text" />
						</xsl:apply-templates>
					</tbody>
				</table>
			</p>
		</div>
	</xsl:template>

	<xsl:template match="Event">
		<tr id="event{@id}" rel="{latitude},{longitude}">
			<td class="name">
				<a href="{umbraco.library:NiceUrl(@id)}">
					<xsl:value-of select="@nodeName" />
				</a>
			</td>
			<td class="location">
				<xsl:value-of select="venue" />
			</td>
			<td class="date">
				<xsl:value-of select="umbraco.library:LongDate(start, 1, ' ')" />
			</td>
		</tr>
	</xsl:template>

</xsl:stylesheet>