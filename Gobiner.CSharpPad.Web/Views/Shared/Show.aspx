<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="code">
        <table>
            <tr><td>
                <% string[] Lines = Server.HtmlEncode(((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code).Split(new string[] { Environment.NewLine}, StringSplitOptions.None); %>
                    <table class="linenumbers">
                        <tr><td>&nbsp;</td></tr>
                        <%
                            for (int i = 1; i <= Lines.Length; i++)
                            {
                        %>
                        <tr><td><%= i%></td></tr>
                        <%    
                            }
                        %>
                    </table>
            </td>
            <td style="background:black;">
            </td>
            <td>
                <pre>
                    <table>
                        <%
                            for (int i = 0; i < Lines.Length; i++)
                            {
                        %>
                        <tr><td><%= Lines[i]%></td></tr>
                        <%
                            }
                        %>
                    </table>
                </pre>
           </td></tr>
       </table>
    </div>
<hr />

<% if (((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output.Length > 0)
   { %>
<div class="output">
<pre class="output">
<% foreach (var line in Server.HtmlEncode(((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output).Split(new string[] { Environment.NewLine }, StringSplitOptions.None)) { %>
<%= line%>
<% } %></pre></div>
<% } %>


<% if (((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Errors.Count() > 0)
   { %>
<div class="errors">
<pre class="errors">
<% foreach (var line in ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Errors)
   { %>
<%= line.ErrorText%>
<% } %></pre></div>
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
