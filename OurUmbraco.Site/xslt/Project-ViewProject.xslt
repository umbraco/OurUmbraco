<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
    <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:our.library="urn:our.library"
  xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator"
  exclude-result-prefixes="our.library umbracoTags.library msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">



    <xsl:output method="html" omit-xml-declaration="yes"/>

    <xsl:param name="currentPage"/>

    <xsl:template match="/">

        <xsl:variable name="mem">
            <xsl:if test="umbraco.library:IsLoggedOn()">
                <xsl:value-of select="uForum:GetCurrentMember()/@id"/>
            </xsl:if>
        </xsl:variable>

        <xsl:variable name="isAdmin" select="uForum:IsInGroup('admin')"/>
        <xsl:variable name="files" select="uWiki:GetAttachedFiles($currentPage/@id)" />
        <xsl:variable name="owner" select="umbraco.library:GetMember( number($currentPage/owner) )"/>
        <xsl:variable name="canVote" select="boolean( number(uForum:GetCurrentMember()/reputationCurrent) &gt; 25 )"/>
        
        <div id="project">
          
            <div id="projectOwner">
                <xsl:call-template name="badge"><xsl:with-param name="mem" select="$owner"/></xsl:call-template>
            </div>

  
          <div id="projectDescription" style="width: 900px">
                <h1>
                    <xsl:value-of select="$currentPage/@nodeName"/>
                </h1>

                <div class="options">
                  
                    <xsl:if test="umbraco.library:IsLoggedOn() and ($mem = $currentPage/owner or our.library:IsProjectContributor($mem,$currentPage/@id))">
                      <a href="/member/profile/projects/edit?id={$currentPage/@id}" style="float:left">
                          Edit
                      </a>
                    </xsl:if>
                  
                    <a href="/member/{$owner/@id}">
                        <xsl:value-of select="$owner/@nodeName"/>
                    </a>
                    started this project on <xsl:value-of select="umbraco.library:ShortDate($currentPage/@createDate)"/>,
                    it's current version is <strong>
                        <xsl:value-of select="$currentPage/version"/>
                    </strong>.
                    <div>
                    </div>
                </div>
          
        <div style="float: right; padding: 10px 0px 30px 40px; width: 250px;" id="projectMeta">
            <xsl:if test="string($currentPage/file)">
              <xsl:variable name="release" select="uWiki:GetAttachedFile($currentPage/file)"/>  
              
              <a href="/FileDownload?id={$currentPage/file}&amp;release=1" class="projectDownload" >
                    Download
                    <span>
                        <xsl:value-of select="$currentPage/@nodeName"/>,  <xsl:value-of select="$currentPage/version"/>
                        <br/>
                                      <strong>Compatible with: 
                                        <xsl:choose>
                                          <xsl:when test="$release//umbracoVersion = 'v4'">Version 4.0.x schema</xsl:when>
                                          <xsl:when test="$release//umbracoVersion = 'v45'">Version 4.5.x with the new schema</xsl:when>
                                          <xsl:when test="$release//umbracoVersion = 'v3'">Version 3.x</xsl:when>
                                          <xsl:when test="$release//umbracoVersion = 'nan'">Not version dependant</xsl:when>
                                        </xsl:choose>
                                      </strong> 
                      
                    </span>
                  
                </a>
              
                
            </xsl:if>

            <xsl:if test="string($currentPage/vendorUrl)">
                <a href="{$currentPage/vendorUrl}" class="projectPurchase" target="_blank" rel="noreferrer noopener" onClick="javascript: pageTracker._trackPageview('/purchase/{$currentPage/@urlName}');">
                    Purchase
                    <span>
                        <xsl:value-of select="$currentPage/@nodeName"/>,  <xsl:value-of select="$currentPage/version"/>
                    </span>
                </a>

            </xsl:if>

            <xsl:if test="string($currentPage/openForCollab) = '1'">
               <div class="openForCollab">
                  Open for collaboration
                 <span>Want to get involved? Get in <a href="/member/send-collab-request?id={$currentPage/@id}">touch</a> with the project owner.</span>
               </div>
            </xsl:if>
            <div class="box" id="projectSummary">

                <h4>Project Summary</h4>
                <dl class="projetProps summary">
                    <dt>Project owner:</dt>
                    <dd>
                        <a href="/member/{$currentPage/owner}">
                            <xsl:value-of select="umbraco.library:GetMemberName($currentPage/owner)"/>
                        </a>
                    </dd>
                     <xsl:variable name="contri" select="our.library:ProjectContributors($currentPage/@id)" />
  
                    <xsl:if test="count($contri//memberId) &gt; 0">
                      <dt>Contributors:</dt>
                      <dd>
                      
                        <xsl:for-each select="$contri//memberId">
                          <xsl:sort select="umbraco.library:GetMemberName(.)" />
                          
                            <a href="/member/{.}"><xsl:value-of select="umbraco.library:GetMemberName(.)"/></a>
                          <xsl:if test="not(position() = last())">
                          <br/>
                          </xsl:if>
                        </xsl:for-each>
                      
                      </dd>
                    </xsl:if>
                  
                    <dt>Created:</dt>
                    <dd>
                        <xsl:value-of select="umbraco.library:ShortDate($currentPage/@createDate)"/>
                    </dd>


                    <xsl:if test="$currentPage/stable = '1'">
                        <dt>Is Stable:</dt>
                        <dd>
                            <span class="green">Project is stable</span>
                        </dd>
                    </xsl:if>

                    <xsl:if test="string($currentPage/version)">
                        <dt>Current version</dt>
                        <dd>
                            <xsl:value-of select="$currentPage/version"/>
                        </dd>
                    </xsl:if>

                    <xsl:if test="string($currentPage/licenseName)">
                        <dt>License</dt>
                        <dd>
                            <a href="{$currentPage/licenseUrl}">
                                <xsl:value-of select="$currentPage/licenseName"/>
                            </a>
                        </dd>
                    </xsl:if>

                    <xsl:if test="count(umbracoTags.library:getTagsFromNode($currentPage/@id)//tag) &gt; 0">
                        <dt>Tags</dt>
                        <dd>
                            <xsl:for-each select="umbracoTags.library:getTagsFromNode($currentPage/@id)//tag">
                                <a href="/projects/tag/{.}">
                                    <xsl:value-of select="."/>
                                </a>&nbsp;
                            </xsl:for-each>
                        </dd>
                    </xsl:if>

                  
                    <xsl:if test="string($currentPage/file)">
                        <dt>Downloads:</dt>
                        <dd>
                            <xsl:value-of select="our.library:GetProjectTotalDownloadCount($currentPage/@id)"/>

                           
                        </dd>
                    </xsl:if>

                </dl>
            </div>


            <xsl:if test="umbraco.library:IsLoggedOn() and our.library:IsAdmin($mem)">

                <div class="box">

                    <h4>Edit Project Tags</h4>
                    <p style="padding-left:10px;">
                        <input class="tagger" type="text" name="projecttags[]" id="projecttagger"/>
                    </p>

                    <div style="clear:both;"></div>

                </div>

                <script type="text/javascript">
                    enableTagger(<xsl:value-of select="$currentPage/@id"/>);
                    $('#projecttagger').autocomplete(<xsl:value-of select="our.library:GetAllTags('project')"/>,{max: 8,scroll: true,scrollHeight: 300});

                    <xsl:for-each select="umbracoTags.library:getTagsFromNode($currentPage/@id)//tag">
                        $('#projecttagger').addTag('<xsl:value-of select="."/>',<xsl:value-of select="$currentPage/@id"/>);
                    </xsl:for-each>


                </script>

            </xsl:if>

        </div>


  <div style="width: 620px" id="projectBody">
    
            <div id="description">
            <xsl:choose>
                <xsl:when test="contains($currentPage/description, '&lt;')">
                    <!--<xsl:value-of select="uForum:ResolveLinks( uForum:Sanitize( $currentPage/data [@alias = 'description'] ))" disable-output-escaping="yes"/>-->
                    <xsl:value-of select="uForum:ResolveLinks( $currentPage/description )" disable-output-escaping="yes"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="uForum:ResolveLinks( umbraco.library:ReplaceLineBreaks($currentPage/description))" disable-output-escaping="yes"/>
                </xsl:otherwise>
            </xsl:choose>
    </div>

            <xsl:if test="count($files//file [type = 'screenshot'])">
                <div class="divider" style="clear:left;"></div>
                <div id="screenshots">
                    <h3>Screenshots</h3>
                    <xsl:for-each select="$files//file [type = 'screenshot']">
                        <a href="{path}" class="projectscreenshot" rel="shadowbox">
                            <img src="/umbraco/imagegen.aspx?image={path}&amp;pad=true&amp;width=100&amp;height=100" style="border:0;"/>
                        </a>
                    </xsl:for-each>
                </div>
            </xsl:if>

            <div class="divider" style="clear:left;"></div>

    <div id="resources">
            <xsl:if test="string($currentPage/demoUrl) or string($currentPage/sourceUrl) or string($currentPage/websiteUrl)">
                <h3>Resources</h3>
                <ul>
                    <xsl:if test="string($currentPage/demoUrl)">
                        <li>
                          <a href="{$currentPage/demoUrl}">Watch a demonstration</a>
                        </li>
                    </xsl:if>
                    <xsl:if test="string($currentPage/sourceUrl)">
                        <li>
                            <a href="{$currentPage/sourceUrl}">Download the source code</a>
                        </li>
                    </xsl:if>
                    <xsl:if test="string($currentPage/websiteUrl)">
                        <li>
                            <a href="{$currentPage/websiteUrl}">Project website</a>
                        </li>
                    </xsl:if>
                </ul>

                <div class="divider" style="clear:left;"></div>

            </xsl:if>
    </div>
    
    
    <div id="files">
            <xsl:if test="count($files//file)">
                <h3>Attached files</h3>
                <ul class="attachedFiles">
                    <xsl:for-each select="$files//file [archived = 'false']">
                        <xsl:if test="boolean(./current)">
                            <xsl:if test="type != 'screenshot'">
                                <li class="{type}">
                                    <a class="fileName" href="/FileDownload?id={id}">
                                        <xsl:value-of select="name" />
                                    </a>
                                    <small>
                                        uploaded <xsl:value-of select="umbraco.library:ShortDate(createDate)"/>
                                        by <a href="/member/{createdBy}"> <xsl:value-of select="umbraco.library:GetMemberName(createdBy)"/>
                                        </a>
                                      <br/>
                                      <strong>Compatible with: 
                                        <xsl:choose>
                                          <xsl:when test="umbracoVersion = 'v4'">Version 4.0.x schema</xsl:when>
                                          <xsl:when test="umbracoVersion = 'v45'">Version 4.5.x with the new schema</xsl:when>
                                          <xsl:when test="umbracoVersion = 'v3'">Version 3.x</xsl:when>
                                          <xsl:when test="umbracoVersion = 'nan'">Not version dependant</xsl:when>
                                        </xsl:choose>
                                      </strong>
                                      
                                      <xsl:if test="$isAdmin and type = 'package'">
                                      &nbsp;
                                         <xsl:if test="verified = 'false'">
                                           <div class="verifyWikiFile" rel="{id}">Mark as verified</div>
                                        </xsl:if>
                                      </xsl:if>
                                      
                                    </small>
                                </li>
                            </xsl:if>
                        </xsl:if>
                    </xsl:for-each>
                </ul>
              
    <xsl:if test="count ($files//file [archived = 'true' and type != 'screenshot']) &gt; 0">
    <h4><a href="#fileArchive" id="fileArchiveLink">Archived files (<xsl:value-of select="count ($files//file [archived = 'true' and type != 'screenshot'])" />)</a></h4>
    <div id="fileArchive" style="display:none">
                <ul class="attachedFiles">
                    <xsl:for-each select="$files//file [archived = 'true' and type != 'screenshot']">
                        <xsl:if test="boolean(./current)">
                           
                                <li class="{type}">
                                    <a class="fileName" href="/FileDownload?id={id}">
                                        <xsl:value-of select="name" />
                                    </a>
                                    <small>
                                        <xsl:value-of select="umbraco.library:GetDictionaryItem(type)"/>
                                        - uploaded <xsl:value-of select="umbraco.library:ShortDate(createDate)"/>
                                        by <a href="/member/{createdBy}">
                                            <xsl:value-of select="umbraco.library:GetMemberName(createdBy)"/>
                                        </a>
                                    </small>
                                </li>
                         
                        </xsl:if>
                    </xsl:for-each>
                </ul>
    </div>
    </xsl:if>
              
              
                <div class="divider" style="clear:left;"></div>
            </xsl:if>
    </div>
    
  </div>
  </div>

  <div id="projectvoting" class="voting rounded" style="width: 60px">
    <xsl:variable name="score" select="uPowers:Score($currentPage/@id, 'powersProject')"/>
                <span>
                    <a href="#" class="history" rel="{$currentPage/@id},project">
                      <xsl:value-of select="$score"/>
                    </a>
                </span>
    <xsl:if test="umbraco.library:IsLoggedOn() and $mem != $currentPage/owner and not(our.library:IsProjectContributor($mem,$currentPage/@id))">

                    <xsl:variable name="vote" select="uPowers:YourVote($mem, $currentPage/@id, 'powersProject')"/>
                        <xsl:if test="$vote = 0">
                            <a href="#" class="ProjectUp vote"  rel="{$currentPage/@id}" title="Reward this project with karma points">
                              <xsl:if test="boolean(not($canVote))">
                                <xsl:attribute name="class">noVote</xsl:attribute >
                              </xsl:if>
                               Reward
                            </a>
                          
                          <xsl:if test="$isAdmin and number($score) &lt; 15">
                            <br/>
                              <a href="#" class="ProjectApproval vote"  rel="{$currentPage/@id}" title="Approve this for the umbraco repository">APPROVE</a>
                          </xsl:if>
                          
                        </xsl:if>
     

                </xsl:if>
            </div>
        </div>

        </xsl:template>
  
