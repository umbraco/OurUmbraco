<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyProjects.ascx.cs" Inherits="Marketplace.usercontrols.Deli.Profile.MyProjects" %>
<%@ Import Namespace="Marketplace.Interfaces" %>
<%@ Import Namespace="Marketplace" %>
<div class="profile-settings">
    <strong>My Packages</strong>
    <div class="profile-settings-packages packages-content">
        <a href="<%= editUrl %>" class="button green"><i class="icon-Add"></i>Add package</a>
        <asp:Repeater runat="server" ID="myProjects">
            <ItemTemplate>

                <div class="box liked">

                    <div class="row">

                        <div class="col-xs-2 col-md-1">
                            <img src="/umbraco/imagegen.ashx?image=<%# GetIcon((string)Eval("DefaultScreenshot"))%>&pad=true&width=65&height=65 " alt="">
                        </div>

                        <div class="col-xs-10 col-md-6">
                            <div class="forum-thread-text">
                                <h3><a href="<%# Eval("NiceUrl") %>"><%# Eval("Name") %></a></h3>
                                <p>
                                     <%#  Eval("Description").ToString().StripHtmlAndLimit(45) %> ...
                                    <br />
                                    <a href="<%= editUrl %>?id=<%# Eval("Id") %>">Edit</a>
                                    <a href="<%= forumUrl %>?id=<%# Eval("Id") %>">Forums</a>
                                </p>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-5">
                            <div class="other">

                                <div class="package-badge">
                                    <span class="package-name">version</span>
                                    <span class="package-number"><%# Eval("CurrentVersion") %></span>
                                </div>

                                <span class="stats">
                                    <span class="downloads">
                                        <%# Eval("Downloads") %><span><i class="icon-Download-alt"></i></span>
                                    </span>

                                    <span class="karma">
                                        <%# Eval("Karma") %><span><i class="icon-Hearts"></i></span>
                                    </span>
                                </span>
                            </div>
                        </div>

                    </div>

                </div>







            </ItemTemplate>
        </asp:Repeater>


        <strong>Package Contributions</strong>
        <asp:Repeater runat="server" ID="myTeamProjects">

            <ItemTemplate>

               <div class="box liked">

                    <div class="row">

                        <div class="col-xs-2 col-md-1">
                            <img src="/umbraco/imagegen.ashx?image=<%# GetIcon((string)Eval("DefaultScreenshot"))%>&pad=true&width=65&height=65 " alt="">
                        </div>

                        <div class="col-xs-10 col-md-6">
                            <div class="forum-thread-text">
                                <h3><a href="<%# Eval("NiceUrl") %>"><%# Eval("Name") %></a></h3>
                                <p>
                                     <%#  Eval("Description").ToString().StripHtmlAndLimit(45) %>

                                    <br />
                                    <a href="<%= editUrl %>?id=<%# Eval("Id") %>">Edit</a>
                                    <a href="<%= forumUrl %>?id=<%# Eval("Id") %>">Forums</a>
                                </p>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-5">
                            <div class="other">

                                <div class="package-badge">
                                    <span class="package-name">version</span>
                                    <span class="package-number"><%# Eval("CurrentVersion") %></span>
                                </div>

                                <span class="stats">
                                    <span class="downloads">
                                        <%# Eval("Downloads") %><span><i class="icon-Download-alt"></i></span>
                                    </span>

                                    <span class="karma">
                                        <%# Eval("Karma") %><span><i class="icon-Hearts"></i></span>
                                    </span>
                                </span>
                            </div>
                        </div>

                    </div>

                </div>

            </ItemTemplate>

        </asp:Repeater>
    </div>
</div>


