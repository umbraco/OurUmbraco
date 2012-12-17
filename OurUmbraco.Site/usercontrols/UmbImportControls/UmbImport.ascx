<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UmbImport.ascx.cs" Inherits="UmbImport.Import" %>
<%@ Register src="ImportSteps/Intro.ascx" tagname="Intro" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/SelectDataSourceType.ascx" tagname="SelectDataSourceType" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/SelectDataSource.ascx" tagname="SelectDataSource" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/ContentImport/SelectUmbracoTypeAndLocation.ascx" tagname="SelectUmbracoTypeAndLocation" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/MapProperties.ascx" tagname="MapProperties" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/ConfirmSelectedOptions.ascx" tagname="Confirm" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/Importing.ascx" tagname="Importing" tagprefix="umbImport" %>
<%@ Register src="ImportSteps/MemberImport/SelectMembertype.ascx" tagname="SelectMembertype" tagprefix="umbImport" %>
<h3><asp:Literal ID="StepTitleLiteral" runat="server"/></h3>
<br /><br />
<asp:CustomValidator ID="ValidateStep" runat="server" 
    OnServerValidate="Validate" /><br />
<asp:PlaceHolder ID="Step0PlaceHolder" runat="server" Visible="False"><umbImport:Intro ID="IntroStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step1PlaceHolder" runat="server" Visible="False"><umbImport:SelectDataSourceType ID="SelectDataSourceTypeStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step2PlaceHolder" runat="server" Visible="False"><umbImport:SelectDataSource ID="SelectDataSourceStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step3PlaceHolderContent" runat="server" Visible="False"><umbImport:SelectUmbracoTypeAndLocation ID="SelectUmbracoTypeAndLocationStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step3PlaceHolderMember" runat="server" Visible="False"><umbImport:SelectMembertype ID="SelectMembertypeStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step4PlaceHolder" runat="server" Visible="False"><umbImport:MapProperties ID="MapPropertiesStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step5PlaceHolder" runat="server" Visible="False"><umbImport:Confirm ID="ConfirmStep" runat="server" /></asp:PlaceHolder>
<asp:PlaceHolder ID="Step6PlaceHolder" runat="server" Visible="False"><umbImport:Importing ID="ImportStep" runat="server" /></asp:PlaceHolder>
<br /><br />
<asp:Panel ID="ButtonsPanel" CssClass="propertypane" runat="server">
<asp:Button ID="PreviousButton" runat="server"  Text="" CausesValidation="false" OnClick="PreviousButton_Click" />
&nbsp;&nbsp;&nbsp;<asp:Button ID="NextButton" runat="server" Text=""  OnClick="NextButton_Click" OnClientClick="document.getElementById('loadingDiv').style.display = 'inline';" />&nbsp;&nbsp;&nbsp;
<div id="loadingDiv" style="display:none"><asp:Literal ID="LoadingLiteral" runat="server" /></div>
</asp:Panel>


