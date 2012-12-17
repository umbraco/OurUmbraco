<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EventMailer.ascx.cs" Inherits="our.usercontrols.EventMailer" %>

<asp:PlaceHolder id="ph_holder" runat="server" Visible="false">
<div class="form simpleForm eventForm">
    <fieldset>
    <legend>Receivers</legend>
      <p>
        Choose your audience
      </p>
      <p>
      <asp:RadioButtonList runat="server" ID="rbl_receivers">
            <asp:ListItem Text="People who have signed up" Value="coming" Selected="True" />
            <asp:ListItem Text="Only people on the waitinglist" Value="waiting" />
            <asp:ListItem Text="Everybody, both those signed up and those on the waitinglist" Value="both" />
      </asp:RadioButtonList>
      </p>
    </fieldset>
    
    <fieldset>
    <legend>The message</legend>
    <p>
        The contents of the email, you want to send out. The email sender name and address will be the ones you use
        on our.umbraco.org and you can therefore expect some replies from your event participants
    </p>
    
    <p>
        <asp:Label ID="Label1" runat="server" CssClass="inputLabel" AssociatedControlID="tb_subject">Subject</asp:Label>
        <asp:TextBox runat="server" style="width: 540px" id="tb_subject" ToolTip="Please enter the email subject" CssClass="required title" />
    </p>
    <p>
        <asp:Label ID="Label2" CssClass="inputLabel" AssociatedControlID="tb_body" runat="server">Body</asp:Label>
        <asp:TextBox runat="server" ID="tb_body" style="width: 550px; height: 450px;" ToolTip="Please enter the body text of the email" CssClass="title" />
    </p>

    </fieldset>


    
    
    <div class="buttons">
        <asp:Button ID="bt_submit" Text="send emails" CssClass="submitButton" OnClick="SendEmail" runat="server" />
    </div>
    
    <br />
    
    <div class="notice">
        <p>
            You are only allowed to send <%= MaxNumberOfEmails %> additional emails out for this event
        </p>
    </div>
    

</div>


<script type="text/javascript">
    $(document).ready(function () {
        $("form").validate();
    });

    tinyMCE.init({
        // General options
        mode: "exact",
        elements: "<%= tb_body.ClientID %>",
        content_css: "/css/fonts.css",
        auto_resize: true,
        theme: "simple",
        remove_linebreaks: false
    });
</script>
</asp:PlaceHolder>