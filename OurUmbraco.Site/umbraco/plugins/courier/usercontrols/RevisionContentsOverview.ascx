<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RevisionContentsOverview.ascx.cs" Inherits="Umbraco.Courier.UI.Usercontrols.RevisionContentsOverview" %>

<script type="text/javascript">


    jQuery(document).ready(function () {

        if (jQuery('#resourceOverView').size() == 0) {
            jQuery('#showResources').hide();

        }

        if (jQuery('#revisionOverview').size() == 0) {
            jQuery('#showRevisionItems').hide();

        }

        jQuery('#showResources').click(
                function () {
                    jQuery('#resourceOverView').toggle("slow");
                }
            );

        jQuery('#showRevisionItems').click(
                function () {
                    jQuery('#revisionOverview').toggle("slow");
                }
            );

    });

</script>

<h3 style="padding-bottom:5px;">Resources <asp:Literal ID="litResourceInfo" runat="server"></asp:Literal>
</h3>
<a id="showResources" style="cursor:pointer">Show details</a>
<div id="resourceOverView" style="clear:both;display:none;;margin-top:10px">
<asp:GridView ID="GridView1" runat="server">
</asp:GridView>
</div>


<h3 style="padding-bottom:5px;">Revision items <asp:Literal ID="litRevisionInfo" runat="server"></asp:Literal></h3>
<a id="showRevisionItems" style="cursor:pointer">Show details</a>
<div id="revisionOverview" style="clear:both;display:none;margin-top:10px">
<asp:GridView ID="GridView2" runat="server">
</asp:GridView>
</div>

<br />