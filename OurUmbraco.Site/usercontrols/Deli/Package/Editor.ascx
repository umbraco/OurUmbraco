<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Editor.ascx.cs" Inherits="uProject.usercontrols.Deli.Package.Editor" %>

<div class="package-create">
<asp:Repeater runat="server" ID="StepNavigation" OnItemDataBound="bindStep">
<HeaderTemplate><ul class="stepNavigation"></HeaderTemplate>
<ItemTemplate>
    <li class='<asp:literal runat="server" ID="lt_class" />'><asp:Literal runat="server" ID="lt_name" /></li>
</ItemTemplate>
<FooterTemplate></ul></FooterTemplate>
</asp:Repeater>

<input type="hidden" runat="server" value="details" id="Step" />
<asp:PlaceHolder runat="server" ID="StepPlaceHolder" />
</div>

