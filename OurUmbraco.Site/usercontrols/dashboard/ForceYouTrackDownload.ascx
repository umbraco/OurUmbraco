<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="uRelease.Controllers" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GetYouTrackButton.Click += GetYouTrackButton_Click;
    }

    protected string Status = "";
    
    void GetYouTrackButton_Click(object sender, EventArgs e)
    {
        var releases = new ReleaseController();
        var result = releases.SaveAllToFile();
        Status = result;
    }

</script>

<div>
    
    <asp:Button runat="server" ID="GetYouTrackButton" Text="Get YouTrack Releases"/>
    
    <p>
    <%= Status %>
    </p>

</div>