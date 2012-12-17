<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MemberLocator.ascx.cs"
    Inherits="Umb.OurUmb.MemberLocator.Usercontrols.MemberLocator" %>


<%@ Register Assembly="Umb.OurUmb.MemberLocator" Namespace="Umb.OurUmb.MemberLocator.Controls"
    TagPrefix="Controls" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit"  TagPrefix="act"  %>   
<asp:Panel ID="pnlLoggedIn" runat="server" >


<script language="javascript">

    $(document).ready(function() {

    __doPostBack('<%= LinkButton1.UniqueID %>', '');
    
    });
    
    
    
</script> 


<asp:LinkButton ID="LinkButton1" runat="server" onclick="LinkButton1_Click" style="display:none">Get the results</asp:LinkButton>



<asp:Panel ID="pnlRecentMeetup" runat="server" CssClass="success" Visible="false">
    <h4>
        You suggested a meetup recently
    </h4>
    <p>
        Check the
        <asp:HyperLink ID="lnkRecentTopic" runat="server">forum topic</asp:HyperLink>
        for responses</p>
</asp:Panel>


<asp:UpdatePanel ID="UpdatePanel1" runat="server">
 <Triggers>
     <asp:AsyncPostBackTrigger ControlID="LinkButton1" />
</Triggers>

<ContentTemplate>



<asp:Panel ID="pnlMultiple" runat="server" Visible="false" CssClass="LocatorMultiple">
    <asp:Literal ID="litMultipleResults" runat="server"></asp:Literal>
    <Controls:ListValidator ControlToValidate="rblLocations" ID="ListValidator1" ValidationGroup="multiple"
        ErrorMessage="*" runat="server" />
    <div class="LocatorMultipleLocations">
        <asp:RadioButtonList ID="rblLocations" runat="server">
        </asp:RadioButtonList>
    </div>
    <asp:Button ID="btnChooseMultiple" runat="server" Text="Ok" OnClick="btnChooseMultiple_Click"
        ValidationGroup="multiple" />
</asp:Panel>

<asp:Panel ID="pnlCreateTopicSuccess" runat="server" CssClass="success" Visible="false">
    <h4>
        <asp:Literal ID="lblOK" runat="server" Text="Ok, meetup suggested"></asp:Literal>
        </h4>
        
    <asp:PlaceHolder ID="NewTopicContainer" runat="server">
    <p>
        View the
        <asp:HyperLink ID="lnkNewTopic" runat="server">forum topic</asp:HyperLink>
        that has been created</p>
        
        </asp:PlaceHolder>
</asp:Panel>
<asp:Literal ID="litResults" runat="server"></asp:Literal>



</ContentTemplate>
</asp:UpdatePanel>

<div id="memberlocatorextra" style="display:none;">


<asp:Panel ID="pnlSearchOptions" runat="server">
    <fieldset>
        <legend>
            <asp:Literal ID="lblSearch" runat="server" Text="Search"></asp:Literal></legend>
        
        <asp:PlaceHolder ID="LocationContainer" runat="server">
        <p>
            <label class="inputLabel" for="<%= txtLocation.ClientID %>">
                Location:</label>
            <asp:TextBox ID="txtLocation" runat="server" CssClass="title"></asp:TextBox>
        </p>
        
        </asp:PlaceHolder>
       <div id="memlocradius" style="margin-bottom:15px;margin-top:7px;">
            <label class="inputLabel">
                Radius:</label>
          
            <div id="memlocslider">
            <asp:TextBox ID="txtRadius" runat="server"></asp:TextBox>
            <act:SliderExtender ID="SliderExtender1" runat="server"
                TargetControlID="txtRadius"
                Minimum="1"
                Maximum="750"
                Steps="750" 
                Length="300"
                TooltipText="Radius in KM"
                BoundControlID="lblRadius" />
                
            <asp:Label ID="lblRadius" runat="server" Text="Label"></asp:Label> KM
          </div>
      </div>
    </fieldset>
    <div class="buttons">
        <asp:Button ID="btnSearch" runat="server" Text="Go" OnClick="btnSearch_Click" CssClass="submitButton" />
    </div>
</asp:Panel>

<asp:Panel ID="pnlCreateTopic" runat="server" Visible="false">
    <div class="divider">
    </div>
    <fieldset>
   <legend>
       <asp:Literal ID="lblNotify" runat="server" Text="Organize a meetup:"></asp:Literal></legend>
  
   <p>
    <asp:TextBox ID="TextBox1" runat="server" TextMode="MultiLine" Width="100%" Rows="5"></asp:TextBox>
   </p>
    <div class="buttons">
    <asp:Button ID="btnCreateTopic" runat="server" Text="Go" OnClick="btnCreateTopic_Click" CssClass="submitButton"/>
 </div>
     </fieldset>
    <script language="javascript">
        function topictiny() {

            if ($('#<%= TextBox1.ClientID %>').length != 0) {
                tinyMCE.init({
                    mode: "exact",
                    elements: "<%= TextBox1.ClientID %>",
                    content_css: "/css/fonts.css",
                    auto_resize: true,
                    theme: "simple",
                    remove_linebreaks: false
                });

            }
        }
    </script>

</asp:Panel>
</div>

</asp:Panel>

<asp:Panel ID="pnlNotLoggedIn" runat="server"  CssClass="notice" Visible="false">

<p><a href="/member/login" title="login">Login</a> or <a href="/member/signup" title="create">create a profile</a> to find members near you, or to locate
members in a different location.</p>

</asp:Panel>
