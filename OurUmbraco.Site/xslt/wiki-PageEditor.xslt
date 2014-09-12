<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:uWiki="urn:uWiki" xmlns:uPowers="urn:uPowers"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" 
  exclude-result-prefixes="uWiki uPowers msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum ">

<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:template match="/">
<xsl:variable name="minLevel" select="1" />
<xsl:variable name="isAdmin" select="uForum:IsInGroup('admin') or uForum:IsInGroup('wiki editor')"/>
<xsl:variable name="isLoggedOn" select="umbraco.library:IsLoggedOn()"/>
<xsl:variable name="mem"><xsl:if test="$isLoggedOn"><xsl:value-of select="uForum:GetCurrentMember()/@id"/></xsl:if></xsl:variable>
<xsl:variable name="canVote" select="boolean( number(uForum:GetCurrentMember()/reputationCurrent) &gt;= 25 )"/>
<xsl:variable name="isUmbracoHelp" select="$currentPage/@level > 3 and $currentPage/ancestor-or-self::* [@isDoc and @level = 3]/@nodeName = 'Umbraco Help'" />

<div id="wikivoting" class="voting rounded">
  <span>
    <a href="#" class="history" rel="{$currentPage/@id},wiki"><xsl:value-of select="uPowers:Score($currentPage/@id, 'powersWiki')"/></a>
  </span>
  
  <xsl:if test="$isLoggedOn and $mem != $currentPage/author">
  <xsl:variable name="vote" select="uPowers:YourVote($mem, $currentPage/@id, 'powersWiki')"/>   
  
  <xsl:if test="$vote = 0">  
    <a href="#" class="WikiUp vote" rel="{$currentPage/@id}">
      <xsl:if test="boolean(not($canVote))">
           <xsl:attribute name="class">noVote</xsl:attribute >
       </xsl:if>
      Reward</a> 
  <!--  <a href="#" class="WikiDown"  rel="{$currentPage/@id}"><img src="/css/img/icons/thumb_down.png" alt="Mark this page as useless noise" /></a>-->
  </xsl:if>
  
  </xsl:if>

</div>
  

<h1 id="wikiHeader" class="wikiheadline"><xsl:value-of select="$currentPage/@nodeName"/></h1>
<input type="text" id="wikiHeaderEditor" class="wikiheadline" style="display: none;" />


<xsl:if test="$isLoggedOn">
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('tinyMce', '/scripts/tiny_mce/tiny_mce_src.js')"/>
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uWiki', '/scripts/wiki/uWiki.js?v=3')"/>

<script type="text/javascript">
  jQuery(document).ready(function(){
  jQuery("#bt_edit").click(function(){ uWiki.Edit(<xsl:value-of select="$currentPage/@id"/>, '<xsl:value-of select="$currentPage/@version"/>', <xsl:value-of select="$isUmbracoHelp"/>); return false; });
  jQuery(".bt_save").click(function(e){ 
    //Prevent the submit event and remain on the screen
    e.preventDefault();
 
    var wikiKeywords = '';
    if(jQuery('#wikiKeywords').length > 0){
          wikiKeywords = jQuery('#wikiKeywords').val();
     }
  
    uWiki.Save(<xsl:value-of select="$currentPage/@id"/>, jQuery('#wikiHeaderEditor').val(),  tinyMCE.get('wikiContent').getContent(), wikiKeywords ); 
    return false; 
  });
  jQuery("#bt_cancel").click(function(){ uWiki.Cancel(true); return false; });
  jQuery("#bt_history").click(function(){ jQuery("#historyBar").toggle(); return false; });
  });
</script>
</xsl:if>



