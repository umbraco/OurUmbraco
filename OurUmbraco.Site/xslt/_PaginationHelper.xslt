<?xml version="1.0"?>
<?umbraco-package XSLT Helpers v0.9.1 - PaginationHelper v1.5?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:umb="urn:umbraco.library" xmlns:str="urn:Exslt.ExsltStrings" xmlns:make="urn:schemas-microsoft-com:xslt" version="1.0" exclude-result-prefixes="umb str make">

	<!-- Config constants -->
	<xsl:variable name="pagerParam" select="'p'"/><!-- Name of QueryString parameter for 'page' -->
	<xsl:variable name="perPage" select="10"/><!-- Default number of items on a page -->
	<xsl:variable name="prevPage" select="'&#x2039; Previous'"/>
	<xsl:variable name="nextPage" select="'Next &#x203A;'"/>
	<xsl:variable name="pageLinksBeside" select="'4'"/><!-- Number of pagination links to show before and after the current page -->
	
	<!--
		This is where we get the options for the page, which defaults to the QueryString
		but as long as it is formatted like a QueryString it can come from anywhere you like.
	-->
	<xsl:variable name="optionString" select="umb:RequestServerVariables('QUERY_STRING')"/>
	
	<!--
		We also need the base page's URL without QueryString params
	-->
	<xsl:variable name="pageURL" select="umb:NiceUrl($currentPage/@id)"/>
	
	<!--
		Build an `options` variable of all the query string params for easy lookup,
		e.g.: If you need to pass a search-string (q=xslt) along to all pages, it
		will be available as $options[@key = 'q']
	-->
	<xsl:variable name="optionsProxy">
		<xsl:call-template name="parseOptions">
			<xsl:with-param name="options" select="$optionString"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="options" select="make:node-set($optionsProxy)/options/option"/>

	<!-- Paging variables -->
	<xsl:variable name="reqPage" select="$options[@key = $pagerParam]"/>
	<xsl:variable name="page">
		<xsl:choose>
			<xsl:when test="number($reqPage) = $reqPage"><xsl:value-of select="$reqPage"/></xsl:when>
			<xsl:otherwise><xsl:value-of select="1"/></xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	
	<xsl:template name="PaginateSelection">
		<!-- The stuff to paginate - defaults to all children of the context node when invoking this  -->
		<xsl:param name="selection" select="*"/>
		
		<!-- This is to allow forcing a specific page without using QueryString  -->
		<xsl:param name="page" select="$page"/>
		
		<!-- This is the number of results you want per page -->
		<xsl:param name="perPage" select="$perPage"/>
		
		<!-- Specify which node() to sort by (as a string), e.g.: 'name', 'name DESC', '@updateDate ASC' etc. -->
		<xsl:param name="sortBy"/>
		
		<!-- Also, allow forcing specific options -->
		<xsl:param name="options" select="$options"/>

		<!-- You can disable the "Pager" control by setting this to false() - then manually calling RenderPager somewhere else -->
		<xsl:param name="showPager" select="true()"/>
		
		<!-- Specify how many links to show on each side of the "current" page in the Pager (if shown) -->
		<xsl:param name="pageLinksBeside" select="$pageLinksBeside"/>
		
		<xsl:variable name="startIndex" select="$perPage * ($page - 1) + 1"/><!-- First item on this page -->
		<xsl:variable name="endIndex" select="$page * $perPage"/><!-- First item on next page -->
		
		<xsl:choose>
			<!-- Do we need to pre-sort the selection? -->
			<xsl:when test="normalize-space($sortBy)">
				<xsl:variable name="sortedProxy">
					<xsl:call-template name="preSort">
						<xsl:with-param name="selection" select="$selection"/>
						<xsl:with-param name="sortBy" select="$sortBy"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="sortedSelection" select="make:node-set($sortedProxy)/nodes/nodeId"/>
				<xsl:apply-templates select="$selection[generate-id() = $sortedSelection[position() &gt;= $startIndex and position() &lt;= $endIndex]]">
					<xsl:sort select="count($sortedSelection[. = generate-id(current())]/preceding-sibling::nodeId)" data-type="number" order="ascending"/>
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<!-- Render the current page using apply-templates -->
				<xsl:apply-templates select="$selection[position() &gt;= $startIndex and position() &lt;= $endIndex]"/>
			</xsl:otherwise>
		</xsl:choose>
		
		<!-- Should we render the pager controls? -->
		<xsl:if test="$showPager">
			<xsl:call-template name="RenderPager">
				<xsl:with-param name="selection" select="$selection"/>
				<xsl:with-param name="page" select="$page"/>
				<xsl:with-param name="perPage" select="$perPage"/>
				<xsl:with-param name="pageLinksBeside" select="$pageLinksBeside"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>
	
	<xsl:template name="RenderPager">
		<xsl:param name="selection" select="*"/>
		<xsl:param name="page" select="$page"/>
		<xsl:param name="perPage" select="$perPage"/>
		<xsl:param name="pageLinksBeside" select="$pageLinksBeside"/>
		
		<xsl:variable name="total" select="count($selection)"/>
		<xsl:variable name="lastPageNum" select="ceiling($total div $perPage)"/>
		
		<xsl:variable name="needToRenderGaps" select="$lastPageNum &gt; 2 * $pageLinksBeside + 4"/>
		<xsl:variable name="pagerWidth" select="2 * $pageLinksBeside"/>

		<!-- Build the base query (i.e. the page's URL with any non-paging params) -->
		<xsl:variable name="query">
			<xsl:value-of select="$pageURL"/>
			<xsl:for-each select="$options[not(@key = $pagerParam)]">
				<xsl:if test="position() = 1">?</xsl:if>
				<xsl:if test="not(preceding-sibling::*[@key = current()/@key])">
					<xsl:value-of select="concat(@key, '=', .)"/>
					<xsl:if test="not(position() = last())">&amp;</xsl:if>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="sep" select="substring('&amp;|?', not($options[not(@key = $pagerParam)]) * 2 + 1, 1)"/>

		<ul class="pager">
			<!-- Create the "Previous" link -->
			<li class="prev">
				<xsl:choose>
					<xsl:when test="$page = 1">
						<xsl:attribute name="class">prev disabled</xsl:attribute>
						<xsl:value-of select="$prevPage"/>
					</xsl:when>
					<!-- Avoid duplicate content by not linking p=1 (issue #7) -->
					<xsl:when test="$page = 2">
						<a href="{$query}" rel="prev start"><xsl:value-of select="$prevPage"/></a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$query}{$sep}{$pagerParam}={$page - 1}" rel="prev"><xsl:value-of select="$prevPage"/></a>
					</xsl:otherwise>
				</xsl:choose>
			</li>

			<!-- Do we need to create page 1 & 2 + a "gap"? -->
			<xsl:if test="$needToRenderGaps and ($page - $pageLinksBeside &gt; 4)">
				<li><a href="{$query}" rel="start">1</a></li>
				<li><a href="{$query}{$sep}{$pagerParam}=2">2</a></li>
				<li class="gap">...</li>
			</xsl:if>

			<!-- Create links for each page available -->
			<xsl:for-each select="$selection[position() &lt;= $lastPageNum]">
				<xsl:choose>
					<xsl:when test="$page = position()">
						<li class="current">
							<xsl:value-of select="position()"/>
						</li>
					</xsl:when>
					<xsl:when test="not($needToRenderGaps)">
						<li>
							<a href="{$query}{$sep}{$pagerParam}={position()}">
								<!-- Avoid duplicate content by not linking p=1 (issue #7) -->
								<xsl:if test="position() = $page - 1"><xsl:attribute name="rel">prev</xsl:attribute></xsl:if>
								<xsl:if test="position() = $page + 1"><xsl:attribute name="rel">next</xsl:attribute></xsl:if>
								<xsl:if test="position() = 1">
									<xsl:attribute name="href"><xsl:value-of select="$query"/></xsl:attribute>
									<xsl:attribute name="rel">start</xsl:attribute>
									<xsl:if test="$page = 2">
										<xsl:attribute name="rel">prev start</xsl:attribute>
									</xsl:if>
								</xsl:if>
								<!-- Set rel=prev or rel=next if applicable -->
								<xsl:value-of select="position()"/>
							</a>
						</li>
					</xsl:when>
					<xsl:when test="$needToRenderGaps">
						<!-- If there are too many pages to show, figure out where to start -->
						<xsl:variable name="from">
							<xsl:choose>
								<xsl:when test="$page - $pageLinksBeside &lt;= 4">
									<xsl:value-of select="1"/>
								</xsl:when>
								<xsl:when test="$page &gt; ($lastPageNum - $pageLinksBeside)">
									<xsl:value-of select="$lastPageNum - $pagerWidth"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$page - $pageLinksBeside"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>

						<!-- Likewise, determine where to stop -->
						<xsl:variable name="to">
							<xsl:choose>
								<xsl:when test="$page + $pageLinksBeside &gt;= $lastPageNum - 3">
									<xsl:value-of select="$lastPageNum"/>
								</xsl:when>
								<xsl:when test="$page &lt;= $pageLinksBeside">
									<xsl:value-of select="$pagerWidth + 1"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$page + $pageLinksBeside"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						
						<!-- If we're inside that window, render the link -->
						<xsl:if test="position() &gt;= $from and position() &lt;= $to">
							<li>
								<a href="{$query}{$sep}{$pagerParam}={position()}">
									<!-- Avoid duplicate content by not linking p=1 (issue #7) -->
									<xsl:if test="position() = 1">
										<xsl:attribute name="href"><xsl:value-of select="$query"/></xsl:attribute>
										<xsl:attribute name="rel">start</xsl:attribute>
									</xsl:if>
									<!-- Set rel=prev or rel=next if applicable -->
									<xsl:if test="position() = $page - 1"><xsl:attribute name="rel">prev</xsl:attribute></xsl:if>
									<xsl:if test="position() = $page + 1"><xsl:attribute name="rel">next</xsl:attribute></xsl:if>
									<xsl:value-of select="position()"/><!-- <xsl:value-of select="concat(' (', $from, '——', $to, ')')" /> -->
								</a>
							</li>
						</xsl:if>
					</xsl:when>
				</xsl:choose>
			</xsl:for-each>
			
			<!-- Do we need to create a "gap" + page n-1 & n ? -->
			<xsl:if test="$needToRenderGaps and ($page + $pageLinksBeside &lt; $lastPageNum - 3)">
				<li class="gap">...</li>
				<li><a href="{$query}{$sep}{$pagerParam}={$lastPageNum - 1}"><xsl:value-of select="$lastPageNum - 1"/></a></li>
				<li><a href="{$query}{$sep}{$pagerParam}={$lastPageNum}"><xsl:value-of select="$lastPageNum"/></a></li>
			</xsl:if>
			
			<!-- Create the "Next" link -->
			<li class="next">
				<xsl:choose>
					<xsl:when test="$page = $lastPageNum">
						<xsl:attribute name="class">next disabled</xsl:attribute>
						<xsl:value-of select="$nextPage"/>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$query}{$sep}{$pagerParam}={$page + 1}" rel="next"><xsl:value-of select="$nextPage"/></a>
					</xsl:otherwise>
				</xsl:choose>
			</li>
			
			<!-- Create the "All" link -->
			<li class="all">
				<a href="{$query}{$sep}{$pagerParam}=all">
					<xsl:value-of select="concat('Show all ', $total, ' topics')" />
				</a>
			</li>
		</ul>
	</xsl:template>
	
	<!-- Options Parsing -->
	<xsl:template name="parseOptions">
		<xsl:param name="options" select="''"/>
		<options>
			<xsl:apply-templates select="str:split($options, '&amp;')" mode="parse"/>
		</options>
	</xsl:template>

	<xsl:template match="token" mode="parse">
		<xsl:variable name="key" select="substring-before(., '=')"/>
		<option key="{$key}">
			<xsl:value-of select="umb:RequestQueryString($key)"/>
		</option>
	</xsl:template>
	
	<!-- Pre-sorting -->
	<xsl:template name="preSort">
		<xsl:param name="selection" select="/.."/>
		<xsl:param name="sortBy"/>

		<nodes>
			<xsl:choose>
				<xsl:when test="$sortBy = '$CUSTOM'">
					<xsl:call-template name="customSort">
						<xsl:with-param name="selection" select="$selection"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="normalize-space($sortBy)">
					<xsl:variable name="sortNode">
						<xsl:value-of select="substring-before($sortBy, ' ')"/>
						<xsl:if test="not(contains($sortBy, ' '))">
							<xsl:value-of select="$sortBy"/>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="sortDirection">
						<xsl:value-of select="substring-after($sortBy, ' ')"/>
						<xsl:if test="not(contains($sortBy, ' '))">
							<xsl:value-of select="'ASC'"/>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="direction" select="translate(concat($sortDirection, 'ending'), 'ACDES', 'acdes')"/>
					<xsl:choose>
						<xsl:when test="starts-with($sortNode, '@')">
							<xsl:apply-templates select="$selection" mode="presort">
								<xsl:sort select="@*[name() = substring-after($sortNode, '@')]" data-type="text" order="{$direction}"/>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="$selection" mode="presort">
								<xsl:sort select="*[name() = $sortNode]" data-type="text" order="{$direction}"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</nodes>
	</xsl:template>
	
	<!--
		This is the template that gets called when you ask for custom sorting.
		It should just apply templates to the $selection parameter in "presort" mode,
		and of course add a special sort element or more to accomplish the task.
	-->
	<xsl:template name="customSort">
		<xsl:param name="selection"/>
		<xsl:apply-templates select="$selection" mode="presort">
			<xsl:sort select="." data-type="text" order="ascending"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="*" mode="presort">
		<nodeId><xsl:value-of select="generate-id()"/></nodeId>
	</xsl:template>

</xsl:stylesheet>