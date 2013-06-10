<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RenderForm.ascx.cs" Inherits="Umbraco.Forms.UI.Usercontrols.RenderForm" %>
<%@ Register Assembly="Umbraco.Forms.Core" Namespace="Umbraco.Forms.Core.Controls.Validation" TagPrefix="uform" %>

<asp:PlaceHolder ID="ph_noFormWarning" runat="server" Visible="false">
    <div style="border: 2px solid red; padding: 10px; text-align: center; color: Red;">
        Umbraco Contour will only work properly if placed inside a &lt;form runat="server"&gt; tag.
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder id="ph_styles" runat="server" />

<asp:PlaceHolder id="placeholder" runat="server" Visible="true">


<div id='contour' <asp:Literal ID="pageCssClass" runat="server" />>

    <asp:Literal ID="pageName" runat="server" />
    <asp:ValidationSummary ID="validationsummary" runat="server"  Enabled="false" CssClass="contourValidationSummary"/>
   
     <asp:Repeater ID="rpFieldsets" runat="server" OnItemDataBound="RenderFieldset">
            <ItemTemplate>
                <fieldset class='contourFieldSet <asp:Literal ID="cssClass" runat="server" />'>
                    <asp:Literal ID="legend" runat="server" />
                    
                    <asp:Repeater id="rpFields" runat="server" OnItemDataBound="RenderField">
                        <ItemTemplate>
                            
                            <div class='contourField <asp:Literal ID="cssClass" runat="server" />'>
                            
                             <!-- Our label -->
                            <asp:Label ID="label" CssClass="fieldLabel" runat="server"/>
                                       
                            <div>                           
                            <!-- The data entry control -->
                            <asp:PlaceHolder ID="placeholder" runat="server" />
                            </div>
                            
                             <!-- Our Tooltip -->                            
                            <asp:Literal ID="tooltip" runat="server" />
                                                                                       
                            <!-- Validation -->
                            <uform:RequiredValidator ID="mandatory" runat="server" ErrorMessage="mandatory" Visible="false" Display="Dynamic" CssClass="contourError" />
                            <uform:RegexValidator ID="regex" ErrorMessage="The field could not be validated" runat="server" Visible="false" Display="Dynamic" CssClass="contourError" />
                            
                                                       
                            <br style="clear: both;" />
                            </div>
                                                   
                        </ItemTemplate>
                    </asp:Repeater> 
                                        
                </fieldset>
                
            </ItemTemplate>
                
    </asp:Repeater>

<div class="contourNavigation">
    <asp:Button ID="b_prev" runat="server" OnClick="prevPage" CssClass="contourButton contourPrev" Text="Previous"/>
     <asp:Button ID="b_next" runat="server" OnClick="nextPage" CssClass="contourButton contourNext" Text="Next"/>
</div>
</div>  
  
</asp:PlaceHolder>

<asp:PlaceHolder ID="ph_messageOnSubmit" runat="server" Visible="false">
<p class="contourMessageOnSubmit">
    <asp:Literal ID="lt_message" runat="server" />    
</p>
</asp:PlaceHolder>


