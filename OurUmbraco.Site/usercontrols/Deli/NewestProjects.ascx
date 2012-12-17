<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewestProjects.ascx.cs" Inherits="Marketplace.usercontrols.Deli.NewestProjects" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<%@ Import Namespace="Marketplace.Providers.Helpers" %>
<%@ Register Src="~/usercontrols/Deli/ListPagination.ascx" TagPrefix="deli" TagName="paging" %>

<asp:PlaceHolder runat="server" ID="ListingOpen">
<div class="deliPromoArea">
</asp:PlaceHolder>
<div class="deliPromoBox new clearfix">
        <asp:PlaceHolder runat="server" ID="WidgetTitle"><h2>Latest Projects</h2></asp:PlaceHolder>

        <ul class="promoOptions">
            <li><a href="?nplt=free" id="newfree" class="newnav <%= (ListingType == "free")?"on":"" %>">Free</a></li>
            <li><a href="?nplt=commercial" id="newcommercial" class="newnav <%= (ListingType == "commercial")?"on":"" %>">Commercial</a></li>
            <li><a href="<%= Request.Url.GetLeftPart(UriPartial.Path) %>" id="newboth" class="newnav <%= (String.IsNullOrEmpty(ListingType) || ListingType != "free" && ListingType != "commercial")?"on":"" %>">Both</a></li>
        </ul>

        <asp:PlaceHolder runat="server" ID="WidgetFeatures">
            <a href="<%= Request.Url + "/newest" %>" class="viewAll">All new</a>
            <div id="newest-loading" class="deli-loader"><img src="/images/custom/loadanim2.gif" alt="loading newest projects" /></div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="ProjectCounter">
            <p class="viewAll">Showing <%= PageStartListingNumber %> - <%=PageEndListingNumber %> of <%=TotalListings %> Projects</p>
        </asp:PlaceHolder>


<asp:Repeater runat="server" ID="Listing">
<HeaderTemplate><ul id="newest-projects"></HeaderTemplate>
<ItemTemplate>
    <li class="clearfix">
            <div class="deliPackage">
            
            <div class="brief">
              <a href="<%# Eval("NiceUrl") %>" class="packageIcon" style="background:url(<%# Marketplace.library.GetDefaultScreenshot((string)Eval("DefaultScreenShot")) %>) no-repeat top left;">Package</a>
              
              <h3><a href="<%# Eval("NiceUrl") %>">
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
</li>
</ItemTemplate>
<FooterTemplate></ul></FooterTemplate>
</asp:Repeater>
       </div>
    <asp:PlaceHolder runat="server" ID="ListingClose">
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="NoListings" Visible="false">
        <div class="noListingMessage clearfix">
        <p>There are no listings that meet your filter criteria</p>
        </div>
    </asp:PlaceHolder>

<deli:paging runat="server" ID="paging" />