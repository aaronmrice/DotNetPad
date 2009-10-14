<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<pre id="code">
<% foreach(var line in ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code.Split(new string[] { Environment.NewLine}, StringSplitOptions.None)) { %>
<%= line %>
<% } %>
</pre>
<hr />
<pre id="output">
<% foreach(var line in ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output.Split(new string[] { Environment.NewLine}, StringSplitOptions.None)) { %>
<%= line %>
<% } %>
</pre>
<pre id="errors">
<% foreach(var line in ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Errors) { %>
<%= line.ErrorText %>
<% } %>
</pre>
</asp:Content>
