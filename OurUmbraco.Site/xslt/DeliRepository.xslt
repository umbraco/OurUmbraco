<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library ">


  <xsl:output method="xml" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>

  <xsl:variable name="mode" select="umbraco.library:RequestQueryString('mode')"/>
  <xsl:variable name="id" select="umbraco.library:RequestQueryString('id')"/>
  <xsl:variable name="q" select="umbraco.library:RequestQueryString('q')"/>

  <xsl:variable name="version" select="umbraco.library:RequestQueryString('version')"/>
  <xsl:variable name="useLegacySchema" select="umbraco.library:RequestQueryString('useLegacySchema')"/>

  <!-- legacy -->
  <xsl:variable name="category" select="umbraco.library:RequestQueryString('category')"/>

  <xsl:variable name="root" select="umbraco.library:GetXmlNodeById(1113)"/>
  <xsl:variable name="current" select="umbraco.library:GetXmlNodeById($id)"/>
  <xsl:variable name="cb" select="umbraco.library:Request('callback')"/>

  <xsl:variable name="defaultIcon" select="umbraco.library:GetMedia(8558, false())//umbracoFile"/>

  <!-- here we build the standard QS to attach to all links -->
  <xsl:variable name="qs" select="concat('callback=',$cb,'&amp;version=',$version,'&amp;useLegacySchema=', $useLegacySchema)"/>


  <xsl:variable name="experimental">
    <xsl:choose>
      <xsl:when test="contains($cb,'65194810-1f85-11dd-bd0b-0800200c9a68')">1</xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="schema">
    <xsl:choose>
      <xsl:when test="$useLegacySchema = 'True' and $version = 'v45'">v45l</xsl:when>
      <xsl:when test="$useLegacySchema = 'False'">v45</xsl:when>
      <xsl:when test="$useLegacySchema = 'True' or $version = 'v31' or $version = 'v4'">v4</xsl:when>
      <xsl:when test="$version= 'all'">all</xsl:when>
      <xsl:otherwise>v4</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="/">


    <xsl:choose>
      <xsl:when test="$q != ''">
        <xsl:call-template name="search" />
      </xsl:when>
      <xsl:when test="$mode = 'package'">
        <xsl:call-template name="package" />
      </xsl:when>
      <xsl:when test="$mode = 'category'">
        <xsl:call-template name="category">
          <xsl:with-param name="current" select="$current" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$category != ''">
        <xsl:call-template name="category">
          <xsl:with-param name="current" select="umbraco.library:GetXmlNodeById($category)" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="frontpage" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--
***************************************
* Frontpage
***************************************
-->
  <xsl:template name="frontpage">
    <p class="guiDialogNormal">

      <h4>Categories:</h4>
      <div class="repoBox">
        <xsl:for-each select="$root//ProjectGroup">
          <div style="float: left; width:200px;margin-right: 20px;">
            <xsl:if test="string(data [@alias = 'icon']) != ''">
              <img src="{ umbraco.library:GetMedia(icon , false() )//umbracoFile }" style="vertical-align: middle; margin: 0 10px 10px 0" />
            </xsl:if>
            <a href="?mode=category&amp;id={@id}&amp;{$qs}">
              <xsl:value-of select="@nodeName"/>
            </a>

          </div>
        </xsl:for-each>
        <br style="clear: both;"/>
      </div>

      <h4>Latest Packages:</h4>
      <div class="repoBox noborder">
        <xsl:call-template name="RenderCategory">
          <xsl:with-param name="page" select="$root" />
          <xsl:with-param name="maxItems">10</xsl:with-param>
        </xsl:call-template>
        <br style="clear: both;"/>
      </div>

    </p>
  </xsl:template>


  <!--
