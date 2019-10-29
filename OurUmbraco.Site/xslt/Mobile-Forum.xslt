<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications" 
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:variable name="mode" select="umbraco.library:RequestQueryString('mode')" />
<xsl:variable name="id" select="umbraco.library:RequestQueryString('id')" />
<xsl:variable name="q" select="umbraco.library:RequestQueryString('q')" />

<xsl:variable name="loggedOn" select="umbraco.library:IsLoggedOn()" />

<xsl:variable name="root" select="umbraco.library:GetXmlNodeById(/macro/root)" />
<xsl:variable name="current" select="umbraco.library:GetXmlNodeById($id)" />

<xsl:variable name="loginpage" select="'/login'" />

<xsl:template match="/">

<xsl:choose>
<xsl:when test="$mode = 'forum'">
<xsl:call-template name="forum" />
</xsl:when>

<xsl:when test="$mode = 'topic'">
<xsl:call-template name="topic" />
</xsl:when>

<xsl:when test="$mode = 'latest'">
<xsl:call-template name="latest" />
</xsl:when>

<xsl:when test="$mode = 'search'">
<xsl:call-template name="search" />
</xsl:when>

<xsl:when test="$mode = 'overview'">
<xsl:call-template name="overview" />
</xsl:when>

<xsl:when test="$mode = 'yours'">
<xsl:call-template name="yours" />
</xsl:when>

<xsl:otherwise>
<xsl:call-template name="front" />
</xsl:otherwise>

</xsl:choose>


</xsl:template>

<xsl:template name="latest">
<xsl:call-template name="renderTopBar"><xsl:with-param name="title">Latest</xsl:with-param><xsl:with-param name="backLink" select="concat('/','') "/></xsl:call-template>

<xsl:variable name="topics" select="uForum.raw:LatestTopics(30, 0)"/>

<div id="content">
<ul class="list">

<xsl:for-each select="$topics/topics/topic">
<li>

<div class="memberBadge" style="margin-top: 3px; margin-bottom: 0px; float: left; background-image: url(/media/avatar/{latestReplyAuthor}.jpg);">.</div>

<xsl:choose>
<xsl:when test="latestComment &gt; 0">
<a href="?mode=topic&amp;id={id}#comment-{latestComment}">
RE: <xsl:value-of select="title" />
<br/> <span>Posted in <xsl:value-of select="umbraco.library:GetXmlNodeById(parentId)/@nodeName" /> by <xsl:value-of select="umbraco.library:GetMemberName(latestReplyAuthor)"/> </span>
</a>
</xsl:when>
<xsl:otherwise>
<a href="?mode=topic&amp;id={id}"><xsl:value-of select="title" />
<br/><span>Posted in <xsl:value-of select="umbraco.library:GetXmlNodeById(parentId)/@nodeName" /> by <xsl:value-of select="umbraco.library:GetMemberName(latestReplyAuthor)"/> </span>
</a>

</xsl:otherwise>
</xsl:choose>

<br style="clear: both"/>
</li>
</xsl:for-each>
</ul>
</div>

</xsl:template>

<xsl:template name="yours">
<xsl:call-template name="renderTopBar"><xsl:with-param name="title">Your Topics</xsl:with-param><xsl:with-param name="backLink" select="concat('/','') "/></xsl:call-template>
<div id="content">
<ul class="topicList list">
<xsl:for-each select="uForum.raw:TopicsWithAuthor(uForum:GetCurrentMember()/@id)/topics/topic">
<xsl:sort select="updated" order ="descending"/>

	<xsl:call-template name="renderTopic"><xsl:with-param name="topic" select="."/></xsl:call-template>

</xsl:for-each>
</ul>
</div>
</xsl:template>

<xsl:template name="front">
<xsl:call-template name="renderTopBar"><xsl:with-param name="title">our.umbraco.com</xsl:with-param></xsl:call-template>

<div id="content">
<xsl:if test="$loggedOn">
<p>Welcome <xsl:value-of select="umbraco.library:GetMemberName(uForum:GetCurrentMember()/@id)"/></p>
</xsl:if>
<ul class="list">
<li><a href="?mode=latest">Latest topics</a></li>
<xsl:if test="$loggedOn">
<li><a href="?mode=yours">Your topics</a></li>
</xsl:if>
<li><a href="?mode=overview">Forum overview</a></li>
</ul>
<br/>
<xsl:if test="not($loggedOn)">
<ul class="list">
<li><a href="{$loginpage}">Login</a></li>
</ul>
</xsl:if>
</div>
</xsl:template>




