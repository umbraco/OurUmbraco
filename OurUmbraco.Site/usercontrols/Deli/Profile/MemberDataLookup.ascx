<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MemberDataLookup.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Profile.MemberDataLookup" %>

  <asp:Panel ID="lookupLogin" runat="server">
    
    E-mail:
     <asp:TextBox ID="tb_login" runat="server"></asp:TextBox> <br/>
    Password:
     <asp:TextBox ID="tb_password" TextMode="Password" runat="server"></asp:TextBox> <br />

      <asp:Button ID="bt_getMemberData" runat="server" 
          Text="Get My data From umbraco.com" onclick="getMemberData_Click"/>  
  </asp:Panel>


   <asp:Panel ID="lookupFailed" runat="server" Visible="false">
    <h4>Couldn't connect to umbraco.com</h4>
    <p>This could be because your credentials are wrong or because you're not online.</p>"
   </asp:Panel>

   <asp:Panel ID="lookupSuccess" runat="server" Visible="false">
    <h4>We retrieved the following data from umbraco.com</h4>
       <asp:Literal ID="lt_data" runat="server"></asp:Literal>
   </asp:Panel>
  