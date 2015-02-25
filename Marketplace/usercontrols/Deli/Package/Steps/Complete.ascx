<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Complete.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Package.Steps.Complete" %>

<asp:PlaceHolder ID="eligibilityNotice" runat="server">
<div class='eligibilityNotification <%=NotificationClass %>'>
    <asp:Literal runat="server" ID="NotificationMessage" />
</div>
</asp:PlaceHolder>

<fieldset>
    <legend>Make &quot;<asp:Literal runat="server" ID="ProjectName" />&quot; public</legend> 
    <p>By checking the following box your project will become public as long as you have met all of the terms &amp; conditions of listing.  If there are any problems displayed above please recitify and try again.</p> 
    <p>
    <asp:Checkbox runat="server" ID="Live" Text="Make live" />
    </p>
</fieldset>

    <div class="buttons">
        <asp:linkbutton runat="server" Text="Previous" ID="MovePrevious" OnClick="MoveLast"/>&nbsp;
        <asp:Button runat="server" Text="Save" ID="MoveNext" OnClick="Complete_Click"  CssClass="submitButton button tiny green"/>
    </div>