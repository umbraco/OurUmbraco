<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="OurUmbraco.Documentation" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GetDocsButton.Click += GetDocsButton_Click;
    }

    protected string Status = "";
    
    void GetDocsButton_Click(object sender, EventArgs e)
    {
        var docsUpdater = new DocumentationUpdater();
        docsUpdater.EnsureGitHubDocs();
        Status = "Done!";
    }

</script>

<div>
    
    <asp:Button runat="server" ID="GetDocsButton" Text="Get Docs!!"/>
    
    <p>
    <%= Status %>
    </p>

</div>