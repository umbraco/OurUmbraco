<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DependencySelector.ascx.cs" Inherits="Umbraco.Courier.UI.Usercontrols.DependencySelector" %>


<asp:Repeater ID="rpDependencies" runat="server" OnItemDataBound="RenderItemChildren">

                <ItemTemplate>
                        <div class="dependency">

                              <input type="checkbox" id="<%# ((Umbraco.Courier.Core.Dependency)Container.DataItem).ItemId.ToString() %>" class="depchb" />

                              <%# ((Umbraco.Courier.Core.Dependency)Container.DataItem).Name %>

                            <asp:PlaceHolder ID="phChildren" runat="server"></asp:PlaceHolder>

                        </div>

                </ItemTemplate>
</asp:Repeater>