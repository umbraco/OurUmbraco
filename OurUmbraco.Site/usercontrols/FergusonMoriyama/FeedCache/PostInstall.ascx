<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PostInstall.ascx.cs" Inherits="FergusonMoriyama.Umbraco.PostInstall" %>
<p>
    Feed cache for Umbraco has been successfully installed. If you are upgrading your config has been moved to
    /config/FergusonMoriyama/FeedCache/feeds.config
</p>

<p>
    <asp:CheckBox ID="CheckBox1" runat="server" Checked="true"/> Send information of this installation to the author to allow collection of installation statistics. This is a one off http request and we just record the date and time of install. No 
    other information is stored. Please leave this option enabled to support future development of this product.
</p>

<div>
    
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Complete installation" />

</div>



