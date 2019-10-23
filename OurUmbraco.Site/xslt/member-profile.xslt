<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
    <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:our.library="urn:our.library"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki"
  exclude-result-prefixes="msxml our.library umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki ">


  <xsl:output method="xml" omit-xml-declaration="yes"/>
  <xsl:param name="currentPage"/>

  <xsl:variable name="memId" select="umbraco.library:GetHttpItem('umbMemberLogin')"/>

  <!-- Signed in Member -->
  <xsl:variable name="isAdmin" select="uForum:IsInGroup('admin')"/>
  <xsl:variable name="isHQ" select="uForum:IsInGroup('HQ')"/>
  <xsl:variable name="mem">
    <xsl:if test="umbraco.library:IsLoggedOn()">
      <xsl:value-of select="uForum:GetCurrentMember()/@id"/>
    </xsl:if>
  </xsl:variable>

  <xsl:template match="/">
      
      <xsl:if test="$memId != ''">
        <xsl:variable name="mem" select="umbraco.library:GetMember(number($memId))"/>
        
        <div id="memberProfile">
          <div id="avatar">
            <xsl:call-template name="badge">
              <xsl:with-param name="mem" select="$mem" />
            </xsl:call-template>
          </div>
          
          <div id="details">
            <h1><xsl:value-of select="$mem/node/@nodeName"/></h1>

            <xsl:if test="string-length($mem//company) &gt; 1">
              <h2>Company: <xsl:value-of select="$mem//company"/></h2>
            </xsl:if>
            
            <xsl:variable name="twittr" select="$mem//twitter"/>
            <xsl:if test="string-length($twittr) &gt; 1">
              <h2>
                Twitter:
                <a href="http://twitter.com/{$twittr}">
                  <xsl:value-of select="$twittr"/>
                </a>
              </h2>
            </xsl:if>
            
            <xsl:variable name="location" select="$mem//location" />
            <xsl:if test="string-length($location) &gt; 1">
              <h2>Location: <xsl:value-of select="$location"/></h2>
            </xsl:if>
            
            <p style="clear: both; margin-right: 100px;">
              <xsl:value-of select="$mem//profileText" disable-output-escaping="yes"/>
            </p>
            
            <!-- Block Member Options - For Admin group members only -->
            <xsl:if test="umbraco.library:IsLoggedOn()">
              <xsl:if test="$isAdmin = true()">
                
                <xsl:variable name="memberBlocked" select="$mem//blocked" />

                <div class="options">
                  <ul>
                    <li>
                      <xsl:if test="$memberBlocked = 1">
                        <xsl:attribute name="style">
                          <xsl:text>display:none;</xsl:text>
                        </xsl:attribute>
                      </xsl:if>

                      <a href="#" class="act blockMember" title="Block Member" rel="{$memId}">Block Member</a>
                    </li>
                    <li>
                      <xsl:if test="not($memberBlocked = 1)">
                        <xsl:attribute name="style">
                          <xsl:text>display:none;</xsl:text>
                        </xsl:attribute>
                      </xsl:if>

                      <a href="#" class="act unblockMember" title="Unblock Member" rel="{$memId}">Unblock Member</a>
                    </li>
                  </ul>
                </div>
              </xsl:if>
              
              <xsl:if test="$isHQ = true()">
                <div class="options">
                  <ul>
                    <li>
                      <a href="#" class="act deleteMember" title="Delete Member" rel="{$memId}">Delete Member</a>
                    </li>
                  </ul>
                </div>
              </xsl:if>
            </xsl:if>
            
            
            <xsl:variable name="memberTopics" select="uForum.raw:TopicsWithAuthor(number($memId))"/>
            <div style="overflow: hidden; width: 390px; float: left; clear: none;">
              <div class="box">
                <h4>Recent Posts</h4>
                <ul class="summary">
                  <xsl:choose>
                    <xsl:when test="count($memberTopics) &gt; 0">
                      <xsl:for-each select="$memberTopics//topic">
                        <xsl:sort select="updated" order="descending"/>
                        
                        <!-- SHOW upto 10 Recent posts -->
                        <xsl:if test="position() &lt;= 10">
                          <li>
                            <a href="{uForum:NiceTopicUrl(id)}"><xsl:value-of select="title"/></a>
                            <small><xsl:value-of select="uForum:TimeDiff(updated)"/></small>
                          </li>
                        </xsl:if>
                      </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                      <li>This user has yet to make a post.</li>
                    </xsl:otherwise>
                  </xsl:choose>
                </ul>
              </div>
            </div>
            
            <div style="overflow: hidden; width: 390px; float: right; clear: none;">
              <xsl:variable name="projects" select="$currentPage/ancestor-or-self::Community/Projects//Project" />
              
              <xsl:if test="count($projects [owner = $memId and projectLive = 1]) &gt; 0">
                <div class="box">
                  <h4>Projects</h4> 
                  <ul class="projects summary">
                    <xsl:for-each select="$projects">
                      <xsl:sort select="@nodeName"/>
                      
                      <!-- Check to see if Owner of project -->
                      <xsl:if test="owner = $memId and projectLive = 1">
                        <li>
                          <a href="{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="@nodeName"/></a>
                          <xsl:if test="version">
                            <small>Version: <xsl:value-of select="version"/></small>
                          </xsl:if>
                        </li>
                      </xsl:if>
                    </xsl:for-each>
                  </ul>
                </div>
              </xsl:if>
              
              <xsl:variable name="contripr" select="our.library:ProjectsContributing(number($memId))" />
              <xsl:if test="count($contripr//projectId) &gt; 0">
                <div class="box">
                  <h4>Contributes to</h4>
                  <ul class="projects summary">
                    <xsl:for-each select="$contripr//projectId">
                      <xsl:sort select="umbraco.library:GetXmlNodeById(.)/@nodeName" />
                      <xsl:variable name="project" select="umbraco.library:GetXmlNodeById(.)" />
                      <li>
                        <a href="{umbraco.library:NiceUrl(.)}"><xsl:value-of select="$project/@nodeName"/></a>
                        <xsl:if test="$project/version">
                          <small>Version: <xsl:value-of select="$project/version"/></small>
                        </xsl:if>
                      </li>
                    </xsl:for-each>
                  </ul>
                </div>
              </xsl:if>
            </div>
            
          </div>
        </div>
        
        <!-- DEBUG -->
        <xsl:if test="umbraco.library:RequestQueryString('dingo') = 'meh'">
          <br style="clear: both;"/>
          
          <xsl:variable name="karma"              select="umbraco.library:GetXmlDocument( concat('/upowers/',$memId,'.xml'), true())"/>
          <xsl:variable name="topicsReceived"     select="sum($karma/karma/summaries/summary [alias = 'topic']/received)"/>
          <xsl:variable name="topicsPerformed"    select="sum($karma/karma/summaries/summary [alias = 'topic']/performed)"/>
          <xsl:variable name="commentsReceived"   select="sum($karma/karma/summaries/summary [alias = 'comment']/received)"/>
          <xsl:variable name="commentsPerformed"  select="sum($karma/karma/summaries/summary [alias = 'comment']/performed)"/>
          <xsl:variable name="projectsReceived"   select="sum($karma/karma/summaries/summary [alias = 'project']/received)"/>
          <xsl:variable name="projectsPerformed"  select="sum($karma/karma/summaries/summary [alias = 'project']/performed)"/>
          <xsl:variable name="wikiPerformed"      select="sum($karma/karma/summaries/summary [alias = 'wiki']/performed)"/>
          
          <div class="box">
            <h4>Karma history</h4>
            <div style="padding: 10px;">
              <h3>Forum topics</h3>
              <ul>
                <li><xsl:value-of select="$topicsPerformed" /> points earned for starting conversations</li>
                <li><xsl:value-of select="$topicsReceived" /> points rewarded for posting helpfull topics</li>
              </ul>
              
              <h3>Forum Comments</h3>
              <ul>
                <li><xsl:value-of select="$commentsReceived" /> points rewarded for posting helpfull comments</li>
                <li><xsl:value-of select="$commentsPerformed" /> points earned by giving other replies karma</li>
              </ul>
              
              <h3>Projects</h3>
              <ul>
                <li><xsl:value-of select="$projectsReceived" /> rewarded for quality projects</li>
                <li><xsl:value-of select="$projectsPerformed" /> giving projects karma</li>
              </ul>
              
              <h3>Wiki</h3>
              <ul>
                <li><xsl:value-of select="$wikiPerformed" /> points rewarded for starting wiki pages</li>
              </ul>
            </div>
          </div>
        </xsl:if>
        
      </xsl:if>
  
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
    <img alt="Avatar" class="photo" src="/media/avatar/{$memId}.jpg" style="width: 32px; height: 32px;" />
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