***************************************
* Category listing
***************************************
-->
  <xsl:template name="category">
    <xsl:param name="current" />

    <h4>
      <xsl:value-of select="$current/@nodeName"/>
    </h4>
    <xsl:call-template name="breadcrumb" />

    <p class="guiDialogNormal">
      <div class="repoBox noborder">

        <xsl:call-template name="RenderCategory">
          <xsl:with-param name="page" select="$current" />
          <xsl:with-param name="maxItems">99999999</xsl:with-param>
          <xsl:with-param name="sort">byName</xsl:with-param>
        </xsl:call-template>

      </div>

    </p>
  </xsl:template>


  <!--
***************************************
* Package details
***************************************
-->
  <xsl:template name="package">
    <h4>
      <xsl:value-of select="$current/@nodeName"/>
    </h4>
    <xsl:call-template name="breadcrumb" />


    <div class="repoBox noborder">
      <p class="guiDialogNormal" style="margin-top: 5px;">

        <xsl:value-of select="$current/description" disable-output-escaping="yes"/>


        <a href="{umbraco.library:NiceUrl($current/@id)}" target="_blank" rel="noreferrer noopener">View complete project page</a>
      </p>

      <p class="guiDialogNormal" style="margin-top: 5px;">
        <strong>Author: </strong>
        <a href="https://our.umbraco.com/member/{$current/owner}" target="_blank" rel="noreferrer noopener">
          <xsl:value-of select="umbraco.library:GetMemberName($current/owner)"/>
        </a>
      </p>
    </div>

    <h3 style="margin: 0;">Install</h3>

    <xsl:choose>
      <xsl:when test="$current/file != ''">
        <xsl:if test="uWiki:GetAttachedFile($current/file)//verified = 'false'">
          <xsl:call-template name="RenderNotVerifiedNotice" />
        </xsl:if>
      </xsl:when>
      <xsl:when test="count(uWiki:FindPackageForUmbracoVersion($current/@id,$schema)//wikiFile) &gt; 0">
        <xsl:if test="uWiki:FindPackageForUmbracoVersion($current/@id,$schema)//verified = 'false'">
          <xsl:call-template name="RenderNotVerifiedNotice" />
        </xsl:if>
      </xsl:when>
    </xsl:choose>

    <p class="guiDialogNormal">

      <a href="#">

        <xsl:if test="$cb != ''">
          <xsl:attribute name="onclick">
            if(confirm('Are you sure you wish to download:\n\n<xsl:value-of select="$current/@nodeName"/>\n\n'))document.location.href = 'http://<xsl:value-of select="$cb"/>&amp;guid=<xsl:value-of select="$current/packageGuid"/>'
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="$cb = '' and $current/file != ''">


          <xsl:attribute name="onclick">
            document.location.href = '<xsl:value-of select="concat('/FileDownload?id=', $current/file)" />'
          </xsl:attribute>
        </xsl:if>

        <img src="/images/repository/downloadBtn.gif" align="absmiddle" alt="Download and Install Package"/>&nbsp;Download and Install package
      </a>
    </p>


    <h3 style="margin: 0;">Documentation</h3>
    <p class="guiDialogNormal">

      <xsl:variable name="currentDocumentation" select="uWiki:FindPackageDocumentationForUmbracoVersion($current/@id,$schema)" />

      <xsl:choose>
        <xsl:when test="count($currentDocumentation/wikiFile) &gt; 0">
          <a href="/FileDownload?id={$currentDocumentation/wikiFile/@id}" title="Download Documentation (opens in new window)" target="_blank" rel="noreferrer noopener">
            <img src="/images/repository/documentation.png" align="absmiddle" alt="Download Documentation (opens in new window)"/>&nbsp;Documentation (opens in new window)
          </a>
        </xsl:when>
        <xsl:otherwise>
          <em>There's currently no documentation available</em>
        </xsl:otherwise>
      </xsl:choose>
    </p>
  </xsl:template>


  <!--
*****************************************
* Search
*****************************************
-->
  <xsl:template name="search">
    search
  </xsl:template>




  <!--
