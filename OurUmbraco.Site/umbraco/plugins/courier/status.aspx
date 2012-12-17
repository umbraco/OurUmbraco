<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="status.aspx.cs" Inherits="Umbraco.Courier.UI.Pages.status"  MasterPageFile="../MasterPages/CourierPage.Master"   %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<umb:Pane runat="server">
    <div align="center" style="padding: 10px">
    <p><%= Request["Message"] %></p>

    <img src="/umbraco_client/images/progressbar.gif" alt="loading" />
    <small id="currentStatus" style="display: block; padding: 10px"></small>

    <a href="#"><small>Show details</small></a>

    <div id="StatusLines" clientidmode="Static" runat="server" style="display: none; white-space: nowrap; font-family: consolas; text-align: left; font-size: 10px; background: #fff; color: #999; border-top: #efefef 1px solid; padding: 10px; margin-top: 10px;">
    </div>
</umb:Pane>

    <script type="text/javascript">

    var closeWhenDone = true;
    var failures = 0;
            function setReload(statusId) {
                setTimeout("reload('" + statusId + "');", 500);
            }

            function showDetails() {
                jQuery("#currentStatus").hide();
                jQuery("a").hide();
                jQuery("#StatusLines").show();
                closeWhenDone = false;
            }

            function reload(statusId) {
                $.ajax({
                    type: "POST",
                    url: "/umbraco/plugins/courier/webservices/Courier.asmx/GetEngineStatus",
                    dataType: "json",
                    data: "{maxLines:500,statusId:'" + statusId + "'}",
                    contentType: "application/json; charset=utf-8",
                    success: function (msg) {

                        var currentStatus = msg.d[0].Message;

                        if (currentStatus != "#0") {

                            jQuery("#currentStatus").html(currentStatus);
                            setReload(statusId);

                            var log = "";
                            $.each(msg.d, function (i, item) {
                                log += item.Message + "</br>";
                            });
                            jQuery("#StatusLines").html(log);


                        } else if (closeWhenDone) {
                            UmbClientMgr.closeModalWindow();
                            return;
                        } else {
                            jQuery("img").hide();
                            showDetails();
                        }
                    },
                    failure: function (msg) {
                        failures++;
                        if (failures < 10)
                            setReload(statusId);
                        else
                            jQuery("#StatusLines").html("<span style='color:red'>An error occured while trying to get the status, please reload this frame/page.</span><br/>"+ jQuery("#StatusLines").html());
                    }
                });
            }

            setReload('<%=Request.Params["statusId"] %>');

            jQuery("a").click(function () {
                showDetails();
                return false;
            });

        </script>
    </div>
</asp:Content>