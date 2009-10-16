<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<pre id="code">
<% foreach(var line in Server.HtmlEncode(((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code).Split(new string[] { Environment.NewLine}, StringSplitOptions.None)) { %>
<%= line %>
<% } %>
</pre>
<hr />

<% if (((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output.Length > 0)
   { %>
<pre id="output">
<% foreach (var line in Server.HtmlEncode(((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output).Split(new string[] { Environment.NewLine }, StringSplitOptions.None)) { %>
<%= line%>
<% } %></pre>
<% } %>


<% if (((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Errors.Count() > 0)
   { %>
<pre id="errors">
<% foreach (var line in ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Errors)
   { %>
<%= line.ErrorText%>
<% } %></pre>
<% } %>
<hr />


<% using (Html.BeginForm(new { Controller = "Home", Action = "Index" })) { %>
		
		<%= Html.TextArea("Code", ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code, new { Rows = 15, @class = "code" })%>
		
		<br />
		
		<button type="submit" value="Submit">
			<span>Submit</span>
		</button>
    
<% } %>
</asp:Content>