*****************************************
* Breadcrumb
*****************************************
-->
  <xsl:template name="breadcrumb">
    <p id="breadcrumb">
      <a href="?{$qs}">Umbraco Package Repository</a>

      <!--
<xsl:if test="$mode = 'category' or $mode = 'package'">
<xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
  <a href="?callback={$cb}&amp;mode=category&amp;id={$root/@id}"><xsl:value-of select="$root/../@nodeName" /></a>  
</xsl:if>
  -->

      <!-- legacy -->
      <xsl:if test="$category != ''">
        <xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
        <a href="?mode=category&amp;id={$category}&amp;{$qs}">
          <xsl:value-of select="umbraco.library:GetXmlNodeById($category)/@nodeName" />
        </a>
      </xsl:if>

      <xsl:if test="$mode = 'package'">
        <xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
        <a href="?mode=category&amp;id={$current/../@id}&amp;{$qs}">
          <xsl:value-of select="$current/../@nodeName" />
        </a>

        <xsl:text disable-output-escaping="yes"> &amp;rsaquo;&amp;rsaquo; </xsl:text>
        <a href="?mode=package&amp;id={$current/@id}&amp;{$qs}">
          <xsl:value-of select="$current/@nodeName" />
        </a>
      </xsl:if>
    </p>
  </xsl:template>


  <xsl:template name="RenderCategory">
    <xsl:param name="page" />
    <xsl:param name="maxItems"/>
    <xsl:param name="sort"/>

    <xsl:variable name="items">
      <xsl:choose>
        <xsl:when test="$schema = 'v45l'">
          <xsl:copy-of select="$page/descendant::Project [( contains( concat(compatibleVersions,','),'v45l,') or contains(compatibleVersions,'nan') ) and ( (vendorUrl = '' or providesTrial = '1' ) or ../@id = 8567) and file != '' and packageGuid != '' and ($experimental = 1 or string(approved) = '1') and string(notAPackage) != '1']"/>
        </xsl:when>
        <xsl:when test="$schema = 'v45'">
          <xsl:copy-of select="$page/descendant::Project [( contains( concat(compatibleVersions,','),'v45,') or contains(compatibleVersions,'nan') ) and ( (vendorUrl = '' or dprovidesTrial = '1' ) or ../@id = 8567) and file != '' and packageGuid != '' and ($experimental = 1 or string(approved) = '1') and string(notAPackage) != '1']"/>
        </xsl:when>
        <xsl:when test="$schema = 'v4'">
          <xsl:copy-of select="$page/descendant::Project [( contains( concat(compatibleVersions,','),'v4,')  or contains(compatibleVersions,'nan') ) and ( (vendorUrl = '' or providesTrial = '1' ) or ../@id = 8567) and file != '' and packageGuid != '' and ($experimental = 1 or string(approved) = '1') and string(notAPackage) != '1']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="$page/descendant::Project [file != '' and packageGuid != '' and ($experimental = 1 or string(approved) = '1') and string(notAPackage) != '1']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$sort = 'byName'">
        <xsl:for-each select="msxml:node-set($items)/* [@isDoc]">
          <xsl:sort select="@nodeName" order="ascending"/>
          <xsl:if test="position() &lt;= $maxItems">
            <xsl:call-template name="RenderPackageItem">
              <xsl:with-param name="item" select="." />
              <xsl:with-param name="truncate">1</xsl:with-param>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="msxml:node-set($items)/* [@isDoc]">
          <xsl:sort select="@createDate" order="descending"/>
          <xsl:if test="position() &lt;= $maxItems">
            <xsl:call-template name="RenderPackageItem">
              <xsl:with-param name="item" select="." />
              <xsl:with-param name="truncate">1</xsl:with-param>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>

    <!--  
