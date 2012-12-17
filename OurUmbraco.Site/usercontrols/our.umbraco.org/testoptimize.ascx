<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="testoptimize.ascx.cs" Inherits="LuceneTools.testoptimize" %>
<h3>Forum</h3>
<asp:Button runat="server" ID="optimize" OnClick="optimizeForum_Click" Text="Optimize Forum" />

<h3>Projects</h3>
<asp:Button runat="server" ID="Button1" OnClick="optimizeProjects_Click" Text="Optimize Projects" />

<h3>wiki</h3>
<asp:Button runat="server" ID="Button2" OnClick="optimizeWiki_Click" Text="Optimize Wiki" />