<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IndexStatus.ascx.cs"
    Inherits="FergusonMoriyama.Umbraco.ExamineGui.UserControls.IndexStatus" %>

<%@ Import Namespace="Examine" %>
<%@ Import Namespace="Examine.LuceneEngine.Config" %>
<%@ Import Namespace="Examine.LuceneEngine.Providers" %>
<%@ Import Namespace="umbraco.IO" %>
<%@ Import Namespace="UmbracoExamine" %>

<style type="text/css">
    .examineConfig table
    {
        border-collapse: collapse;
        margin-bottom: 15px;
        width: 100%;
    }
    .examineConfig td, th
    {
        border: 1px solid #000000;
        vertical-align: baseline;
    }
    .examineConfig td, th
    {
        padding: 8px;
    }
    .examineConfig th
    {
        background-color: #dddddd;
    }
    .examineConfig td.heading
    {
        background-color: #aaaaaa;
        font-weight: bold;
    }
    .examineConfig td.heading img
    {
        margin-right: 8px;
        cursor: pointer;
    }
    .examineConfig td.label
    {
        background-color: #cccccc;
        width: 20%;
    }
    .examineConfig tr.hideByDefault
    {
        display: none;
    }
    
    .examineConfig input.button, select
    {
        width: 200px;
        padding: 5px;
        margin: 5px;
    }
</style>
<script type="text/javascript">
    <!--


    String.prototype.endsWidth = function (s) {
        return this.length >= s.length && this.substr(this.length - s.length) == s;
    };

    var fmExamine = {};
    fmExamine.timeOutInterval = 7000;


    fmExamine.ajax = function(url, data, fn) {
        $.ajax({
            type: "POST",
            url: fmExamine.umbracoPath + '/plugins/ExamineDash/Actions.asmx/' + url,
            data: data,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: fn
        });
    };

    fmExamine.umbracoPath = '<%= IOHelper.ResolveUrl(ConfigurationManager.AppSettings["umbracoPath"]) %>';

    fmExamine.refresh = function (el, method, indexProvider) {
        fmExamine.ajax(
                method, "{'indexProvider': '" + indexProvider + "' }",
                function (data) {
                    $('#' + el).html(data.d + ($('#' + el).html().endsWidth('.') ? '' : ' ...'));
                    setTimeout("fmExamine.refresh('" + el + "', '" + method + "', '" + indexProvider + "')", fmExamine.timeOutInterval);
                }
        );
    };

    fmExamine.button = function (button, command) {

        var t = $(button).attr('value');
        $(button).attr('value', 'Please Wait...');
        var indexProvider = $(button).parent().find('input[type=hidden]').attr('value');

        fmExamine.ajax(
                command, "{'indexProvider': '" + indexProvider + "' }",
                function (data) {
                    $(button).attr('value', t);

                }
        );

    };

    $().ready(function () {

        $('.examineConfig .numberOfDocuments').each(function () {
            var indexProvider = $(this).attr('class').replace(new RegExp("^.+?\\s(.*)$", "g"), "$1");
            fmExamine.refresh($(this).attr('id'), 'NumberOfDocuments', indexProvider);
        });

        $('.examineConfig .queuedItems').each(function () {
            var indexProvider = $(this).attr('class').replace(new RegExp("^.+?\\s(.*)$", "g"), "$1");
            fmExamine.refresh($(this).attr('id'), 'QueueStatus', indexProvider);
        });

        $('.examineConfig td.heading a').click(function (e) {

            $(this).parentsUntil('table').find('tr.hideByDefault').toggle();

            var i = $(this).find('img');
            $(i).attr('src').endsWidth('plus.png') ? $(i).attr('src', '/umbraco/images/small_minus.png') : $(i).attr('src', '/umbraco/images/small_plus.png');

            e.preventDefault();
            return false;
        });

        $('.examineConfig .rebuild').click(function () {
            fmExamine.button(this, 'RebuildIndex');
            return false;
        });

        $('.examineConfig .deleteQueue').click(function () {
            fmExamine.button(this, 'DeleteQueue');
            return false;
        });

        $('.examineConfig .deleteIndex').click(function () {
            fmExamine.button(this, 'DeleteEntireIndex');
            return false;
        });
    });
    //-->
