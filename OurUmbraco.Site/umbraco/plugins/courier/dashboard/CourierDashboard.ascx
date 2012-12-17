<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourierDashboard.ascx.cs" Inherits="Umbraco.Courier.UI.Dashboard.CourierDashboard" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<script type="text/javascript">
    jQuery(function () {

        jQuery.ajax({
            type: 'GET',
            url: 'plugins/courier/pages/feedproxy.aspx?url=http://umbraco.com/feeds/videos/courier',
            dataType: 'xml',
            success: function (xml) {

                var html = "<div class='tvList'>";

                jQuery('item', xml).each(function () {

                    html += '<div class="tvitem">'
                    + '<a target="_blank" href="'
                    + jQuery(this).find('link').eq(0).text()
                    + '">'
                    + '<div class="tvimage" style="background: url(' + jQuery(this).find('thumbnail').attr('url') + ') no-repeat center center;">'
                    + '</div>'
                    + jQuery(this).find('title').eq(0).text()
                    + '</a>'
                    + '</div>';
                });

                html += "</div>";

                jQuery('#latestCourierVideos').html(html);
            }

        });



    });
</script>
<style type="text/css">
.tvList .tvitem
    {
        font-size: 11px;
        text-align: center;
        display: block;
        width: 130px;
        height: 158px;
        margin: 0px 20px 20px 0px;
        float: left;
        overflow: hidden;
    }
    .tvList a
    {
        overflow: hidden;
        display: block;
    }
    .tvList .tvimage
    {
        display: block;
        height: 120px;
        width: 120px;
        overflow: hidden;
        border: 1px solid #999;
        margin: auto;
        margin-bottom: 10px;
    }
</style>

<umb:Pane ID="bugreportpane" runat="server" Text="Bug report" Visible="false">
    <div class="dashboardWrapper">
    <h2>Courier Beta Bug reports</h2>
    <img src="/umbraco/dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
    <h3>
        If you encounter anything that doesn't work as expected, please let us know</h3>
     <p>
        Should you encounter any bugs or issues with using Umbraco Courier 2, please submit
        a bug report to us, so we can fix it as fast as possible.
    </p>

    <p>
        <button onclick="window.open('<%= Umbraco.Courier.UI.UIConfiguration.BugSubmissionURL %>'); return false;">Submit a bug</button>
    </p>
</div>
</umb:Pane>


<umb:Feedback ID="expressNotice" runat="server" />   

<umb:Pane ID="register" runat="server" Visible="false">  
    <div class="dashboardWrapper">
    <h2>Thank you for trying out Umbraco Courier 2!</h2>
    <img src="/umbraco/dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
        
    <h3>To purchase a license</h3>
        <p> To purchase this product, simply go to the <a target="_blank" href="http://umbraco.org/redir/<%= Umbraco.Courier.Core.Licensing.InfralutionLicensing.LICENSE_PRODUCTNAME %>">
            Umbraco.com site</a> and you're up and running in minutes!</p>
            
        <p>If you've already purchased a license, you can install it 
            automatically by using your <strong>Umbraco.com profile credentials</strong> below.</p>
            
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
            <umb:Feedback ID="licenseFeedback" runat="server" />
            <asp:Panel ID="licensingLogin" runat="server">
                    <umb:PropertyPanel runat="server" Text="E-mail">
                        <asp:TextBox ID="login" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" Text="Password">
                        <asp:TextBox ID="password" TextMode="Password" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" Text=" ">
                    <p>
                        <asp:Button ID="getLicensesButton" runat="server" Text="Get My licenses From umbraco.org" OnClick="getLicensesButton_Click" />  
                    </p>
                    </umb:PropertyPanel>
            </asp:Panel>
                
            <asp:Panel ID="listLicenses" runat="server" Visible="false">
                    <umb:PropertyPanel runat="server">
                        <h4>
                            Following licenses was found via your profile on umbraco.org:
                        </h4>
                        <p>    
                        <asp:RadioButtonList ID="licensesList" runat="server" />
                        </p>                        
                        <p>
                        <asp:Button ID="chooseLicense" runat="server" Text="Use or configure this license" OnClick="chooseLicense_Click" />
                        </p>
                    </umb:PropertyPanel>
                    
            </asp:Panel>
                
            <asp:Panel ID="configureLicense" runat="server" Visible="false">
                   
                    <umb:PropertyPanel ID="PropertyPanel7" runat="server">
                    <p><strong>Please choose the domain that should be used for this license (without www - for instance 'mysite.com')</strong></p>
                    <p>Any subdomain will work with this license, ie. 'www.mysite.com', 'dev.mysite.com', 'staging.mysite.com'. In addition 'localhost' will always work.</p>
                    </umb:PropertyPanel>
                    
                    <umb:PropertyPanel runat="server" text="Domain">
                        <asp:TextBox ID="domainOrIp" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox><br />
                   
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="domainOrIp" runat="server" ErrorMessage="Please enter a domain"></asp:RequiredFieldValidator>
                    </umb:PropertyPanel>
                    

                    <umb:PropertyPanel runat="server" Text=" ">
                        <p>
                            <asp:Button ID="generateLicenseButton" runat="server" Text="Generate and save license" OnClick="generateLicense_Click" />
                        </p>
                    </umb:PropertyPanel>
                    
            </asp:Panel>                
               
            </ContentTemplate>
        </asp:UpdatePanel>        
    </div>
