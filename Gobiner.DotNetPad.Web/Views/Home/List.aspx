<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Gobiner.CSharpPad.Web.Extensions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Recent Pastes | .NET Pad</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% foreach (var paste in ((IEnumerable<Gobiner.CSharpPad.Web.Models.Paste>)ViewData.Model)) { %>
		<div class="code">
			<% IEnumerable<string> Code = paste.Code.Split(new string[] { "\n" }, StringSplitOptions.None);
				Code = Code.Where(x => Regex.Match(x,"\\S").Success);
				Code = Code.SkipWhile(x => x.StartsWith("using "));
				Code = Code.Take(10); %>

<pre class="prettyprint">
<% foreach(var line in Code) { %>
<%= Server.HtmlEncode(line.Replace("\t", "    ").Replace("\r",""))%>
<% } %>
</pre>

		</div>
		<p style="padding-bottom:10px"><%= Html.ActionLink("Pasted " + paste.Created.PrettyPrintTimeAgo(), "ViewPaste", new { Id = paste.Slug } ) %></p>
	
	<% } %>
</asp:Content>