<xsl:template name="overview">
<xsl:call-template name="renderTopBar"><xsl:with-param name="title">Forums</xsl:with-param><xsl:with-param name="backLink" select="concat('/','') "/></xsl:call-template>

<div id="content">
<xsl:for-each select="$root/node">
<xsl:if test="count(./node) &gt; 0">
<h2><xsl:value-of select="@nodeName"/></h2>
	<xsl:call-template name="renderForum">  
		<xsl:with-param name="parent" select="uForum:Forums(./@id, true())"/>  
	</xsl:call-template>
</xsl:if>
</xsl:for-each>
</div>

</xsl:template>

<xsl:template name="forum">
<xsl:variable name="topics" select="uForum.raw:Topics($current/@id, 25, 0)//topic"/>

<xsl:call-template name="renderTopBar"><xsl:with-param name="title" select="$current/@nodeName"/><xsl:with-param name="backLink" select="concat('?mode=overview','') "/></xsl:call-template>


<div id="content">
<h2><xsl:value-of select="$current/@nodeName"/></h2>



<ul class="topicList list">
<xsl:for-each select="$topics">
	<xsl:sort select="created" order="descending" />
	<xsl:call-template name="renderTopic"><xsl:with-param name="topic" select="."/></xsl:call-template>
</xsl:for-each>
</ul>

<xsl:if test="string($current/data [@alias = 'forumAllowNewTopics'] = '1')">
<xsl:call-template name="renderNewTopic"><xsl:with-param name="forum" select="$current/@id"/></xsl:call-template>
</xsl:if>

<a class="backUp" href="#header">Back to the top</a>

</div>

</xsl:template>



<xsl:template name="topic">
<xsl:variable name="topic" select="uForum.raw:Topic($id)/topics/topic"/>
<xsl:variable name="comments" select="uForum.raw:CommentsByDate($id, $topic/replies, 0, 'DESC')"/>

<xsl:call-template name="renderTopBar"><xsl:with-param name="title" select="$topic/title"/><xsl:with-param name="backLink" select="concat('?mode=forum&amp;id=',$topic/parentId) "/></xsl:call-template>

<div id="content">
<h2><xsl:value-of select="$topic/title"/></h2>

<xsl:call-template name="renderFullTopic"><xsl:with-param name="topic" select="$topic"/></xsl:call-template>

<xsl:call-template name="renderTopicReply"><xsl:with-param name="topic" select="$topic"/></xsl:call-template>

<xsl:for-each select="$comments//comment">
  <xsl:if test="./isSpam != 'true'">
    <xsl:call-template name="renderComment">
      <xsl:with-param name="topic" select="$topic"/>
      <xsl:with-param name="comment" select="."/>
    </xsl:call-template>
  </xsl:if>
</xsl:for-each> 

</div>

<a class="backUp" href="#header">Back to the top</a>

</xsl:template>


<xsl:template name="search">

<xsl:call-template name="renderTopBar"><xsl:with-param name="title">Search</xsl:with-param><xsl:with-param name="backLink">javascript:history(-1);</xsl:with-param></xsl:call-template>

<xsl:variable name="results" select="uSearh:LuceneInContentType($q, 'forumComments,forumTopics', 0, 255, 30)"/>
<xsl:variable name="all" select="$results/search/results/result"/>


<div id="content">

<ul class="list">

	<xsl:for-each select="$all">
	<xsl:choose>
	<xsl:when test="contentType = 'forumTopics'">		
		<xsl:call-template name="renderTopicSearchResult"><xsl:with-param name="t" select="./id"/></xsl:call-template>
	</xsl:when>
	<xsl:when test="contentType = 'forumComments'">
		<xsl:call-template name="renderCommentSearchResult"><xsl:with-param name="c" select="./id"/></xsl:call-template>
	</xsl:when>
	</xsl:choose>
	</xsl:for-each>

</ul>


</div>

</xsl:template>




<xsl:template name="renderForum">
<xsl:param name="parent"/>
<xsl:if test="count($parent/forum) &gt; 0">
<ul class="forumList list">
<xsl:for-each select="$parent/forum">
<xsl:sort select="@SortOrder" />
<li class="arrow">
<small class="counter"><xsl:value-of select="./@TotalTopics"/></small>
<a href="?mode=forum&amp;id={@id}"><xsl:value-of select="./title"/></a>
</li>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>


<xsl:template name="renderNewTopic">

<div class="newTopic">
	<a id="newTopicStart" href="#">Create a new topic</a>