</script>
<div class="examineConfig">
    <p>Only fields marked with * auto refresh.</p>
    <asp:Button ID="ReloadButton" runat="server" Text="Reload" CssClass="button"/>
    <h2>
        Index Sets</h2>
    <asp:Repeater ID="IndexSetRepeater" runat="server" Visible="True">
        <ItemTemplate>
            <table>
                <tr>
                    <td colspan="2" class="heading">
                        <a href="javascript:void(null);" onclick="return false;"><img src="/umbraco/images/small_plus.png" /></a><%# ((IndexSet) Container.DataItem).SetName %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Name
                    </td>
                    <td>
                        <%# ((IndexSet)Container.DataItem).SetName%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Directory
                    </td>
                    <td>
                        <%# ((IndexSet) Container.DataItem).IndexDirectory %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Parent Id
                    </td>
                    <td>
                        <%# (((IndexSet)Container.DataItem).IndexParentId == null) ? "N/A" : ((IndexSet)Container.DataItem).IndexParentId.ToString() %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Attribute Fields
                    </td>
                    <td>
                        <asp:Repeater ID="IndexAttributeFieldsRepeater" runat="server">
                            <HeaderTemplate>
                                <table>
                                    <thead>
                                        <tr>
                                            <th>
                                                Name
                                            </th>
                                            <th>
                                                Type
                                            </th>
                                            <th>
                                                Enable Sorting?
                                            </th>
                                        </tr>
                                    </thead>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).Name %>
                                    </td>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).Type %>
                                    </td>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).EnableSorting %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Literal ID="NoIndexAttributeFieldsLiteral" runat="server" Visible="false">
                        <p>No items.</p>
                        </asp:Literal>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index User Fields
                    </td>
                    <td>
                        <asp:Repeater ID="IndexUserFieldsRepeater" runat="server">
                            <HeaderTemplate>
                                <table>
                                    <thead>
                                        <tr>
                                            <th>
                                                Name
                                            </th>
                                            <th>
                                                Type
                                            </th>
                                            <th>
                                                Enable Sorting?
                                            </th>
                                        </tr>
                                    </thead>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).Name %>
                                    </td>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).Type %>
                                    </td>
                                    <td>
                                        <%# ((IndexField) Container.DataItem).EnableSorting %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Literal ID="NoIndexUserFieldsLiteral" runat="server" Visible="false">
                        <p>No items.</p>
                        </asp:Literal>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Include Node Types
                    </td>
                    <td>
                        <asp:Literal ID="IncludeNodeTypesLiteral" runat="server">
                        <p>No items.</p>
                        </asp:Literal>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Exclude Node Types
                    </td>
                    <td>
                        <asp:Literal ID="ExcludeNodeTypesLiteral" runat="server">
                        <p>No items.</p>
                        </asp:Literal>
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    <h2>
        Index Providers</h2>
    <asp:Repeater ID="IndexProvideRepeater" runat="server" EnableViewState="false">
        <ItemTemplate>
            <table>
                <tr>
                    <td colspan="2" class="heading">
                        <a href="javascript:void(null);" onclick="return false;"><img src="/umbraco/images/small_plus.png" /></a>
                        <%# ((ProviderSettings)Container.DataItem).Name %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Set Name
                    </td>
                    <td>
                        <%# ((ProviderSettings)Container.DataItem).Name %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Type
                    </td>
                    <td>
                        <%# ((ProviderSettings)Container.DataItem).Type %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Analyzer
                    </td>
                    <td>
                        <%# ((LuceneIndexer) ExamineManager.Instance.IndexProviderCollection[((ProviderSettings)Container.DataItem).Name]).IndexingAnalyzer.GetType().FullName%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Set
                    </td>
                    <td>
                        <%# ((LuceneIndexer) ExamineManager.Instance.IndexProviderCollection[((ProviderSettings)Container.DataItem).Name]).IndexSetName%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Run Async
                    </td>
                    <td>
                        <%# ((LuceneIndexer) ExamineManager.Instance.IndexProviderCollection[((ProviderSettings)Container.DataItem).Name]).RunAsync%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Support Unpublished Content
                    </td>
                    <td>
                        
                           
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Support Protected Content
                    </td>
                    <td>
                        <%# SupportsProtected(((ProviderSettings)Container.DataItem).Name) %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Default Event handler enabled?
                    </td>
                    <td>
                        
                       
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Folder
                    </td>
                    <td>
                        <%# ((LuceneIndexer) ExamineManager.Instance.IndexProviderCollection[((ProviderSettings)Container.DataItem).Name]).LuceneIndexFolder.FullName %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Queue *
                    </td>
                    <td id="<%# ((ProviderSettings)Container.DataItem).Name %>QueuedItems" class="queuedItems <%# ((ProviderSettings)Container.DataItem).Name %>">
                       
                    </td>
                </tr>
                  <tr class="hideByDefault">
                    <td class="label">
                        Number of documents *
                    </td>
                    <td id="<%# ((ProviderSettings)Container.DataItem).Name %>NumberOfDocuments" class="numberOfDocuments <%# ((ProviderSettings)Container.DataItem).Name %>">
                        <%# NumberOfDocuments(((ProviderSettings)Container.DataItem).Name)%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Data Service Type
                    </td>
                    <td>
                        <%# 
                        ExamineManager.Instance.IndexProviderCollection[((ProviderSettings)Container.DataItem).Name].Name
                       
                        %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Actions
                    </td>
                    <td>
                        <asp:HiddenField ID="IndexProviderId" runat="server" Value="<%# ((ProviderSettings)Container.DataItem).Name %>"/>

                        <asp:Button ID="RebuildIndexButton" runat="server" Text="Rebuild Index" CssClass="button rebuild" CommandName="Rebuild" CommandArgument="<%# ((ProviderSettings)Container.DataItem).Name %>"/>
                        <br />

                        <!--<asp:Button ID="DeleteQueueButton" runat="server" Text="Delete Queue" CssClass="button deleteQueue"  CommandName="Delete" CommandArgument="<%# ((ProviderSettings)Container.DataItem).Name %>"/> -->
                        <br />
                        <!-- <asp:Button ID="DeleteIndexButton" runat="server" Text="Delete Entire Index" CssClass="button deleteIndex <%# ((ProviderSettings)Container.DataItem).Name %>"  CommandName="DeleteIndex" CommandArgument="<%# ((ProviderSettings)Container.DataItem).Name %>"/>
                        <br /> -->
                        <!-- <asp:Button ID="ReIndexDocTypeButton" runat="server" Text="ReIndex Doc Type" CssClass="button" />
                        <asp:DropDownList ID="DocumentTypeDropDownList" runat="server" /> -->
                        <br />
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    <h2>
        Search Providers</h2>
    <asp:Repeater ID="SearchProviderRepeater" runat="server">
        <ItemTemplate>
            <table>
                <tr>
                    <td colspan="2" class="heading">
                        <a href="javascript:void(null);" onclick="return false;"><img src="/umbraco/images/small_plus.png" /></a>
                        <%# ((ProviderSettings)Container.DataItem).Name %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Name
                    </td>
                    <td>
                        <%# ((ProviderSettings)Container.DataItem).Name %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Type
                    </td>
                    <td>
                        <%# ((ProviderSettings)Container.DataItem).Type %>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Default?
                    </td>
                    <td>
                        <%# (((ProviderSettings)Container.DataItem).Name == ExamineManager.Instance.DefaultSearchProvider.Name)%>
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Set
                    </td>
                    <td>
                       
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Enable Leading Wildcards?
                    </td>
                    <td>
                       
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Analyzer
                    </td>
                    <td>
                       
                    </td>
                </tr>
                <tr class="hideByDefault">
                    <td class="label">
                        Index Folder
                    </td>
                    <td>
                        
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    <h2>
        Logs (Last Hour)</h2>
    <asp:Repeater ID="LogRepeater" runat="server" Visible="True">
        <HeaderTemplate>
            <table>
                <tr>
                    <td class="heading" colspan="2">
                        <a href="javascript:void(null);" onclick="return false;"><img src="/umbraco/images/small_plus.png" /></a>
                        Logs
                    </td>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr class="hideByDefault">
                <td class="label">
                    <%# Eval("DateStamp") %>
                </td>
                <td>
                    <%# Eval("logComment") %>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
</div>
