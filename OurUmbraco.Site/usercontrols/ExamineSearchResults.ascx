<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExamineSearchResults.ascx.cs" Inherits="our.usercontrols.ExamineSearchResults" %>
<%@ Import Namespace="our.usercontrols" %>

<asp:PlaceHolder ID="phNotValid" runat="server" Visible="false">
<p>Please provide a longer search term</p>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phResults" runat="server">

<div id="search">
    <p>
        Your search for <strong><%=searchTerm %></strong> returned <i><%= this.searchResults.Count() %></i> results.<br />
        You can narrow down your results by unchecking categories below the search field
    </p>
    
    <asp:Repeater ID="searchResultListing" runat="server">
        <HeaderTemplate>
            <div id="results" class="ui-tabs ui-widget ui-widget-content ui-corner-all">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="result <%# ((Examine.SearchResult)Container.DataItem).cssClassName() %>">
                <h3>
                    <a href='<%# ((Examine.SearchResult)Container.DataItem).fullURL()%>'>
                        <%# ((Examine.SearchResult)Container.DataItem).getTitle() %>
                    </a>
                </h3>
                <p>
                
                   <%# ((Examine.SearchResult)Container.DataItem).generateBlurb(250) %>
                </p>
                <cite>http://our.umbraco.org/<%# ((Examine.SearchResult)Container.DataItem).fullURL()%></cite>
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