<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ProjectView.ascx.cs"
    Inherits="Marketplace.usercontrols.Deli.ProjectView" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<div id="project">
    <div id="projectVotes">
        <div id="projectvoting" class="voting rounded" style="width: 60px">
            <span><a href="#" class="history" rel="<%= Project.Id %>,project">
                <%=Project.Karma %>
            </a></span>
        </div>
        <asp:PlaceHolder runat="server" ID="VotingOptions"><a href="#" id="addVote" class="ProjectUp vote<%= IsVoteable() %>"
                rel="<%= Project.Id %>" title="Reward this project with karma points">Add Vote </a>
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="AdminVotingOptions"><a href="#" id="projectApprove" class="ProjectApproval vote"
                rel="<%= Project.Id %>" title="Approve this for the umbraco repository">
                APPROVE</a> </asp:PlaceHolder>
    </div>
    <div id="projectDescription">
        <div class="options">
            <asp:PlaceHolder runat="server" ID="EditOption"><a href="/member/profile/projects/edit?id=<%= Project.Id %>"
                style="float: left">Edit </a></asp:PlaceHolder>
            <asp:HyperLink runat="server" ID="VendorLink" />
            started this project on
            <asp:Literal runat="server" ID="ProjectCreateDate" />
            it's current version is <strong>
                <asp:Literal runat="server" ID="ProjectCurrentVersion" />
            </strong>.
        </div>
        <div id="projectSidebar">
            <asp:PlaceHolder runat="server" ID="DownloadPanel">
                <div id="projectDownload">
                    <% if (!String.IsNullOrEmpty(Project.GACode))
                       {  
                    %>
                    <a href="/FileDownload?id=<%= Project.CurrentReleaseFile %>&amp;release=1" class="downloadBtn"
                        onclick="vendorTracker._trackEvent('<%= "_" + Project.Name.ToLower().Replace(" ", "") %>', 'Download Button', '<%= Project.Name %>');">
                        Download </a>
                    <% 
                           }
                       else
                       {
                    %>
                    <a href="/FileDownload?id=<%= Project.CurrentReleaseFile %>&amp;release=1" class="downloadBtn">
                        <img src="/css/img/download-package.png" />
                    </a>
                    <%
                           } 
                    %>
                </div>
            </asp:PlaceHolder>
            <h2>
                Package Info</h2>
            <h3>
                Project Owner/Creator</h3>

                <div class="memberBadge rounded">
                    <a href="/member/<%= Project.Vendor.Member.Id %>">
                        <img alt="Avatar" class="photo" src="/media/avatar/<%= Project.Vendor.Member.Id %>.jpg" />
                    </a>
                    <h4><%= Project.Vendor.Member.Name %></h4>
                    <span class="posts">
                        <asp:Literal ID="MemberPosts" runat="server" /><small>posts</small>
                    </span> 
                    <span class="karma">
                            <asp:Literal ID="MemberKarma" runat="server" /><small>karma</small>
                    </span>
                </div>
            <asp:PlaceHolder runat="server" ID="CommercePanel">
                <div class="projectPurchase">
                    <h4>
                        Purchase a License</h4>
                    <ul>
                        <asp:Repeater runat="server" ID="ProjectPurchaseRepeater" OnItemDataBound="LicenseBound">
                            <ItemTemplate>
                                <li><strong>
                                    <asp:Literal runat="server" ID="LicenseType" /></strong> <span class="money">
                                        <asp:Literal runat="server" ID="LicensePrice" /></span>
                                    <asp:ImageButton runat="server" ID="LicenseAddToCart" Text="Add" OnClick="_addToCart_Click"
                                        ImageUrl="/css/img/addtocart.png" CssClass="addToCart" />
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </asp:PlaceHolder>
            <div id="projectCompat" class="sideSection">
                <h3>
                    Project Compatibility</h3>
                <p>
                    <asp:PlaceHolder runat="server" ID="HasReports">
                    <span class="compatSummary">
                        <asp:Literal runat="server" ID="ProjectCompatitbleWithUmbraco" /><br />
                        <br />
                    </span>
                    <span class="compatDetails" style="display:none;">
                        <span id="compatAjaxDetails">Please wait loading...</span>
                        <asp:Literal runat="server" ID="ProjectCompatibileDetails" />
                        <br />
                        <br />
                    </span>
                    <span id="compatLoading" style="display:none;">Retrieving details...</br></span>
                    <a href="#" rel="<%= Project.CurrentReleaseFile %>,<%= Project.Id %>" class="viewFullCompatibilityDetails">View Details</a>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder runat="server" ID="ReportVersionOptions">
                    <a href="#" rel="<%= Project.CurrentReleaseFile %>,<%= Project.Id %>" class="compatibilityReport">Report Compatibility</a>
                    </asp:PlaceHolder>
                </p>
            </div>
            <div class="sideSection">
                <h4>
                    Project Information</h4>
                <dl class="projetProps summary">
                    <dt>Project owner:</dt>
                    <dd>
                        <a href="/member/<%= Project.Vendor.Member.Id %>">
                            <%= Project.Vendor.Member.Name %>
                        </a>
                    </dd>
                    <asp:Repeater runat="server" ID="ContribRepeater">
                        <HeaderTemplate>
                            <dt>Contributors:</dt>
                            <dd>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <a href="/member/<%# Eval("Id") %>">
                                <%# Eval("Text") %></a>&nbsp;
                        </ItemTemplate>
                        <FooterTemplate>
                            </dd>
                        </FooterTemplate>
                    </asp:Repeater>
                    <dt>Created:</dt>
                    <dd>
                        <%= Project.CreateDate.ToString("D") %>
                    </dd>
                    <% if (Project.Stable)
                       { %>
                    <dt>Is Stable:</dt>
                    <dd>
                        <span class="green">Project is stable</span>
                    </dd>
                    <%} %>
                    <dt>Current version</dt>
                    <dd>
                        <%= Project.CurrentVersion %>
                    </dd>
                    <% if (!string.IsNullOrEmpty(Project.LicenseName))
                       {  %>
                    <dt>License</dt>
                    <dd>
                        <a href="<%= Project.LicenseUrl %>">
                            <%= Project.LicenseName %></a>
                    </dd>
                    <% } %>
                    <asp:Repeater runat="server" ID="TagRepeater">
                        <HeaderTemplate>
                            <dt>Tags</dt>
                            <dd>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <a href="/projects/tag/<%# Eval("Text") %>">
                                <%# Eval("Text") %></a>&nbsp;
                        </ItemTemplate>
                        <FooterTemplate>
                            </dd></FooterTemplate>
                    </asp:Repeater>
                    <dt>Downloads:</dt>
                    <dd>
                        <asp:Literal runat="server" ID="DownloadCount" />
                    </dd>
                </dl>
            </div>
            <asp:PlaceHolder ID="TagEditPanel" runat="server">
                <div class="sideSection">
                    <h4>
                        Edit Project Tags</h4>
                    <p style="padding-left: 10px;">
                        <input class="tagger" type="text" name="projecttags[]" id="projecttagger" />
                    </p>
                    <div style="clear: both;">
                    </div>
                </div>
                <script type="text/javascript">
                        enableTagger(<%=Project.Id %>);
                        $('#projecttagger').autocomplete(['<asp:literal runat="server" id="TagStringArray"/>'],{max: 8,scroll: true,scrollHeight: 300});

                        <asp:repeater Id="TagEditRepeater" runat="server">
                        <itemtemplate>
                            $('#projecttagger').addTag('<%# Eval("Text") %>',<%=Project.Id %>);
                        </itemtemplate>
                        </asp:repeater>

                </script>
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="CollabPanel">
                <div class="openForCollab">
                    <h3>
                        Contribute</h3>
                    <img src="/images/group.png" alt="Group" /><p>
                        This package is open for collaboration.
                        <br />
                        <a href="/member/send-collab-request?id=<%=Project.Id %>">Contact the owner</a></p>
                </div>
            </asp:PlaceHolder>
        </div>
        <div id="projectDescriptionBody">
            <h1 class="projectName">
                <asp:Literal runat="server" ID="ProjectName" /></h1>
            <div id="projectDescriptionText">
                <asp:Literal runat="server" ID="ProjectDescrition" />
            </div>
            <asp:Repeater runat="server" ID="ScreenshotRepeater">
                <HeaderTemplate>
                    <div id="projectScreenshots">
                        <h3>
                            Screenshots</h3>
                </HeaderTemplate>
                <ItemTemplate>
                    <a href="<%# Eval("Path") %>" class="projectscreenshot" rel="shadowbox[gallery]">
                        <img src="/umbraco/imagegen.ashx?image=<%# Eval("Path") %>&amp;path=true&amp;width=100&amp;height=100"
                            style="border: 0;" />
                    </a>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
            <div id="tabs">
                <div class="projectOptions">
                    <ul id="tabNav">
                    </ul>
                </div>
                <asp:Repeater runat="server" ID="ProjectFileRepeater">
                    <HeaderTemplate>
                        <div id="projectFiles" class="tabContent">
                            <h3>
                                Package Files</h3>
                            <ul class="attachedFiles">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="<%# Eval("FileType") %>">
                            <% if (!String.IsNullOrEmpty(Project.GACode))
                               {  
                            %>
                            <a class="fileName" onclick="vendorTracker._trackEvent('<%= "_" + Project.Name.ToLower().Replace(" ", "") %>', 'Download', '<%= Project.Name %> Download Tab');"
                                href="/FileDownload?id=<%# Eval("Id") %>">
                                <%# Eval("Name")%>
                            </a>
                            <% 
                               }
                               else
                               {
                            %>
                            <a class="fileName" href="/FileDownload?id=<%# Eval("Id") %>">
                                <%# Eval("Name")%>
                            </a>
                            <%
                                } 
                            %>
                            <small>uploaded
                                <%# Eval("CreateDate") %>
                                by <a href="/member/<%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Id %>">
                                    <%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Text %></a>
                                <br />
                                <strong>
                                    <%# Eval("UmbracoVersion") %><br />
                                    .NET Version:
                                    <%# Eval("DotNetVersion") %><br />
                                    <%# Eval("MediumTrust") %>
                                </strong></small></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul> </div>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Repeater runat="server" ID="HotFixRepeater">
                    <HeaderTemplate>
                        <div id="hotfixFiles" class="tabContent">
                            <h3>
                                Hot fix / Upgrade Files</h3>
                            <ul class="attachedFiles">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="<%# Eval("FileType") %>">
                            <% if (!String.IsNullOrEmpty(Project.GACode))
                               {  
                            %>
                            <a class="fileName" onclick="vendorTracker._trackEvent('<%= "_" + Project.Name.ToLower().Replace(" ", "") %>', 'Download', '<%= Project.Name %> Download Tab');"
                                href="/FileDownload?id=<%# Eval("Id") %>">
                                <%# Eval("Name")%>
                            </a>
                            <% 
                               }
                               else
                               {
                            %>
                            <a class="fileName" href="/FileDownload?id=<%# Eval("Id") %>">
                                <%# Eval("Name")%>
                            </a>
                            <%
                                } 
                            %>
                            <small>uploaded
                                <%# Eval("CreateDate") %>
                                by <a href="/member/<%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Id %>">
                                    <%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Text %></a>
                                <br />
                                <strong>
                                    <%# Eval("UmbracoVersion") %><br />
                                    .NET Version:
                                    <%# Eval("DotNetVersion") %><br />
                                    <%# Eval("MediumTrust") %>
                                </strong></small></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul> </div>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Repeater runat="server" ID="SourceRepeater">
                    <HeaderTemplate>
                        <div id="projectSource" class="tabContent">
                            <h3>
                                Source Code</h3>
                            <ul class="attachedFiles">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="<%# Eval("FileType") %>"><a class="fileName" href="/FileDownload?id=<%# Eval("Id") %>">
                            <%# Eval("Name")%>
                        </a><small>uploaded
                            <%# Eval("CreateDate") %>
                            by <a href="/member/<%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Id %>">
                                <%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Text %></a>
                            <br />
                            <strong>
                                <%# Eval("UmbracoVersion") %><br />
                                .NET Version:
                                <%# Eval("DotNetVersion") %><br />
                                <%# Eval("MediumTrust") %>
                            </strong></small></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul> </div>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Repeater runat="server" ID="ProjectDocRepeater">
                    <HeaderTemplate>
                        <div id="projectDocumentation" class="tabContent">
                            <h3>
                                Documentation</h3>
                            <ul class="attachedFiles">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="<%# Eval("FileType") %>"><a class="fileName" href="/FileDownload?id=<%# Eval("Id") %>">
                            <%# Eval("Name") %>
                        </a><small>uploaded
                            <%# Eval("CreateDate") %>
                            by <a href="/member/<%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Id %>">
                                <%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Text %></a> </small>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul> </div>
                    </FooterTemplate>
                </asp:Repeater>
                <div id="projectResources" class="tabContent">
                    <h3>
                        Resources</h3>
                    <ul>
                        <asp:Literal runat="server" ID="ProjectResources" />
                    </ul>
                </div>
                <asp:Repeater runat="server" ID="ArchiveRepeater">
                    <HeaderTemplate>
                        <div id="projectArchive" class="tabContent">
                            <h3>
                                Archived Files</h3>
                            <ul class="attachedFiles">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="<%# Eval("FileType") %>"><a class="fileName" href="/FileDownload?id=<%# Eval("Id") %>">
                            <%# Eval("Name") %>
                        </a><small>uploaded
                            <%# Eval("CreateDate") %>
                            by <a href="/member/<%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Id %>">
                                <%# ((umbraco.cms.businesslogic.member.Member)Eval("Member")).Text %></a>
                            <asp:PlaceHolder ID="PlaceHolder1" runat="server" Visible='<%# (Eval("FileType") == "package") %>'>
                                <br />
                                <strong>
                                    <%# Eval("UmbracoVersion") %><br />
                                    .NET Version:
                                    <%# Eval("DotNetVersion") %><br />
                                    <%# Eval("MediumTrust") %>
                                </strong></asp:PlaceHolder>
                        </small></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul> </div>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
            <script language="javascript">
                $(document).ready(function () {



                    var t = $('#tabNav');


                    $.each($('div.tabContent'), function () {
                        t.append('<li><a href="#' + $(this).prop('id') + '">' + $(this).find('h3').text() + '</a></li>');
                    });

                    $('#tabs div.tabContent').hide();
                    $('#tabs div.tabContent:first').show();
                    $('#tabNav li:first').addClass('current');

                    $('#tabNav li a').click(function () {
                        $('#tabNav li').removeClass('current');
                        $(this).parent().addClass('current');
                        var currentTab = $(this).attr('href');
                        $('#tabs div.tabContent').hide();
                        $(currentTab).show();
                        return false;
                    });
                });
            </script>
        </div>
    </div>
</div>
