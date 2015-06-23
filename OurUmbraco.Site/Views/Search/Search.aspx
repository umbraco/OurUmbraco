<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/Master.master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="OurUmbraco.Site.Views.Search.Search" %>

<%@ Import Namespace="Umbraco.Web" %>
<%@ Import Namespace="our" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Search" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Main" runat="server">

    <div class="plain">
        <!-- search OVERVIEW START -->
        <section class="search-results">
            <div>
                <!-- FORUM HEADER START -->
                <div class="forum-archive-header">
                    <ul id="breadcrumb">
                        <li><a href="/" rel="nofollow">Our Umbraco</a></li>
                        <li><a href="#" rel="nofollow">Search</a></li>
                        <li><a href="#" rel="nofollow"><%=Model.Results.SearchTerm %></a></li>
                    </ul>
                    
                    <% if (Context.IsDebuggingEnabled)
                       { %>
                        <p style="border: 1px solid orange;">
                            <strong>Debugging output</strong><br/>
                            <strong>Query:</strong> <%= Model.Results.LuceneQuery %><br />
                            <strong>Order by:</strong> <%= Model.Results.OrderBy %><br />
                            <strong>Time elapsed:</strong> <%= Model.Results.Totalmilliseconds %>
                        </p>
                    <% } %>

                    <div class="search-big">
                        <asp:TextBox runat="server" ID="SearchText" ></asp:TextBox>
                        <label for="<%=SearchText.ClientID %>">Search</label>
                    </div>
                    <% if (Model.Results.SearchResults.Any() == false) { %>
                        <p>No results</p>
                    <% } %>
                    <div class="clear"></div>
                </div>

                <section>

                    <ul class="search-all-results docs-search-listing">
                        <% foreach (var result in Model.Results.SearchResults)
                           {%>
                        <li>
                            <a href="<%=result.FullUrl() %>">
                                <div class="type-icon">
                                    <i class="<%=result.GetIcon() %>"></i>
                                </div>

                                <div class="type-context">
                                    <div class="type-name"><%=result.GetTitle() %></div>
                                    <div class="type-description"><%=result.GenerateBlurb(300) %></div>
                                </div>
                            </a>
                        </li>
                        <% } %>
                    </ul>


                </section>
                <!-- Search end -->

            </div>
        </section>
        <!-- search OVERVIEW END -->
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="EndScripts" runat="server">
</asp:Content>