<xsl:choose>
  <xsl:when test="$schema = 'v45'">
  <xsl:for-each select="$page/descendant::node [@nodeTypeAlias = 'Project' and contains(data [@alias = 'compatibleVersions'],'v45') and data [@alias = 'file'] != '' and data [@alias = 'packageGuid'] != '' and ($experimental = 1 or string(data [@alias = 'approved']) = '1') ]">
    <xsl:sort select="@createDate" order="descending"/>
      <xsl:if test="position() &lt;= $maxItems">
        <xsl:call-template name="RenderPackageItem">
          <xsl:with-param name="item" select="." />
          <xsl:with-param name="truncate">1</xsl:with-param>
        </xsl:call-template>
      </xsl:if>
  </xsl:for-each>
  </xsl:when>
  
  <xsl:when test="$schema = 'v4'">
  <xsl:for-each select="$page/descendant::node [@nodeTypeAlias = 'Project' and contains( concat(data [@alias = 'compatibleVersions'],','),'v4,') and data [@alias = 'file'] != '' and data [@alias = 'packageGuid'] != '' and ($experimental = 1 or string(data [@alias = 'approved']) = '1') ]">
    <xsl:sort select="@createDate" order="descending"/>
      <xsl:if test="position() &lt;= $maxItems">
        <xsl:call-template name="RenderPackageItem">
          <xsl:with-param name="item" select="." />
          <xsl:with-param name="truncate">1</xsl:with-param>
        </xsl:call-template>
      </xsl:if>
  </xsl:for-each>
  </xsl:when>

  <xsl:when test="$schema = 'all'">
  <xsl:for-each select="$page/descendant::node [@nodeTypeAlias = 'Project' and data [@alias = 'file'] != '' and data [@alias = 'packageGuid'] != '' and ($experimental = 1 or string(data [@alias = 'approved']) = '1') ]">
    <xsl:sort select="@createDate" order="descending"/>
      <xsl:if test="position() &lt;= $maxItems">
        <xsl:call-template name="RenderPackageItem">
          <xsl:with-param name="item" select="." />
          <xsl:with-param name="truncate">1</xsl:with-param>
        </xsl:call-template>
      </xsl:if>
  </xsl:for-each>  
    
  </xsl:when>
</xsl:choose>  
-->

  </xsl:template>

  <xsl:template name="RenderPackageItem">
    <xsl:param name="item" />
    <xsl:param name="truncate" />

    <xsl:variable name="icon">
      <xsl:choose>
        <xsl:when test="string($item/defaultScreenshotPath) != ''">
          <xsl:value-of select="$item/defaultScreenshotPath"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$defaultIcon"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div style="margin-bottom: 20px; display: block; clear: both">
      <img src="{$icon}" style="width: 32px; height: 32px; float: left; vertical-align: middle;" />
      <div style="margin-left: 50px;">
        <h3 style="margin:0">
          <a href="?id={$item/@id}&amp;mode=package&amp;{$qs}">
            <xsl:value-of select="@nodeName"/>
          </a>
        </h3>
        <p style="color: #666; font-size: 90%; margin: 5px 0">

          <xsl:choose>
            <xsl:when test="$truncate = '1'">
              <xsl:value-of select="umbraco.library:TruncateString(  umbraco.library:ReplaceLineBreaks( umbraco.library:StripHtml($item/description )), 170, '...')" disable-output-escaping="yes"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="umbraco.library:ReplaceLineBreaks( umbraco.library:StripHtml( $item/description ))" disable-output-escaping="yes"/>
            </xsl:otherwise>
          </xsl:choose>

          <br/>
        </p>
        <a href="?id={$item/@id}&amp;mode=package&amp;{$qs}">
          <img src="/images/repository/info.png" align="absmiddle" alt="Info"/>&nbsp;More info and download
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template name="RenderNotVerifiedNotice">
    <div class="notice" style="margin-top:5px">
      <p>
        <strong>Please note:</strong> the compatibility between this package and your umbraco version hasn't been verified by our admins yet.
      </p>
    </div>
  </xsl:template>
</xsl:stylesheet>