<div id="options">
<ul>
    <xsl:choose>
      <xsl:when test="$isUmbracoHelp">
          <xsl:if test="$isLoggedOn and $isAdmin">
              <li  id="tab_edit"><a id="bt_edit" class="act edit" href="#">Edit this page</a></li>
          </xsl:if>
      </xsl:when>
      <xsl:otherwise>
          <xsl:if test="$isLoggedOn"> 
            <li  id="tab_edit"><a id="bt_edit" class="act edit" href="#">Edit this page</a></li>
          </xsl:if>  
      </xsl:otherwise>
  </xsl:choose>
  
    <xsl:if test="$isLoggedOn">
      <li ><a class="act upload" href="{umbraco.library:NiceUrl($currentPage/@id)}/WikiPageAttachments">Upload Attachments</a></li>
      <li><a class="act history" id="bt_history" href="#">History</a></li>
    </xsl:if>   
  
      <xsl:choose>
      <xsl:when test="$isUmbracoHelp">
          <xsl:if test="$isLoggedOn and $isAdmin">
             <li class="create"><a class="act add" id="bt_create" href="{umbraco.library:NiceUrl($currentPage/@id)}/New-Page?wikiEditor=y&amp;subPage=y"><span>Create a new page</span></a></li>
          </xsl:if>
      </xsl:when>
      <xsl:otherwise>
          <xsl:if test="$isLoggedOn">
           <li class="create"><a class="act add" id="bt_create" href="{umbraco.library:NiceUrl($currentPage/@id)}/New-Page"><span>Create a new page</span></a></li>
          </xsl:if>  
      </xsl:otherwise>
  </xsl:choose>
  
    
   <xsl:if test="$isLoggedOn and $isAdmin">
       <li class="move"><a class="act move WikiMove" id="bt_move" href="#" rel="{$currentPage/@id}">Move this page</a></li>
       <li class="delete"><a class="act delete WikiDelete" id="bt_delete" href="#" rel="{$currentPage/@id}">Delete this page</a></li>   
    </xsl:if>
</ul>
</div>

<xsl:if test="$isLoggedOn">
  <div id="historyBar" class="box">
    <h4>Click the pins to preview older versions of this content</h4>
    <xsl:call-template name="history" />
  </div>
</xsl:if>

<div id="editMode" style="display: none;"> 
  Your changes have not been saved yet, <button class="bt_save">save now</button><em> or </em><a href="#" id="bt_cancel">discard changes</a>
</div> 

<div class="wiki-sidebar" style="float: right;">
  <xsl:choose>
  <xsl:when test="count($currentPage/* [@isDoc]) &gt; 0"> 
  <xsl:call-template name="drawNodes"> 
    <xsl:with-param name="parent" select="$currentPage"/>
    <xsl:with-param name="sidebar" select="true()"/>
  </xsl:call-template>
  </xsl:when>
  <xsl:otherwise>
  <xsl:call-template name="drawNodes"> 
    <xsl:with-param name="parent" select="$currentPage/.."/>
    <xsl:with-param name="sidebar" select="true()"/>
  </xsl:call-template>
  </xsl:otherwise>
   </xsl:choose>  
</div>

  
<div id="wikiContent"><xsl:value-of select="uForum:Sanitize(uForum:ResolveLinks( uForum:CleanBBCode( $currentPage/bodyText ) ) )" disable-output-escaping="yes"/></div>

<xsl:if test="$isAdmin">
<div id="divKeywords" style="display: none;">
  <h3>Keywords for finding related content</h3>
  <small>Enter keywords for aggregating related projects, forum posts and videos, seperate keywords with commas</small>
  <input type="text" style="width: 970px;" class="title" id="wikiKeywords" value="{$currentPage/keywords}" />
</div>
</xsl:if>
  
  
<xsl:variable name="files" select="uWiki:GetAttachedFiles($currentPage/@id)" />
<xsl:if test="count($files//file)">
<h3>Attached files</h3>
<ul id="attachedFiles">
<xsl:for-each select="$files//file">
  <xsl:if test="boolean(./current)"> 
  <li class="{type}">
    <a class="fileName" href="/FileDownload?id={id}"><xsl:value-of select="name" /></a>
    <small><xsl:value-of select="umbraco.library:GetDictionaryItem(type)"/> 
    - uploaded <xsl:value-of select="umbraco.library:ShortDate(createDate)"/>
    by <a href="/member/{createdBy}"><xsl:value-of select="umbraco.library:GetMemberName(createdBy)"/></a>
    </small>
  </li>
  </xsl:if>
