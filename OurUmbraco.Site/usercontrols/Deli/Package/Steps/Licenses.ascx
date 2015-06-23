<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Licenses.ascx.cs" Inherits="uProject.usercontrols.Deli.Package.Steps.Licenses" %>
<asp:PlaceHolder ID="holder" runat="server" Visible="false">
    <div class="form simpleForm" id="registrationForm">
    
    <asp:PlaceHolder runat="server" ID="EditForm" Visible="false">
    <fieldset>
    <legend>Edit License Type</legend>
        <p>
              <label class="inputLabel">License Type</label>
            
              <asp:TextBox runat="server" Enabled="false" ID="EditLicenseType" CssClass="title" />
        </p>

        <p>
              <label class="inputLabel">Price (format 0.00) minium price 49.00</label>
              <asp:textbox runat="server" ID="EditLicensePrice" CssClass="required title minPrice"/>            
        </p>
        
        <div class="buttons">
            <asp:button runat="server" text="Update License" id="UpdateLicense" OnClick="UpdateLicense_Click" class="submitButton" />
        </div>
        </fieldset>
    
    
    </asp:PlaceHolder>


    <asp:Repeater ID="rp_licenses" OnItemDataBound="OnLicenseBound" Visible="false" runat="server">

    <HeaderTemplate>
        <table class="dataTable">
            <thead>
            <tr>
                <th>License Type</th>
                <th class="money">Price</th>
                <th class="right">&nbsp;</th>
            </tr>
            </thead>
            <tbody>
    </HeaderTemplate>
    <ItemTemplate>

            <tr>
                <td>
                  <asp:Literal ID="lt_type" runat="server" />
                </td>
                <td class="money">
                  <asp:Literal ID="lt_price" runat="server" />
                </td>
                <td class="right">
                  <asp:Button runat="server" ID="bt_edit" OnCommand="EditLicense" Text="Edit" CssClass="actionButton cancel" />&nbsp;
                  <asp:Button ID="bt_disableEnable" runat="server" OnCommand="DisableEnableLicense" Text="Disable" CssClass="actionButton cancel" />
                  <asp:Button Visible="false" ID="bt_delete" runat="server" OnClientClick="return confirm('Are you sure you want to delete this license?')" OnCommand="DeleteLicense" Text="Delete" CssClass="actionButton cancel" />
                </td>
            </tr>

    </ItemTemplate>
    <FooterTemplate>
            </tbody>
        </table>
    </FooterTemplate>
    </asp:Repeater>
    <small>NOTE: Only licenses types that have not been purchased by users can be deleted.  If a license has been purchased you may disable it.</small>
    <asp:PlaceHolder runat="server" ID="CreateForm">
    <fieldset>
    <legend>Create License Type</legend>
        <p>
              <label class="inputLabel">Choose License Type</label>
            
              <asp:DropDownList runat="server" id="licenseTypes" cssclass="title">
                <asp:ListItem Value="Domain" Text="Domain"/>
                <asp:ListItem Value="IP" Text="IP"/>
                <asp:ListItem Value="Unlimited" Text="Unlimited"/>
                <asp:ListItem Value="SourceCode" Text="Source Code"/>
              </asp:DropDownList>
        </p>

        <p>
              <label class="inputLabel">Price (format 0.00) minium price 49.00</label>
              <asp:textbox runat="server" ID="price" CssClass="required title minPrice" />            
        </p>
        
        <p>
            <asp:button runat="server" text="Create License" id="btn_SaveLicense" OnClick="SaveLicense_Click" CssClass="submitButton" />
        </p>

<div class="deliNotification">
    <h3>Your Vendor Key</h3>
    <asp:PlaceHolder runat="server" ID="GenKeyPlaceHolder" Visible ="false">
        <asp:linkButton runat="server" ID="GenKey" OnClick="GenKey_Click">Generate your project key</asp:linkButton>.   This will generate a unique key for your project.  This step can only be done once so click wisely.
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="DownloadKeyPlaceHolder" Visible ="false">
        <asp:linkButton runat="server" ID="DownloadKey" OnClick="DownloadKey_Click">Download your Vendor Key for your project</asp:linkButton>.   This License Key file is used in your code when you implement Deli Licensing in your project.
    </asp:PlaceHolder>
    <h3 style="margin-top:10px">Your Packages Unique Deli Id</h3>
    <p><asp:Literal runat="server" ID="uniqueId" /></p>
    </div>
    <div class="buttons">
        <asp:linkbutton runat="server" Text="Previous" ID="MovePrevious" OnClick="MoveLast" class="cancel" />&nbsp;
        <asp:Button runat="server" Text="Next" ID="MoveNext" OnClick="SaveStep"  CssClass="submitButton cancel" />
    </div>
    </fieldset>
    </asp:PlaceHolder>
    </div>
    <script type="text/javascript">

        $(document).ready(function () {
            var form = $("form");

            jQuery.validator.addMethod("minPrice", function (value, element) { return value >= 49.00 }, "Please enter a price greater than equal to 49.00");

            form.validate({
                onsubmit: false,
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