</div>

<xsl:choose>
<xsl:when test="not($loggedOn)">
<div id="pleaseLogin" style="display: none;">
	<h2>Create a new topic</h2>
	<div class="notification">You need to <a href="{$loginpage}">login</a> before you can create a new topic</div>
</div>
</xsl:when>
<xsl:otherwise>

<div id="newTopicForm" style="display: none;">
<h2>Create a new topic</h2>

<div class="form">
<input id="title" type="text" value=""/><br/>
</div>
<br/>
<div class="form">
<textarea col="10" id="body" rows="2" ></textarea><br/>
</div>
<div id="buttons">
<a href="#" class="newTopicButton" id="newTopicPost">Post Topic</a> <a class="cancelButton" id="newTopicCancel" href="#">Cancel</a><br style="clear: both;"/>
</div>

<p class="notice" style="display: none;" id="replyLoading">Please wait while we post your comment...</p>

</div>

</xsl:otherwise>
</xsl:choose>

<script type="text/javascript" charset="utf-8">

var TEXTAREA_LINE_HEIGHT = 13;
function grow() {
  var textarea = document.getElementById('body');
  var newHeight = textarea.scrollHeight;
  var currentHeight = textarea.clientHeight;

  if (newHeight &gt; currentHeight) {
     textarea.style.height = newHeight + 5 * TEXTAREA_LINE_HEIGHT + 'px';
  }
} 

 
$(document).ready(function(){

  $('#body').keyup(function(){grow();});
  $('#newTopicStart').click( function(){$('#pleaseLogin').show();$('#newTopicForm').show(); $(this).hide(); return false;});
  $('#newTopicCancel').click( function(){$('#newTopicForm').hide(); $('#newTopicStart').show(); $('#body').val(''); $('#title').val('');return false;});
 
  $('#newTopicPost').click(function(){		
			
			jQuery("#buttons").hide();
			jQuery("#replyLoading").show();
			
			var converter = new Showdown.converter();
			var title = jQuery('#title').val();
	      		var body = converter.makeHtml(jQuery('#body').val());
			var forumId = <xsl:value-of select="$id" />;
						
			var url = mForum.NewTopic(forumId, title,  body);
			location.href = location.href;
			return false;
	});


});


</script>


</xsl:template>

<xsl:template name="renderTopicReply">
<xsl:param name="topic"/>

<div class="reply"><a href="#" id="replyStart">Reply</a></div>

<xsl:choose>
<xsl:when test="not($loggedOn)">
<div id="pleaseLogin" style="display: none;">
	<h2>Post reply</h2>
	<div class="notification">You need to <a href="{$loginpage}">login</a> before you can reply</div>
</div>
</xsl:when>
<xsl:otherwise>
<div id="replyForm" style="display: none;">
<h2>Post reply</h2>



<div class="form">
<textarea col="10" id="comment" rows="2" ></textarea><br/>
</div>
<div id="buttons">
<a href="#" class="replyButton" id="replyPost">Post Reply</a> <a class="cancelButton" id="replyCancel" href="#">Cancel</a><br style="clear: both;"/>
</div>

<p class="notice" style="display: none;" id="replyLoading">Please wait while we post your comment...</p>

</div>
</xsl:otherwise>
</xsl:choose>



<script type="text/javascript" charset="utf-8">

var TEXTAREA_LINE_HEIGHT = 13;
function grow() {
  var textarea = document.getElementById('comment');
  var newHeight = textarea.scrollHeight;
  var currentHeight = textarea.clientHeight;

  if (newHeight &gt; currentHeight) {
     textarea.style.height = newHeight + 5 * TEXTAREA_LINE_HEIGHT + 'px';
  }
} 

 
$(document).ready(function(){

  $('#comment').keyup(function(){grow();});
  $('#replyStart').click( function(){$('#pleaseLogin').show();$('#replyForm').show(); $(this).hide(); return false;});
  $('#replyCancel').click( function(){$('#replyForm').hide(); $('#replyStart').show(); $('#comment').val(''); return false;});
 
  $('#replyPost').click(function(){		
			
			jQuery("#buttons").hide();
			jQuery("#replyLoading").show();
			
			var converter = new Showdown.converter();
	      		var body = converter.makeHtml(jQuery('#comment').val());
			var topicId = <xsl:value-of select="$id" />;
						
			var url = mForum.NewComment(topicId, 10,  body);
			location.href = location.href;
			return false;
	});


});


</script>



</xsl:template>

