<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Forgotpassword.ascx.cs" Inherits="our.usercontrols.Forgotpassword" %>
<asp:Panel runat="server" ID="message" Visible="False">
    <div class="notice">
        <p style="color: green;">A new password has been sent to the email address: <asp:Literal ID="lt_email" runat="server" /></p>
        <p>Go to the <a href="/member/login">login page</a></p>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="error" Visible="False">
    <div class="notice" style="color: red;">
        <p>Something went totally wrong trying to reset your password, <a href="https://umbraco.com/about-us/team">try to reach out to us!</a></p>
    </div>
</asp:Panel>

<asp:Panel runat="server" DefaultButton="bt_login">

    <div class="form simpleForm" id="registrationForm">
        <fieldset>
            <asp:Panel runat="server" ID="retrieve_error" Visible="False">
                <p style="color: red;">We couldn't find an account with this email address</p>
            </asp:Panel>

            <p>
                <asp:Label ID="Label2" AssociatedControlID="tb_email" CssClass="inputLabel" runat="server">Email</asp:Label>
                <asp:TextBox ID="tb_email" runat="server" ToolTip="Please enter your email address" CssClass="required email title" />
            </p>

        </fieldset>

        <div class="buttons">
            <asp:Button ID="bt_login" OnClick="sendPass" CssClass="submitButton" runat="server" Text="Retrieve password" />
        </div>

    </div>

    <script type="text/javascript">
        $(document).ready(function () {
            $("form").validate();
        });
    </script>
</asp:Panel>
