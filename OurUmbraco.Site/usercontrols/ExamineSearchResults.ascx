<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExamineSearchResults.ascx.cs" Inherits="our.usercontrols.ExamineSearchResults" %>
<%@ Import Namespace="our.usercontrols" %>
<%@ Import Namespace="our" %>

<asp:PlaceHolder ID="phNotValid" runat="server" Visible="false">
<p>Please provide a longer search term</p>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phResults" runat="server">

<div id="search">
    <p>
        Your search for <strong><%=searchTerm %></strong> returned <i><%= this.searchResults.Count() %></i> results.<br />
        You can narrow down your results by unchecking categories in the search field
    </p>

    <p>
        Order results by:
        <asp:LinkButton runat="server" ID="OrderByDateDescLink" OnClick="OrderByDateDesc">date (newest to oldest)</asp:LinkButton> | 
        <asp:LinkButton runat="server" ID="OrderByDateAscLink" OnClick="OrderByDateAsc">date (oldest to newest)</asp:LinkButton> | 
        <asp:LinkButton runat="server" ID="OrderByNoneLink" OnClick="OrderByNone">default (based on score)</asp:LinkButton>
    </p>  
    <asp:Repeater ID="searchResultListing" runat="server">
        <HeaderTemplate>
            <div id="results" class="ui-tabs ui-widget ui-widget-content ui-corner-all">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="result <%# ((Examine.SearchResult)Container.DataItem).CssClassName() %>">
                <h3>
                    <a href='<%# ((Examine.SearchResult)Container.DataItem).FullUrl()%>'>
                        <%# ((Examine.SearchResult)Container.DataItem).GetTitle() %>
                    </a>
                </h3>
                <p>
                
                   <%# ((Examine.SearchResult)Container.DataItem).GenerateBlurb(250) %>
                </p>
                <cite><%# ((Examine.SearchResult)Container.DataItem).GetDate() %><br />http://our.umbraco.org/<%# ((Examine.SearchResult)Container.DataItem).FullUrl()%></cite>
            </div>            
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>
</div>


<asp:Literal ID="pager" runat="server"></asp:Literal>

</asp:PlaceHolder>

<!--
  <script type="text/javascript">
    jQuery(document).ready(function () {
        jQuery("#results").tabs();
    });
</script>

</div>
-->