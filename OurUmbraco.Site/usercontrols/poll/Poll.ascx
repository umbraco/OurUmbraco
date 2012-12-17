<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Poll.ascx.cs" Inherits="Nibble.Umb.Poll.Poll" %>

<%@ Register Assembly="System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

        
<asp:UpdatePanel ID="UpdatePanelPoll" runat="server">
    <ContentTemplate>
        <div class="pollcontainer">
            <div class="pollquestion">
                <asp:Literal ID="lblQuestion" runat="server"></asp:Literal></div>
            <div class="pollinfo"><asp:Literal ID="lblInfo" runat="server"></asp:Literal></div>
            <div class="poll">
                <asp:Panel ID="pnlQuestion" runat="server" Visible="false">
                    <div class="pollawnsers">
                        <asp:Panel ID="pnlAnswers" runat="server">
                        </asp:Panel>
                    </div>
                    <div id="pollsubmit<%= btnSubmit.ClientID %>" class="pollsubmit">
                        <asp:Button ID="btnSubmit" runat="server" Text="Cast your vote!" OnClick="btnSubmit_Click" CssClass="castvote"/>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlResults" runat="server" Visible="false">
                    <dl>
                        <asp:Literal ID="Literal3" runat="server"></asp:Literal>
                    </dl>
            
                </asp:Panel>
                  <asp:Panel ID="pnlLogin" runat="server" Visible="false">
                    <p>Please login to cast your vote.</p>
                  </asp:Panel>
                  <asp:Panel ID="pnlThanks" runat="server" Visible="false">
                    <p>Thanks for your vote!</p>
                  </asp:Panel>
                   <asp:Panel ID="pnlAllreadyVotes" runat="server" Visible="false">
                    <p>You have allready voted.</p>
                  </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

  <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanelPoll" DisplayAfter="500">
    <ProgressTemplate>
        <div>
            Submitting vote…
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>

<script>
function hideSubmit<%= this.ClientID %>()
{
    if(document.getElementById('<%= btnSubmit.ClientID %>') != null)
    {
        document.getElementById('<%= btnSubmit.ClientID %>').style.visibility = 'hidden';
    }
    
    if(document.getElementById('pollsubmit<%= btnSubmit.ClientID %>') != null)
    {
        document.getElementById('pollsubmit<%= btnSubmit.ClientID %>').style.display='none';
    }
}
</script>
