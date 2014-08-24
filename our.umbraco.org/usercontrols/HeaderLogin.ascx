<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HeaderLogin.ascx.cs" Inherits="our.usercontrols.HeaderLogin" %>
<asp:PlaceHolder ID="ph_main" runat="server">


  <span class="memberProfileInfo">
    <asp:Literal ID="lt_LoggedInmsg" runat="server">You are logged in as <strong>%name%</strong> <span title="Your karma: %karma%">(%karma%)</span></asp:Literal>
    <asp:Literal ID="lt_notLoggedInmsg" runat="server">You are not logged in</asp:Literal>
  </span>
  
  <asp:HyperLink ID="hl_profile" CssClass="memberLoginLink" runat="server">Your profile</asp:HyperLink>
  <asp:LinkButton ID="lb_logout" OnClick="logout_click" CssClass="memberLoginLink" runat="server">Log out</asp:LinkButton>
  <asp:HyperLink ID="hl_login" CssClass="memberLoginLink" runat="server">Log in</asp:HyperLink>
  
  <asp:HyperLink ID="hl_create" CssClass="memberCreateLink" runat="server">Create a profile</asp:HyperLink>
   

</asp:PlaceHolder>