</umb:Pane>

<umb:Pane ID="welcome" runat="server" Visible="true">

    <div class="dashboardWrapper">
    <h2>Welcome to Courier 2</h2>
    <img src="/umbraco/dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
    
    
    <div class="dashboardColWrapper">
        <div class="dashboardCols">
            <div class="dashboardCol third">
            <h3>Installation resources:</h3>
                <ul>
                    <li><a href="http://nightly.umbraco.org/UmbracoCourier/Installation%20Guide.pdf" target="_blank">Initial setup and configuration</a></li>
                    
                    <li><a href="http://umbraco.com/help-and-support/video-tutorials/umbraco-pro/courier" target="_blank">Introduction videos</a></li>

                    <li><a href="http://umbraco.com/help-and-support/customer-area/courier-2-support-and-download" target="_blank">Support area on Umbraco.com</a></li>
                                        
                    <li><a href="<%= Umbraco.Courier.UI.UIConfiguration.BugSubmissionURL %>" target="_blank">Report issues</a></li>
                </ul>
            </div>

            <div class="dashboardCol third">
            <h3>Developer resources:</h3>
                <ul>
                    <li><a href="http://nightly.umbraco.org/UmbracoCourier/Developer%20Docs.pdf" target="_blank">Developer documentation</a></li>

                    <li><a href="http://umbraco.com/pro-downloads/courier2//Sample%20Itemprovider.pdf" target="_blank">Sample ItemProvider API documentation</a></li>

                    <li><a href="http://umbraco.com/help-and-support/customer-area/courier-2-support-and-download/developer-documentation" target="_blank">Sample Sourcecode</a></li>

                    <li><a href="http://nightly.umbraco.org/umbracoCourier" target="_blank">Courier 2, nightly builds</a></li>
                                        
                    <li><a href="http://nightly.umbraco.org/umbracoCourier/changes.txt" target="_blank">List of recent changes</a></li>
                </ul>
            </div>

            <div class="dashboardCol third last">
            <h3>Licensing and product info</h3>
                <ul>
                    <li><a href="http://umbraco.com/products/more-add-ons/courier-2" target="_blank">Courier 2 on umbraco.com</a></li>

                    <li><a href="http://umbraco.com/profile/options/manage-licenses" target="_blank">Your product licenses</a></li>

                    <li><a href="http://umbraco.com/profile/options/manage-licenses/license-support" target="_blank">License helpdesk</a></li>
                </ul>
            </div>
        </div>
    </div>

    </div>   
</umb:Pane>

<umb:Pane runat="server" Visible="true">

    <div class="dashboardWrapper">
    <h2>Watch and learn</h2>
    <img src="/umbraco/dashboard/images/tv.png" alt="Videos" class="dashboardIcon">

    <div id="latestCourierVideos">Loading...</div>
        
</umb:Pane>