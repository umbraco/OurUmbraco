<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FeaturedProject.ascx.cs" Inherits="Marketplace.usercontrols.Deli.FeaturedProject" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<%@ Import Namespace="Marketplace.Providers.Helpers" %>
<asp:repeater runat="server" ID="ProjectList">
<ItemTemplate>
        <a href="<%# Eval("NiceUrl") %>"><img src="<%= FeatureImage %>" alt="Featured Pakcage Titie" /></a>


        <div class="featuredLinks">
        
        	<a href="/FileDownload?id=<%# Eval("CurrentReleaseFile") %>&amp;release=1" class="download">Download</a>
        	
        	<p>Or <a href="<%# Eval("NiceUrl") %>">find out more</a></p>
        	
        </div>


        <div class="deliPackage">
            
            <div class="brief">
              <a href="<%# Eval("NiceUrl") %>" class="packageIcon" style="background:url(<%# Marketplace.library.GetDefaultScreenshot((string)Eval("DefaultScreenShot")) %>) no-repeat top left;">Package</a>
              
              <h3><a href="<%#Eval("NiceUrl") %>">
                  <%# Eval("Name") %>
              </a></h3>

              <div class="category"><%# Marketplace.library.GetCategoryName((int)Eval("Id")) %></div>


              <div class='commercialIndicator <%# Eval("ListingType")%>'><%# Eval("ListingType")%></div>
            </div>

            <div class="hiLite">
                
                <p><a href="<%# Eval("NiceUrl") %>"><%# Marketplace.library.ShortenText(Eval("Description").ToString())%></a></p>

            </div>

            <div class="popularity">
                <div class="karma">
                    <%# Eval("Karma") %>    
                    <small>Karma</small>
                </div>
                <div class="downloads">
                    <%# Eval("Downloads") %>    
                    <small>Downloads</small>
                </div>
            </div>
        </div>
</ItemTemplate>
</asp:repeater>