<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyProjects.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Profile.MyProjects" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<div class="profileProjectsHolder">
<h3>Your existing projects</h3>
<ul class="profileProjects">
<asp:Repeater runat="server" ID="myProjects">
<ItemTemplate>
    <li>
    <h3><a href="<%# Eval("NiceUrl") %>">
      <%# Eval("Name") %>
    </a></h3>
        <img src="/umbraco/imagegen.ashx?image=<%# GetIcon((string)Eval("DefaultScreenshot")) %>&pad=true&width=50&height=50" alt="<%# Eval("Name") %>" class="projectIcon" style="border:1px solid #ddd"/>
    <small>
      <%# ((DateTime)Eval("CreateDate")).ToString("D")%>
    </small>
    <ul class="projectNav">
        <li><a href="<%= editUrl %>?id=<%# Eval("Id") %>">Edit</a></li>
        <li><a href="<%= forumUrl %>?id=<%# Eval("Id") %>">Forums</a></li>
        

        <asp:placeholder runat="server" Visible='<%# (((ListingType)Eval("ListingType")) == ListingType.commercial)?true:false %>'>
            <li><a href="<%= licenseUrl %>?id=<%# Eval("Id") %>">Licenses</a></li> 
        </asp:placeholder>
        
        <asp:placeholder ID="Placeholder1" runat="server" Visible='<%# (bool)Eval("OpenForCollab") %>'>
            <li><a href="<%= teamUrl %>?id=<%# Eval("Id") %>">Team</a></li>       
         </asp:placeholder>
    </ul>
  </li>

</ItemTemplate>
</asp:Repeater>
    <li class="add">
        <h3><a href="<%= editUrl %>">Add new project</a></h3>
        <img src="/css/img/add.png" alt="Add new project" />
    </li>
</ul>

<asp:Repeater runat="server" ID="myTeamProjects">
<HeaderTemplate>
<h3>Project where you are contributor</h3>
<ul class="profileProjects">
</HeaderTemplate>
<ItemTemplate>
    <li>
    <h3><a href="<%# Eval("NiceUrl") %>">
      <%# Eval("Name") %>
    </a></h3>
        <img src="/umbraco/imagegen.ashx?image=<%# GetIcon((string)Eval("DefaultScreenshot")) %>&pad=true&width=50&height=50" alt="<%# Eval("Name") %>" class="projectIcon" style="border:1px solid #ddd"/>
    <small>
      <%# ((DateTime)Eval("CreateDate")).ToString("D") %><br />
      Owner: <a href="mailto:<%# ((IVendor)Eval("Vendor")).Member.Email %>"><%# ((IVendor)Eval("Vendor")).Member.Name %></a>
    </small>
    <ul class="projectNav">
        <li><a href="<%= editUrl %>?id=<%# Eval("Id") %>">Edit</a></li>
        <li><a href="<%= forumUrl %>?id=<%# Eval("Id") %>">Forums</a></li>
        

        <asp:placeholder runat="server" Visible='<%# (((ListingType)Eval("ListingType")) == ListingType.commercial)?true:false %>'>
            <li><a href="<%= licenseUrl %>?id=<%# Eval("Id") %>">Licenses</a></li> 
        </asp:placeholder>
    </ul>
  </li>
  </ItemTemplate>
<FooterTemplate>
</ul>
</FooterTemplate>
</asp:Repeater>
</div>

<asp:LoginView ID="LoginView1" runat="server">
<RoleGroups>
    <asp:RoleGroup Roles="Deli Vendor">
    <ContentTemplate>
    <div class="deliNotification sidebarNotification">
        <h3>Vendor Resources</h3>
        <p>The following resources will help you get started as a Deli Vendor.  If you have any questions regarding Deli or the commercialization of your package please <a href="http://umbraco.com/contact">contact the Umbraco HQ</a></p>
        <ul class="linkList icons">
            <li><img src="/css/img/vsproject.png" /><strong><a href="/media/1095480/projectlicensing.zip">Download the sample project</a></strong><br />
            <small>This sample project will give you start on integrating Deli Licensing into your project</small></li>
            <li><img src="/css/img/terms.png" /><strong><a href="/wiki/deli/deli-vendor-terms/">Deli Terms &amp; Conditions</a></strong><br />
            <small>Make sure you read and fully understand the deli licensing agreement</small></li>
            <li><img src="/css/img/terms.png" /><strong><a href="/wiki/deli/umbraco-deli-package-license-agreement-standard/">Deli Package License Agreement</a></strong><br />
            <small>Make sure you read and fully understand the deli licensing agreement</small></li>
            <li><img src="/css/img/info.png" /><strong><a href="/wiki/deli/deli-vendor-faq/">Deli FAQ's</a></strong><br />
            <small>View a selection of answers to common Deli Vendor Questions</small></li>
            <li><img src="/css/img/help.png" /><strong><a href="/member/profile/support-request/">Deli Support</a></strong><br />
            <small>Do you have any questions or issues please contact us via the Deli Vendor Support form.</small></li>
        </ul>
    </div>
    </ContentTemplate>
    </asp:RoleGroup>
</RoleGroups>
<LoggedInTemplate>
</LoggedInTemplate>
</asp:LoginView>
