<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CartDetails.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Cart.CartDetails" %>
<%@ Register Assembly="Marketplace" TagPrefix="Deli" Namespace="Marketplace.Controls" %>
<%@ Register Src="~/usercontrols/our.umbraco.org/Login_novalidationscript.ascx" TagName="Login" TagPrefix="our" %>

<asp:PlaceHolder ID="NotLoggedIn" runat="server" Visible="false">

<div class="deliNotification">
<h2>Continue without logging in</h2>
    <p>You can complete your purchase now, we'll create an account for you and email you a temporary password when you're done.</p>
<h2>Or if you are an existing member <a href="#" class="show_hide">login here</a></h2>
<p style="margin-bottom:0;">Your profile probably already has most of the information we need to process your purchase. Login to retrieve your details.</p>
<div id="detailsLoginForm">
    <div id="detailLogin"><our:Login runat="server" ID="MemberLogin" ErrorMessage="Either your email or password is incorrect" /></div>
    <p>Can't remember your password? You can retrieve a new one by <a href="<%= RetrievePasswordPage %>" title="Forgot Password">clicking here</a></p>
</div>
</div> 



<script type="text/javascript">

    $(document).ready(function () {

        $("#detailsLoginForm").hide();
        $(".show_hide").show();

        $('.show_hide').click(function () {
            $("#detailsLoginForm").slideToggle();
        });

    });

</script>

</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="DetailsForm">
<script type="text/javascript" src="/scripts/jsvat-custom-deli.js"></script>
<div id="cartDetailsForm">
<fieldset>
<legend>User Information</legend>
 <p>
 <asp:Label runat="server" AssociatedControlID="UserName" CssClass="inputLabel">Name</asp:Label>
<asp:textbox runat="server" ID="UserName" CssClass="required title" ToolTip="Please enter your name" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="UserEmail" CssClass="inputLabel">Email</asp:Label>
<asp:TextBox runat="server" ID="UserEmail" CssClass="required title email" ToolTip="Please enter your email address" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="UserConfirmEmail" CssClass="inputLabel">Confirm Email</asp:Label>
<asp:TextBox runat="server" ID="UserConfirmEmail" CssClass="CompareFields title" ToolTip="Please confirm your email address" />
</p>

</fieldset>
<fieldset>

<legend>Company Details and Invoicing</legend>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyName" CssClass="inputLabel">Company Name</asp:Label>
<asp:TextBox runat="server" ID="CompanyName" CssClass="title" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyAddress" CssClass="inputLabel">Address</asp:Label>
<asp:TextBox runat="server" TextMode="MultiLine" ID="CompanyAddress" CssClass="title" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyCountry" CssClass="inputLabel">Country</asp:Label>
<Deli:CountryDropDownList runat="server" ID="CompanyCountry" CssClass="title required" ToolTip="Please select your country" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyPhone" CssClass="inputLabel" Visible="false">Phone</asp:Label>
<asp:TextBox runat="server" ID="CompanyPhone" CssClass="title" Visible="false" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyVat" CssClass="inputLabel">VAT-Number</asp:Label>
<asp:TextBox runat="server" ID="CompanyVat" CssClass="ValidateVatFormat title" ToolTip="VAT format is invalid. Include country code and no spaces AA1111111111" />
</p>
<p>
<asp:Label runat="server" AssociatedControlID="CompanyInvoiceEmail" CssClass="inputLabel">Invoice Email</asp:Label>
<asp:TextBox runat="server" ID="CompanyInvoiceEmail" CssClass="title email required" />
</p>




<div class="buttons">
    <asp:linkbutton runat="server" Text="&lt; Previous" ID="CartNavPrevious" OnClick="CartNavPrevious_Click" />&nbsp;
    <asp:Button runat="server" Text="Next &gt;" ID="CartNavNext" OnClick="CartNavNext_Click" class="submitButton"/>
</div>

</fieldset>
</div>
<script type="text/javascript">



    $(document).ready(function () {

        $("#detailLogin input[type=submit]").addClass("cancel");

        jQuery.validator.addMethod("CompareFields", function (value, element) { return value == $("#<%= UserEmail.ClientID %>").val(); }, $("#<%= UserEmail.ClientID %>").attr("tooltip"));

        jQuery.validator.addMethod("ValidateVatFormat", function (value, element) { if(value.length > 0){return checkVATNumber (value);}else{ return true;} }, $("#<%= CompanyVat.ClientID %>").attr("tooltip"));

        $("form").validate({
            invalidHandler: function (f, v) {
                var errors = v.numberOfInvalids();
                if (errors.length > 0) {
                    validator.focusInvalid();
                }
            }

        });




        $(".submitButton").click(function (evt) {
            // Validate the form and retain the result.
            var isValid = $("form").valid();

            // If the form didn't validate, prevent the
            //  form submission.
            if (!isValid)
                evt.preventDefault();
        });

    });

</script>
</asp:PlaceHolder>

<asp:Literal ID="DeliMessage" runat="server"></asp:Literal>