<xsl:template name="badge">
<xsl:param name="mem"/>

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
    <span class="posts"><xsl:value-of select="$forumPosts"/><small>posts</small></span>  
    <span class="karma"><xsl:value-of select="$reputationCurrent"/><small>karma</small></span>
</div>

<xsl:choose>

  
<xsl:when test="contains($groups, '2011-mvp-candidate')">
<a href="/umbracomvp2011" title="Umbraco: Most Valuable Person 2011 Candidate" class="badge mvpcandidate">MVP</a>
</xsl:when>
  
<xsl:when test="contains($groups, 'HQ')">
  <a href="/wiki/about/umbraco-corporation" title="Umbraco HQ Employee" class="badge hq">HQ</a>
</xsl:when>

<xsl:when test="contains($groups, 'admin')">
  <a href="/wiki/about/our-admins" title="Member of the our.umbraco.com admin team" class="badge admin">admin</a>
</xsl:when>

<xsl:when test="contains($groups, 'vendor')">
  <a href="/wiki/about/vendors" title="Umbraco Shop Vendor" class="badge vendor">Vendor</a>
</xsl:when>

<xsl:when test="contains($groups, 'Core')">
  <a href="/wiki/about/core-team" title="Member of the umbraco core team" class="badge core">Core</a>
</xsl:when>

<xsl:when test="contains($groups, 'MVP')">
  <a href="/wiki/about/mvps" title="Umbraco: Most Valuable Person" class="badge mvp">MVP</a>
</xsl:when>

</xsl:choose>

</xsl:template>
</xsl:stylesheet>