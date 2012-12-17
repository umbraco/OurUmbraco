<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="patch.ascx.cs" Inherits="Umbraco.PoetPatcher.usercontrols.patch" %>

<style type="text/css">

.ok{color:Green;}
.notok{color:Red;}

</style>
<div style="padding: 10px;">

<h3>ASP.NET Security Vulnerability Patch</h3>
<p>
    Tests your umbraco installation for vulnerbilities and automaticly fixes any isssues
</p>
<p>
    For full details, please read the <a id="A2" href="~/umbraco/plugins/PoetPatcher/Guide.pdf" target="_blank" runat="server">upgrade guide</a>.
</p>


<asp:Panel ID="pnl_status" runat="server">

<asp:Literal ID="lit_checkstatus" runat="server"></asp:Literal>

<asp:Button ID="bt_execute" runat="server" Text="Fix this problem" style="font-size: 22px;" Visible="false"  onclick="bt_execute_Click"/>
</asp:Panel>



<asp:Panel ID="pnl_UnableToExecutePatch" runat="server" Visible="false">

<h4>Unable to apply patch automaticly</h4>
<p>Your settings need to be updated, but it looks like this will have to be done manually.</p> 

<p>
To perform this action manually please take a look at our 
<a href="~/umbraco/plugins/PoetPatcher/Guide.pdf" target="_blank" runat="server">upgrade guide</a>.</p>

<p>
    Additionally you can place this security tester on your developer dashboard and re-run the security test</br>
    <asp:LinkButton ID="lbt_PlaceOnDashboard" runat="server" onclick="lbt_PlaceOnDashboard_Click">Place this control on the developer dashboard</asp:LinkButton>. <asp:Literal ID="lit_status" runat="server"></asp:Literal>     
</p>

</asp:Panel>

<asp:Panel ID="pnl_PatchApplied" runat="server" Visible="false">
<h4>Patch has been applied</h4>
<p class="ok">Your umbraco installation has been upgrade, the following tasks has been performed:</p>
<ul>
    <li>
        <strong class="ok">Added/updated customErrors element in web.config</strong><br />
        Your website exposed error information valuable to a hacker, this has been turned off   
     </li>
     <li>
        <strong class="ok">Setup custom errors page</strong><br />
        Instead of exposing system information to potential hackers, 
        a standard error message is returned
    </li>
</ul>
</asp:Panel>

</div>