<xsl:template name="renderCommentSearchResult">
<xsl:param name="c"/>
<xsl:variable name="comment" select="uForum.raw:Comment($c)//comment"/>

<li class="comment" id="comment{$comment/id}">
<a href="?mode=topic&amp;id={$comment/topicId}#comment{$comment/id}">Re: <xsl:value-of select="uForum.raw:Topic($comment/topicId)//title"/>
<br/><span>Posted <xsl:value-of select="umbraco.library:ShortDate($comment/created)"/>, By <xsl:value-of select="umbraco.library:GetMemberName($comment/memberId)"/></span>
</a>
<p>
<xsl:value-of select="umbraco.library:Replace(umbraco.library:TruncateString(./content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
</li>
</xsl:template>

<xsl:template name="renderTopicSearchResult">
<xsl:param name="t"/>
<xsl:variable name="topic" select="uForum.raw:Topic($t)//topic"/>
<li class="topic" id="topic{$topic/id}">
<xsl:if test="$topic/answer != 0"><xsl:attribute name="class">topic solved</xsl:attribute></xsl:if>
<a href="?mode=topic&amp;id={$topic/id}"><xsl:value-of select="$topic/title"/>
<br/><span>Posted <xsl:value-of select="umbraco.library:ShortDate($topic/created)"/>, By <xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/></span>
</a>
<p>
<xsl:value-of select="umbraco.library:Replace(umbraco.library:TruncateString(./content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
</li>
</xsl:template>

<xsl:template name="renderTopic">
<xsl:param name="topic"/>
<li class="topic" id="topic{$topic/id}">
<xsl:if test="$topic/answer != 0"><xsl:attribute name="class">topic solved</xsl:attribute></xsl:if>
<small class="counter"><xsl:value-of select="$topic/replies"/></small>
<a href="?mode=topic&amp;id={$topic/id}"><xsl:value-of select="$topic/title"/>
<br/><span>Author:<xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/> - <xsl:value-of select="uForum:TimeDiff($topic/created)"/></span>
</a>
</li>


</xsl:template>






<xsl:template name="renderFullTopic">
<xsl:param name="topic"/>

<div class="topicFull" id="topic{$topic/id}">
<div class="memberBadge" style="background: url(/media/avatar/{$topic/memberId}.jpg);">.</div>
<div class="body">
<strong><xsl:value-of select="umbraco.library:GetMemberName($topic/memberId)"/>:</strong> 
<xsl:value-of select="uForum:Sanitize(uForum:ResolveLinks( uForum:CleanBBCode( $topic/body ) ) )" disable-output-escaping="yes"/>
<small><xsl:value-of select="uForum:TimeDiff($topic/created)"/></small>
</div>

</div>

</xsl:template>





<xsl:template name="renderComment">
<xsl:param name="comment"/>
<xsl:param name="topic"/>

<div class="post postComment" id="comment{$comment/id}">
<xsl:if test="id = $topic/answer">
<xsl:attribute name="class">post postComment postSolution</xsl:attribute>
</xsl:if>

<xsl:if test="id = $topic/answer">
<a name="solution" style="visibility: hidden">Solution</a>
</xsl:if>

<div class="memberBadge" style="background-image: url(/media/avatar/{$comment/memberId}.jpg);">.</div>
<div class="body">
	<strong><xsl:value-of select="umbraco.library:GetMemberName($comment/memberId)"/>:</strong> 
	<xsl:value-of select="uForum:Sanitize( uForum:ResolveLinks( uForum:CleanBBCode( $comment/body ) ) )" disable-output-escaping="yes"/>
	<small><xsl:value-of select="uForum:TimeDiff($topic/created)"/></small>
</div>

<a name="comment-{$comment/id}" style="visibility: hidden"> Comment with ID: <xsl:value-of select="$comment/id"/></a>

</div>

</xsl:template>


<xsl:template name="renderTopBar">
<xsl:param name="title"/>
<xsl:param name="backLink"/>
<div id="header" class="topBar">
<xsl:if test="$backLink != ''"><a id="backButton" href="{$backLink}">back</a></xsl:if>
<h1><xsl:value-of select="$title" /></h1>

<xsl:if test="not($loggedOn)">
<a href="{$loginpage}?returnurl={umbraco.library:RequestServerVariables('PATH_INFO')}">Login</a>
</xsl:if>

</div>

<div id="search">
	<input type="text" id="f_search" name="q" class="field" /> <input type="image" src="/images/search.png" id="bt_search" value="Search" /> 
</div>

</xsl:template>


</xsl:stylesheet>