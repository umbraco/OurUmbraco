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
                <div class="utilities">
                    <ul id="breadcrumb">
                        <li><a href="/" rel="nofollow">Our Umbraco</a></li>
                        <li><a href="#" rel="nofollow">Search</a></li>
                        <li><a href="#" rel="nofollow"><%=Model.Results.SearchTerm %></a></li>
                    </ul>
                </div>

                <div class="search-big">
                    <asp:TextBox runat="server" ID="SearchText"></asp:TextBox>
                    <label for="<%=SearchText.ClientID %>">Search</label>
                </div>

                <% if (Context.IsDebuggingEnabled)
                   { %>
                <div style="border: 1px solid orange;">
                    <p>
                        <strong>Debugging output</strong><br />
                        <strong>Query:</strong> <%= Model.Results.LuceneQuery %><br />
                        <strong>Order by:</strong> <%= Model.Results.OrderBy %><br />
                        <strong>Time elapsed:</strong> <%= Model.Results.Totalmilliseconds %>
                    </p>
                    <div id="search-options" class="search-options">
                        <% if(Request.QueryString["cat"] == "forum")
                           { %>

                            <label>Options:</label>
                            <div class="options">
                                <span><input type="checkbox" name="solved"/> show only solved topics</span><br/>
                                <span><input type="checkbox" name="replies"/> show only topics with replies</span><br/>
                                <span><input type="checkbox" name="order" value="updateDate"/> show last updated first</span>
                            </div>
                        </div>
                        <% } %>

                        <% if (Request.QueryString["cat"] == "project")
                           { %>
                            <span>Options:</span><br/>
                            <span><input type="checkbox" name="order" value="updateDate"/> show last updated first</span>
                        <% } %>

                        <% } %>
                </div>
                

                <% if (Model.Results.SearchResults.Any() == false)
                   { %>
                <p class="message">No results</p>
                <% } %>


                <section>
                    <% if (string.IsNullOrWhiteSpace(Model.Results.Category) == false)
                       { %>
                    <h2 class="search-in">Results from category: <%= Model.Results.Category %></h2>
                    <% } %>

                    <ul class="search-all-results docs-search-listing">
                        <% foreach (var result in Model.Results.SearchResults)
                           {%>
                        <li class="<%= result.SolvedClass() %>">
                            <a href="<%=result.FullUrl() %>">
                                <div class="type-icon">
                                    <i class="<%=result.GetIcon() %>"></i>
                                </div>

                                <div class="type-context">
                                    <div class="type-name"><%=result.GetTitle() %></div>
                                    <span class="type-datetime"><%=result.GetDate() %></span>
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