</xsl:for-each>
</ul>
</xsl:if>

<br style="clear: both;"/>

</xsl:template>


    
    
<xsl:template name="drawNodes">
<xsl:param name="parent"/> 
<xsl:param name="sidebar"/> 
<xsl:if test="umbraco.library:IsProtected($parent/@id, $parent/@path) = 0 or (umbraco.library:IsProtected($parent/@id, $parent/@path) = 1 and umbraco.library:IsLoggedOn() = 1)">
<ul>
<xsl:for-each select="$parent/* [@isDoc and string(umbracoNaviHide) != '1']"> 
<li>  
<a href="{umbraco.library:NiceUrl(@id)}">
<xsl:value-of select="@nodeName"/></a>
  
<xsl:if test="count(./* [@isDoc and string(umbracoNaviHide) != '1']) &gt; 0 and not($sidebar)">   
<xsl:call-template name="drawNodes">    
<xsl:with-param name="parent" select="."/>    
</xsl:call-template>  
</xsl:if> 

</li>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>

    
<xsl:template name="history">
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uWiki', '/scripts/wiki/uWiki.js')"/>
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('jsDiff', '/scripts/libs/jsDiff.js')"/>

<input type="hidden" id="s_version"/>
<script type="text/javascript">
jQuery(document).ready(function(){

  jQuery(".wikiVersion").bind("mouseenter",function(){
    jQuery(this).siblings("div.versionInfo").addClass("peak");
  }).bind("mouseleave",function(){
    jQuery(this).siblings("div.versionInfo").removeClass("peak");
  }).click(function(){
    uWiki.PreviewOldVersion( <xsl:value-of select="$currentPage/@id"/>, jQuery(this).attr('rel'), '<xsl:value-of select="$currentPage/@version"/>' );
    jQuery("#s_version").val(jQuery(this).attr('rel'));

    jQuery("#history div").removeClass("show");
    jQuery(this).siblings("div.versionInfo").addClass("show");
  
    return false;  
  });  

  jQuery(".bt_rollback").click(function(){ uWiki.Rollback(<xsl:value-of select="$currentPage/@id"/>, jQuery(this).attr('rel') ); return false; });  
});
</script>

<!-- total timespan of this page -->
<xsl:variable name="ts" select="Exslt.ExsltDatesAndTimes:seconds(Exslt.ExsltDatesAndTimes:difference($currentPage/@createDate, $currentPage/@updateDate))"/>

<ul id="history">
<xsl:for-each select="uWiki:PageHistory($currentPage/@id)//version">
<xsl:sort select="date" order="ascending"/>

<xsl:variable name="itemTS" select="(number(Exslt.ExsltDatesAndTimes:seconds(Exslt.ExsltDatesAndTimes:difference($currentPage/@createDate, date))) div $ts) * 100"/>

<li style="left: {$itemTS}%;">

<a rel="{guid}" href="#" class="wikiVersion" title="{version}">
<xsl:if test="guid= $currentPage/@version"><xsl:attribute name="class">wikiVersion current</xsl:attribute></xsl:if>&nbsp;
</a>

<div class="versionInfo rounded">
<a href="/member/{author}" class="author"><img src="/media/avatar/{author}.jpg" /></a>
<strong>Author: <a href="/member{author}"><xsl:value-of select="umbraco.library:GetMemberName(author)"/></a></strong><br/>
<small>Edited <xsl:value-of select="uForum:TimeDiff(date)"/></small>
<a href="#" rel="{version}" title="{version}" class="bt_rollback">Restore to this version</a>
</div>

</li>

</xsl:for-each>

</ul>
<small><abbr title="Creation date: {umbraco.library:FormatDateTime($currentPage/@createDate, 'g')}">First</abbr></small>
<small style="float: right"><abbr title="Latest change date: {umbraco.library:FormatDateTime($currentPage/@updateDate, 'g')}">Latest</abbr></small>
</xsl:template>


</xsl:stylesheet>