<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library"
	xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes"
	xmlns:uForum="urn:uForum"
  xmlns:uForum.raw="urn:uForum.raw" xmlns:uPowers="urn:uPowers" xmlns:Notifications="urn:Notifications"
	exclude-result-prefixes="uPowers uForum.raw msxml umbraco.library Exslt.ExsltDatesAndTimes uForum uForum.raw Notifications">

  <xsl:output method="html" omit-xml-declaration="yes"/>
  <xsl:param name="currentPage"/>

  <!-- Member -->
  <xsl:variable name="isAdmin" select="uForum:IsInGroup('admin')"/>
  <xsl:variable name="isModerator" select="uForum:IsModerator()"/>
  <xsl:variable name="mem">
    <xsl:if test="umbraco.library:IsLoggedOn()">
      <xsl:value-of select="uForum:GetCurrentMember()/@id"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="canVote" select="boolean( number(uForum:GetCurrentMember()/reputationCurrent) &gt;= 25 )"/>

  <xsl:template match="/">

    <!-- Topic Data -->
    <xsl:variable name="topicID" select="number(umbraco.library:ContextKey('topicID'))"/>
    <xsl:variable name="topic" select="uForum.raw:Topic($topicID)"/>

    <xsl:choose>
      <xsl:when test="not($topic)">
        <xsl:variable name="topicNotFound" select="uForum:TopicNotFound($topicID)" />
      </xsl:when>
      <xsl:otherwise>

        <!-- Sorting -->
        <xsl:variable name="sortBy">
          <xsl:choose>
            <xsl:when test="umbraco.library:RequestQueryString('sort') != ''">
              <xsl:value-of select="umbraco.library:RequestQueryString('sort')"/>
            </xsl:when>
            <xsl:otherwise>oldest</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- Current Page -->
        <xsl:variable name="p">
          <xsl:choose>
            <xsl:when test="string(number( umbraco.library:RequestQueryString('p') )) != 'NaN'">
              <xsl:value-of select="umbraco.library:RequestQueryString('p')"/>
            </xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- RSS -->
        <xsl:value-of select="uForum:RegisterRssFeed( concat('https://our.umbraco.com/rss/topic?id=',$topicID), concat('New replies to the the ',$topic/title ,' thread'), 'topicRss')"/>

        <!-- Treshold -->
        <xsl:variable name="treshold">
          <xsl:choose>
            <xsl:when test="umbraco.library:IsLoggedOn()">
              <xsl:value-of select="uForum:GetCurrentMember()/treshold" />
            </xsl:when>
            <xsl:otherwise>-10</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- Paging -->
        <xsl:variable name="maxitems">10</xsl:variable>
        <xsl:variable name="pages" select="uForum:TopicPager($topicID, $maxitems, $p)"/>
        
        <!-- if the isSpam property is null, count will be zero, assume that it's not spam -->
        <xsl:variable name="canSeeTopic" select="(count($topic/isSpam) = 0 or $topic/isSpam = 'false') or ($topic/isSpam = 'true' and $isModerator = 'true') or ($topic/isSpam = 'true' and $topic/@memberId = $mem)" />

        <xsl:choose>
          
          <xsl:when test="$canSeeTopic = true()">
            <!-- Indicate if topic has been solved -->
            <ul class="commentsList">
              <!-- Display the topic start -->

              <xsl:if test="$p = 0">

                <xsl:variable name="topicStarter" select="umbraco.library:GetMember($topic/@memberId)"/>

                <li class="post" id="posts-{$topicID}">
                  <div class="author vcard">
                    <xsl:call-template name="badge">
                      <xsl:with-param name="mem" select="$topicStarter" />
                      <xsl:with-param name="date" select="$topic/@created" />
                    </xsl:call-template>
                  </div>

                  <div class="comment">
                    <div class="meta">
                      <h2>
                        <xsl:value-of select="$topic/title"/>
                      </h2>



                      <div class="postedAt">
                        <a href="/member/{$topicStarter//@id}">
                          <xsl:value-of select="$topicStarter//@nodeName"/>
                        </a>
                        <xsl:text> started this topic </xsl:text>
                        <strong title="{umbraco.library:FormatDateTime($topic/@created, 'MMMM d, yyyy @ hh:mm')}">
                          <xsl:value-of select="uForum:TimeDiff($topic/@created)" />
                        </strong>

                        <xsl:if test="Exslt.ExsltDatesAndTimes:seconds(Exslt.ExsltDatesAndTimes:difference($topic/created, $topic/updated)) &gt; 10">
                          <xsl:text>, this topic was edited at: </xsl:text>
                          <xsl:value-of select="umbraco.library:FormatDateTime($topic/updated, 'f')"/>
                        </xsl:if>

                        <xsl:if test="$topic/@answer != 0">
                          <a href="{uForum:NiceCommentUrl($topic/@id, $topic/@answer, $maxitems)}" class="solution">
                            <xsl:text>, Go directly to the topic solution</xsl:text>
                          </a>
                        </xsl:if>
                      </div>
                    </div>

                    <xsl:if test="umbraco.library:IsLoggedOn()">
                      <div class="options">
                        <xsl:call-template name="topicOptions">
                          <xsl:with-param name="topic" select="$topic" />
                        </xsl:call-template>
                      </div>
                    </xsl:if>


                    <div class="body" style="clear: both">

                      <xsl:if test="$topic/isSpam = 'true'">
                        <div class="spamNotice">
                          <h3>
                            Sorry if we're causing you any inconvenience but this topic has been automatically marked as spam. A moderator has been notified and will evaluate the validity of your topic. When this topic has been marked as clean, this topic will be shown as normal.
                          </h3>

                          <xsl:if test="$isModerator = true()">
                            <h3>You can see this topic because you're a moderator of the forum. Only moderators and the topic starter can see this topic.</h3>
                            <p>As a moderator you can <a href="/ManageSpam?type=Topics">browse through topics marked as spam</a>.</p>
                          </xsl:if>

                          <xsl:if test="$mem = $topic/@memberId">
                            <h3>You can see this topic because you started it. Only moderators and the topic starter can see this topic.</h3>
                          </xsl:if>
                        </div>
                      </xsl:if>

                      <xsl:value-of select="uForum:Sanitize($topic/body)" disable-output-escaping="yes"/>

                      <!-- tags -->
                      <xsl:if test="count($topic/tags/tag) &gt; 0">
                        <xsl:text>Tags:</xsl:text>
                        <ul>
                          <xsl:for-each select="$topic/tags/tag">
                            <li>
                              <xsl:value-of select="."/>
                            </li>
                          </xsl:for-each>
                        </ul>
                      </xsl:if>

                    </div>

                  </div>

                  <div class="voting rounded">
                    <span>
                      <a href="#" class="history" rel="{$topicID},topic">
                        <xsl:value-of select="$topic/@score"/>
                      </a>
                    </span>

                    <xsl:if test="umbraco.library:IsLoggedOn()">
                      <xsl:if test="$mem!= $topic/@memberId">
                        <xsl:variable name="vote" select="uPowers:YourVote($mem, $topic/@id, 'powersTopic')"/>

                        <xsl:if test="$vote = 0">
                          <a href="#" class="LikeTopic vote" rel="{$topicID}">
                            <xsl:if test="boolean(not($canVote))">
                              <xsl:attribute name="class">noVote</xsl:attribute >
                            </xsl:if>
                            <xsl:text>High five!</xsl:text>
                          </a>
                        </xsl:if>

                      </xsl:if>
                    </xsl:if>
                  </div>

                  <br style="clear: both;"/>
                </li>

                <xsl:if test="$topic/replies &gt; 0">
                  <li class="replies">
                    <h2>Replies</h2>
                  </li>
                </xsl:if>

                <!-- Topic Start done -->

              </xsl:if>

              <!-- Replies -->
              <xsl:call-template name="commentsList">
                <xsl:with-param name="comments" select="uForum.raw:CommentsByDate($topicID, $maxitems, $p, 'ASC')"/>
                <xsl:with-param name="topic" select="$topic"/>
                <xsl:with-param name="mem" select="$mem"/>
                <xsl:with-param name="isAdmin" select="$isAdmin"/>
                <xsl:with-param name="isModerator" select="$isModerator"/>
                <xsl:with-param name="treshold" select="$treshold"/>
              </xsl:call-template>

              <xsl:if test="$mem &gt; 0">
                <li class="replies">
                  <h2>Post a reply</h2>
                </li>
              </xsl:if>

            </ul>

            <!-- Paging widget -->
            <xsl:if test="count($pages//page) &gt; 1">
              <strong>Pages: </strong>
              <ul id="searchPager" class="pager">
                <xsl:for-each select="$pages//page">
                  <li>
                    <xsl:if test="@current = 'true'">
                      <xsl:attribute name="class">current</xsl:attribute>
                    </xsl:if>
                    <a href="?p={@index}">
                      <xsl:value-of select="@index+1"/>
                    </a>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <h3>Our apologies for the inconvenience, this topic is currently not available.</h3>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="commentsList">
    <xsl:param name="comments"/>
    <xsl:param name="topic"/>
    <xsl:param name="mem"/>
    <xsl:param name="isAdmin"/>
    <xsl:param name="isModerator"/>
    <xsl:param name="treshold"/>

    <xsl:for-each select="$comments//comment" >



      <xsl:choose>
        <xsl:when test="score &lt; $treshold">
          <xsl:call-template name="collapsedcomment">
            <xsl:with-param name="comment" select="."/>
          </xsl:call-template>

          <xsl:call-template name="comment">
            <xsl:with-param name="comment" select="."/>
            <xsl:with-param name="topic" select="$topic"/>
            <xsl:with-param name="mem" select="$mem"/>
            <xsl:with-param name="isAdmin" select="$isAdmin"/>
            <xsl:with-param name="isModerator" select="$isModerator"/>
            <xsl:with-param name="collaps" select="true()"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:otherwise>
          <xsl:call-template name="comment">
            <xsl:with-param name="comment" select="."/>
            <xsl:with-param name="topic" select="$topic"/>
            <xsl:with-param name="mem" select="$mem"/>
            <xsl:with-param name="isAdmin" select="$isAdmin"/>
            <xsl:with-param name="isModerator" select="$isModerator"/>
            <xsl:with-param name="collaps" select="false()"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>


    </xsl:for-each>



  </xsl:template>

  <xsl:template name="comment">
    <xsl:param name="comment"/>
    <xsl:param name="topic"/>
    <xsl:param name="collaps"/>
    <xsl:param name="mem"/>
    <xsl:param name="isAdmin"/>
    <xsl:param name="isModerator"/>

    <!-- if the isSpam property is null, count will be zero, assume that it's not spam -->
    <xsl:variable name="canSeeComment" select="(count($comment/isSpam) = 0 or $comment/isSpam = 'false') or ($comment/isSpam = 'true' and $isModerator = 'true') or ($comment/isSpam = 'true' and $comment/memberId = $mem)" />
    
    <xsl:if test="$canSeeComment = true()">
      <xsl:variable name="author" select="umbraco.library:GetMember($comment/memberId)"/>
      
      <li class="post postComment" id="comment{$comment/id}">
        <xsl:if test="id = $topic/@answer">
          <xsl:attribute name="class">post postComment postSolution</xsl:attribute>
        </xsl:if>
        <xsl:if test="$collaps">
          <xsl:attribute name="class">post postComment hidden</xsl:attribute>
        </xsl:if>


        <div class="author vcard">
          <xsl:call-template name="badge">
            <xsl:with-param name="mem" select="$author" />
            <xsl:with-param name="date" select="$comment/created" />
          </xsl:call-template>

          <xsl:if test="id = $topic/@answer">
            <a name="solution" style="visibility: hidden">Solution</a>
          </xsl:if>
          <a name="comment{$comment/id}" style="visibility: hidden">
            <xsl:text>Comment with ID: </xsl:text>
            <xsl:value-of select="$comment/id"/>
          </a>
        </div>

        <div class="comment" id="comment{$comment/id}">

          <div class="meta">
            <div class="postedAt">
              <a href="/member/{$author//@id}">
                <xsl:value-of select="$author//@nodeName"/>
              </a>
              <xsl:text> posted this reply </xsl:text>
              <strong title="{umbraco.library:FormatDateTime($comment/created, 'MMMM d, yyyy @ hh:mm')}">
                <xsl:value-of select="uForum:TimeDiff($comment/created)" />
              </strong>
            </div>
          </div>

          <xsl:if test="umbraco.library:IsLoggedOn()">
            <div class="options">
              <xsl:call-template name="commentOptions">
                <xsl:with-param name="comment" select="$comment" />
                <xsl:with-param name="topic" select="$topic" />
              </xsl:call-template>
            </div>
          </xsl:if>

          <div class="body"  style="clear: both">
            
            <xsl:if test="./isSpam = 'true'">
              <div class="spamNotice">
                <h3>
                  Sorry if we're causing you any inconvenience but this comment has been automatically marked as spam. A moderator has been notified and will evaluate the validity of your comment. When this topic has been marked as clean, this topic will be shown as normal.
                </h3>

                <xsl:if test="$isModerator = true()">
                  <h3>You can see this comment because you're a moderator of the forum. Only moderators and the original commenter can see this comment.</h3>
                  <p>As a moderator you can <a href="/ManageSpam?type=Comments">browse through comments marked as spam</a>.</p>				  
                </xsl:if>

                <xsl:if test="$mem = ./memberId">
                  <h3>You can see this comment because you created it. Only moderators and the original commenter can see this comment.</h3>
                </xsl:if>
              </div>
            </xsl:if>

            <xsl:value-of select="uForum:Sanitize($comment/body)" disable-output-escaping="yes"/>
          </div>
        </div>


        <div class="voting rounded">
          <span>
            <a href="#" class="history" rel="{$comment/id},comment">
              <xsl:value-of select="$comment/score"/>
            </a>
          </span>
          <xsl:if test="umbraco.library:IsLoggedOn() and $mem != $comment/memberId">
            <xsl:variable name="vote" select="uPowers:YourVote($mem, $comment/id, 'powersComment')"/>
            <xsl:if test="$vote = 0">
              <a href="#" class="LikeComment vote"  rel="{$comment/id}" title="Mark this as a helpfull reply">
                <xsl:if test="boolean(not($canVote))">
                  <xsl:attribute name="class">noVote</xsl:attribute >
                </xsl:if>
                <xsl:text>High five!</xsl:text>
              </a>
            </xsl:if>
          </xsl:if>
        </div>

        <br style="clear: both"/>
      </li>

    </xsl:if>
  </xsl:template>

  <xsl:template name="collapsedcomment">
    <xsl:param name="comment"/>
    <li class="toggle" id="collapsedcomment{$comment/id}">
      <xsl:text>This comment by </xsl:text>
      <em>
        <xsl:value-of select="umbraco.library:GetMemberName($comment/memberId)"/>
      </em>
      <xsl:text> has a very low score and been hidden, </xsl:text>
      <a class="forumToggleComment" rel="comment{$comment/id}" href="#">Show it anyway</a>
    </li>
  </xsl:template>

  <xsl:template name="commentOptions">
    <xsl:param name="comment"/>
    <xsl:param name="topic"/>
    <ul>
      <xsl:if test="$topic/@answer = 0 and $topic/@memberId = $mem">
        <li>
          <a href="#" rel="{$comment/id}" title="Mark this reply as the solution" class="TopicSolver">
            <img src="/css/img/icons/tick.png" alt="Mark this reply as the solution"/>
          </a>
        </li>
      </xsl:if>

      <xsl:if test="$isAdmin = true()">
        <li class="admin">
          <a href="#" class="act delete DeleteComment kill" title="Delete this comment" rel="{$comment/id}">Delete</a>
        </li>
      </xsl:if>
      <xsl:if test="$isModerator = true() and $comment/isSpam = 'false'">
        <li class="admin">
          <a href="#" class="act markSpam MarkCommentAsSpam spam" title="Mark as spam" rel="{$comment/id}">Mark as spam</a>
        </li>
      </xsl:if>
      <xsl:if test="$isModerator = true() and $comment/isSpam = 'true'">
        <li class="admin">
          <a href="#" class="act markHam MarkCommentAsHam ham" title="Mark as ham" rel="{$comment/id}">Mark as ham</a>
        </li>
      </xsl:if>
      <xsl:if test="$isAdmin = true() or $comment/memberId = $mem">
        <li class="admin">
          <a href="/forum/EditReply?id={$comment/id}" class="act edit kill" title="Edit this reply">Edit</a>
        </li>
      </xsl:if>
    </ul>
  </xsl:template>

  <xsl:template name="topicOptions">
    <xsl:param name="topic"/>

    <ul style="width: 100%;">

      <xsl:if test="umbraco.library:IsLoggedOn()">
        <xsl:variable name="subscribed" select="Notifications:IsSubscribedToForumTopic($topic/@id, $mem)" />

        <li>
          <a href="#" class="act subscribe UnSubscribeTopic" title="Unsubscribe from this topic" rel="{$topic/@id}">
            <xsl:if test="not($subscribed)">
              <xsl:attribute name="style">
                <xsl:text>display:none;</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:text>Unsubscribe</xsl:text>
          </a>

          <a href="#" class="act subscribe SubscribeTopic" title="Subscribe to this topic" rel="{$topic/@id}">
            <xsl:if test="$subscribed">
              <xsl:attribute name="style">
                <xsl:text>display:none;</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:text>Subscribe</xsl:text>
          </a>
        </li>
      </xsl:if>

      <xsl:if test="$isAdmin = true() or ($mem = $topic/@memberId and $topic/@replies &lt; 1)">
        <li>
          <a href="/forum/EditTopic?id={$topic/@id}" class="act edit kill" title="Edit this topic">Edit</a>
        </li>
        <li>
          <a href="#" class="act delete DeleteTopic kill" title="Delete this topic" rel="{$topic/@id}">Delete</a>
        </li>
      </xsl:if>

      <xsl:if test="$isAdmin = true()">

        <xsl:if test="$isModerator = true() and $topic/isSpam = 'false'">
          <li class="admin">
            <a href="#" class="act markSpam MarkTopicAsSpam spam" title="Mark as spam" rel="{$topic/@id}">Mark as spam</a>
          </li>
        </xsl:if>
        
        <xsl:if test="$isModerator = true() and $topic/isSpam = 'true'">
          <li class="admin">
            <a href="#" class="act markHam MarkTopicAsHam ham" title="Mark as ham" rel="{$topic/@id}">Mark as ham</a>
          </li>
        </xsl:if>
        
        
        <li>
          <a href="#" onclick="jQuery('#moveList').toggle(); return false;" class="act move ToggleMoveList" id="MoveTopic" title="Move this topic" rel="{$topic/@id}">Move</a>

          <ol id="moveList">
            <li class="close">
              <a href="#" onclick="jQuery('#moveList').toggle(); return false;" class="ToggleMoveList">Close</a>
            </li>
            <xsl:for-each select="umbraco.library:GetXmlNodeById(1053)/* [@isDoc] | umbraco.library:GetXmlNodeById(1113)//Project">
              <xsl:if test="count(./* [@isDoc]) &gt; 0">
                <li>
                  <strong>
                    <xsl:value-of select="@nodeName" />
                  </strong>
                </li>
                <xsl:for-each select="./* [@isDoc]">
                  <li>
                    <a href="#" rel="{@id}" class="MoveToForum">
                      <xsl:value-of select="@nodeName" />
                    </a>
                  </li>
                </xsl:for-each>
              </xsl:if>
            </xsl:for-each>
          </ol>

        </li>

      </xsl:if>
    </ul>

  </xsl:template>

  <xsl:template name="badge">
    <xsl:param name="mem"/>
    <xsl:param name="date"/>

    <xsl:variable name="forumPosts">
      <xsl:choose>
        <xsl:when test="$mem//forumPosts">
          <xsl:value-of select="$mem//forumPosts"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$mem//data [@alias = 'forumPosts']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="reputationCurrent">
      <xsl:choose>
        <xsl:when test="$mem//reputationCurrent">
          <xsl:value-of select="$mem//reputationCurrent"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$mem//data [@alias = 'reputationCurrent']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="groups">
      <xsl:choose>
        <xsl:when test="$mem//groups">
          <xsl:value-of select="$mem//groups"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$mem//data [@alias = 'groups']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <div class="memberBadge rounded">
      <a href="/member/{$mem//@id}">
        <img alt="Avatar" class="photo" src="/media/avatar/{$mem//@id}.jpg" style="width: 32px; height: 32px;" />
      </a>
      <span class="posts">
        <xsl:value-of select="$forumPosts"/>
        <small>posts</small>
      </span>
      <span class="karma">
        <xsl:value-of select="$reputationCurrent"/>
        <small>karma</small>
      </span>
    </div>

    <xsl:if test="string-length($groups) > 0">
      <xsl:variable name="items" select="umbraco.library:Split($groups,',')" />
      <xsl:for-each select="$items//value">
        <xsl:choose>
          <xsl:when test=". = '2011-mvp-candidate'">
            <a href="/umbracomvp2011" title="Umbraco: Most Valuable Person 2011 Candidate" class="badge mvpcandidate">MVP</a>
          </xsl:when>

          <xsl:when test=". = 'HQ'">
            <a href="/wiki/about/umbraco-corporation" title="Umbraco HQ Employee" class="badge hq">HQ</a>
          </xsl:when>

          <xsl:when test=". ='admin'">
            <a href="/wiki/about/our-admins" title="Member of the our.umbraco.com admin team" class="badge admin">admin</a>
          </xsl:when>

          <xsl:when test=". = 'vendor'">
            <a href="/wiki/about/vendors" title="Umbraco Shop Vendor" class="badge vendor">Vendor</a>
          </xsl:when>

          <xsl:when test=".='Core'">
            <a href="/wiki/about/core-team" title="Member of the umbraco core team" class="badge core">Core</a>
          </xsl:when>

          <xsl:when test=".='CoreContrib'">
            <a href="/wiki/about/core-contributor" title="Member of the umbraco core contribution team" class="badge core-contrib">Core</a>
          </xsl:when>

          <xsl:when test=".= 'MVP'">            
            <a href="/wiki/about/mvps" title="Umbraco: Most Valuable Person" class="badge mvp">MVP</a>
          </xsl:when>
        </xsl:choose>                
      </xsl:for-each>
    </xsl:if>

  </xsl:template>

</xsl:stylesheet>