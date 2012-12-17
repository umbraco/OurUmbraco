<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SystemItemSelector.ascx.cs" Inherits="Umbraco.Courier.UI.Usercontrols.SystemItemSelector" %>

<asp:Repeater ID="rpItems" runat="server" OnItemDataBound="RenderItemChildren">

                <ItemTemplate>
                        <div class="item" parent="<%= ParentID %>">

                            <input type="checkbox" 
                                    id="<%# ((Umbraco.Courier.Core.SystemItem)Container.DataItem).ItemId.ToString() %>" 
                                    rel="<%# ((Umbraco.Courier.Core.SystemItem)Container.DataItem).Name %>" 
                                    parent="<%= ParentID %>"
                                    class="itemchb" />

                            <img src="<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco + "/images/umbraco/")%><%# ((Umbraco.Courier.Core.SystemItem)Container.DataItem).Icon %>" />

                            <%# ((Umbraco.Courier.Core.SystemItem)Container.DataItem).Name %>

                                <asp:PlaceHolder ID="phChildren" runat="server"></asp:PlaceHolder>
                        </div>
                </ItemTemplate>
</asp:Repeater>