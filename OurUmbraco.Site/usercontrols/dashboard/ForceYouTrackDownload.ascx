<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GetYouTrackButton.Click += GetYouTrackButton_Click;
    }

    protected string Status = "";
    
    void GetYouTrackButton_Click(object sender, EventArgs e)
    {
        var import = new uRelease.Import();
        var result = import.SaveAllToFile();
        Status = result;
    }

</script>

<div>
    
    <asp:Button runat="server" ID="GetYouTrackButton" Text="Get YouTrack Releases"/>
    
    <p>
    <%= Status %>
    </p>

</div>