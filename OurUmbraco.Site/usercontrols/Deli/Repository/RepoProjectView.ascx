<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="RepoProjectView.ascx.cs" Inherits="Marketplace.usercontrols.Deli.RepoProjectView" %>
<%@ Import Namespace="Marketplace.Interfaces" %>

    
                <h2 class="projectName"><asp:Literal runat="server" ID="ProjectName" /></h2>
                
                <div class="options">
                    <asp:HyperLink runat="server" ID="VendorLink" />
                    started this project on
                    <asp:Literal runat="server" ID="ProjectCreateDate" />
                    it's current version is
                    <strong>
                        <asp:Literal runat="server" ID="ProjectCurrentVersion" />
                    </strong>.
                </div>
                
                <div style="float: right; padding: 10px 0px 30px 40px; width: 250px;">

                    <asp:PlaceHolder runat="server" ID="DownloadPanel">
                        <a href="#" 
                        onclick="if(confirm('Are you sure you wish to download:\n\n<%= Project.Name %>\n\n'))document.location.href = 'http://<%= callback %>&amp;useLegacySchema=<%= useLegacySchema %>&amp;version=<%= version %>&amp;guid=<%= Project.ProjectGuid.ToString() %>&amp;repo=true'">
                            <img src="/css/img/download-package.png"  style="margin-bottom:10px;border:0;" />
                        </a>
                        
                        
                    </asp:PlaceHolder>

                    <asp:PlaceHolder runat="server" ID="CommercePanel">
                   <div class="projectPurchase">
                       <asp:HyperLink ID="PurchaseLink" runat="server" Target="_blank">
                         Visit the Deli to Purchase a License for this project
                       </asp:HyperLink>
                   </div>
                   </asp:PlaceHolder>

                    <div class="box">

                        <h4>Project Summary</h4>
                        <dl class="projetProps summary">
                            <dt>Project owner:</dt>
                            <dd>
                                <%= Project.Vendor.Member.Name %>
                            </dd>
                                                 
                            <asp:repeater runat="server" ID="ContribRepeater">
                            <HeaderTemplate>
                            <dt>Contributors:</dt>
                            <dd>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <%# Eval("ContributorName") %>
                            </ItemTemplate>
                            <FooterTemplate>
                            </dd>
                            </FooterTemplate>
                            </asp:repeater>

                            <dt>Created:</dt>
                            <dd>
                                <%= Project.CreateDate.ToString("D") %>
                            </dd>

                            <dt>Compatible with:</dt>
                            <dd>
                                <asp:Literal runat="server" ID="ProjectCompatitbleWithUmbraco" />
                            </dd>

                            <dt>.NET Version:</dt>
                            <dd>
                                <asp:Literal runat="server" ID="ProjectCompatitbleWithDotNet" />
                            </dd>
 

                            <dt>Supports Medium Trust:</dt>
                            <dd>
                                <asp:Literal runat="server" ID="ProjectCompatitbleWithMediumTrust" />
                            </dd>
 

                            <% if(Project.Stable){ %>
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
                                    <a href="<%= Project.LicenseUrl %>" target="_blank">
                                        <%= Project.LicenseName %>
                                    </a>
                                </dd>
                            <% } %>

                            <asp:Repeater runat="server" ID="TagRepeater">
                            <HeaderTemplate><dt>Tags</dt>
                                <dd></HeaderTemplate>
                            <ItemTemplate>
                            <a href="/deli/tag/<%# Eval("Text") %>">
                                            <%# Eval("Text") %>
                                        </a>&nbsp;
                            </ItemTemplate>
                            <FooterTemplate></dd></FooterTemplate>
                            </asp:Repeater>

                  
                            <dt>Downloads:</dt>
                                <dd>
                                    <asp:Literal runat="server" ID="DownloadCount" />
                                </dd>

                        </dl>
                    </div>

                </div>
                
                
                <div style="width:620px;">
                    
                <div id="projectDescriptionText">
                    <asp:Literal runat="server" ID="ProjectDescrition" />
                    <br />
                    <a href="#" class="show_hide">show more...</a>
                </div> 

                <div id="longDescription">
                    <asp:Literal runat="server" ID="LongProjectDescription" />
                </div>
                
                <script type="text/javascript">

                    $(document).ready(function () {

                        $("#longDescription").hide();
                        $(".show_hide").show();

                        $('.show_hide').click(function () {
                            $("#longDescription").slideToggle();
                        });

                    });

                </script>              
                

                <asp:PlaceHolder runat="server" ID="ScreenshotsPanel">
                <div id="projectScreenshots">
                <div class="divider" style="clear:left;"></div>
                 <div>
                    <h3>Screenshots</h3>
                    <asp:Repeater runat="server" ID="ScreenshotRepeater">
                    <ItemTemplate>
                        <a href="<%# Eval("Path") %>" class="projectscreenshot" rel="shadowbox">
                            <img src="/umbraco/imagegen.ashx?image=<%# Eval("Path") %>&amp;path=true&amp;width=100&amp;height=100" style="border:0;"/>
                        </a>
                    </ItemTemplate>
                    </asp:Repeater>
                </div>
                </div>

                </asp:PlaceHolder>

                <div id="projectFiles">
                <div class="divider" style="clear:left;"></div>

                <asp:PlaceHolder ID="NotVerifiedNotice" runat="server" Visible="false">
                <div class="notice" style="margin-top: 5px; margin-left:5px;">
                        <p>
                        <strong>Please note:</strong>
                        the compatibility between this package and your umbraco version hasn't been verified by our admins yet
                        </p>
                    </div>
                </asp:PlaceHolder>

                    
                </div>

                <div id="projectResources">
                <div class="divider" style="clear:left;"></div>
                       <asp:HyperLink ID="FullProjectLink" runat="server" CssClass="projectDownload" Target="_blank">
                        More Information
                        <span>
                            Visit the project's full page in the Deli
                        </span>
                        </asp:HyperLink>              
                </div>


                <div id="projectDocumentation">
                <div class="divider" style="clear:left;"></div>

                    <asp:Repeater runat="server" ID="ProjectDocRepeater">
                    <ItemTemplate>
                        <a href="/FileDownload?id=<%# Eval("Id") %>" class="projectDownload" target="_blank">
                            Documentation
                        <span>
                            <%# Eval("Name") %>
                        </span>
                        </a>
                    </ItemTemplate>
                    </asp:Repeater>
                </div>
 
                <div class="divider" style="clear:left;"></div>
                </div>
                

    