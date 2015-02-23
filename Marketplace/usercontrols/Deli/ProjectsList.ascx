<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ProjectsList.ascx.cs" Inherits="Marketplace.usercontrols.Deli.ProjectsList" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<%@ Import Namespace="Marketplace.Providers.Helpers" %>
<%@ Register Src="~/usercontrols/Deli/ListPagination.ascx" TagPrefix="deli" TagName="paging" %>

<div class="deliPromoArea">
    <div class="deliPromoBox clearfix">
        <ul class="promoOptions">
            <li><a href="?filter=free" id="filterfree" class="filternav <%= (ListingType == "free")?"on":"" %>">Free</a></li>
            <li><a href="?filter=commercial" id="filtercommercial" class="filternav <%= (ListingType == "commercial")?"on":"" %>">Commercial</a></li>
            <li><a href="<%= Request.Url.GetLeftPart(UriPartial.Path) %>" id="filterboth" class="filternav <%= (String.IsNullOrEmpty(ListingType) || ListingType != "free" && ListingType != "commercial")?"on":"" %>">Both</a></li>
        </ul>

    <asp:PlaceHolder runat="server" ID="ProjectCounter">
        <p class="viewAll">Showing <%= PageStartListingNumber %> - <%=PageEndListingNumber %> of <%=TotalListings %> Projects</p>
    </asp:PlaceHolder>

    <asp:Repeater runat="server" ID="Listing">
    <HeaderTemplate>
        <ul class="summary projectsTagged" id="projectList">
    </HeaderTemplate>
    <ItemTemplate>
            <li class="clearfix">
                <div class="deliPackage">
            
                    <div class="brief">
                      <a href="<%# Eval("NiceUrl") %>" class="packageIcon" style="background:url(<%# Marketplace.library.GetDefaultScreenshot((string)Eval("DefaultScreenShot")) %>) no-repeat top left;">Package</a>
                      <h3><a href="<%# Eval("NiceUrl") %>"><%# Eval("Name") %></a></h3>
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
    <FooterTemplate>
        </ul>
    </FooterTemplate>
    </asp:Repeater>

    <asp:PlaceHolder runat="server" ID="NoListings" Visible="false">
        <div class="noListingMessage clearfix">
        <p>There are no listings that meet your filter criteria</p>
        </div>
    </asp:PlaceHolder>

    </div>
</div>
<deli:paging runat="server" id="paging" />

