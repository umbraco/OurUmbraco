<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="YAF_MemberImport.ascx.cs" Inherits="our.usercontrols.YAF_MemberImport" %>


<asp:Button ID="bt_doit" OnClick="DoIt" runat="server" Text="do it" />


<asp:Button id="bt_1" OnClick="convertMemIds" runat="server"  Text="convert member Ids"/>

<h3>

<asp:Literal ID="oo" runat="server"></asp:Literal>

</h3>