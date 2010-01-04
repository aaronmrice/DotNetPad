<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	.NET Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% var paste = ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model); %>
    <div class="code">
        <table class="outer">
            <tr><td style="width: 30px;">
                <% int linesOfCode = paste.Code.Split(new string[] { "\n" }, StringSplitOptions.None).Length; %>
                    <table class="linenumbers">
                        <%
							for (int i = 1; i <= linesOfCode; i++)
                            {
                        %>
                        <tr><td><%= i %></td></tr>
                        <%    
                            }
                        %>
                    </table>
            </td>
            <td>
                <pre class="prettyprint"><%
                     if (paste.Errors.Length == 0)
                         Response.Write(Server.HtmlEncode(paste.Code.Replace("\t", "    ").Replace("\r", "")));
                     else
                     {
                         string[] codeLines = paste.Code.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                         for (int i = 0; i < codeLines.Length; i++)
                         {
                             if (paste.Errors.Any(E => E.Line - 1 == i))
                                 Response.Write("<span class=\"highlight\">" + Server.HtmlEncode(codeLines[i]) + "</span>\n");
                             else
                                 Response.Write(Server.HtmlEncode(codeLines[i]) + "\n");
                         }
                     } %></pre>
           </td></tr>
       </table>
    </div>
<hr />


<% if (paste.ILDisassemblyText != null && paste.ILDisassemblyText.Length > 0)
   { %>
<div class="disassembly">
<a id="dissasembly-toggle" href="#">Toggle Disassembly</a>
<script type="text/javascript">
	$("#dissasembly-toggle").click(function () {
	    $(".disassembly .outer").toggle();
	});
	$(document).ready(function() {
	    $(".disassembly .outer").toggle();
	});	
</script>

<table class="outer">
    <tr><td style="width: 30px;">
        <table class="linenumbers">
            <% for (int i = 1; i <= paste.ILDisassemblyText.Count(); i++)
               { %>
            <tr><td><%= i%></td></tr>
            <% } %>
        </table>
    </td><td>
        <pre><%= string.Join("\r", paste.ILDisassemblyText.Select(ILD=>ILD.Text.Replace("\t","    "))) %></pre>
    </td></tr>
</table>

</div>
<hr />
<% } %>


<% if (paste.Output.Length > 0)
   { %>
<div class="output">
        <table>
            <tr><td>
                <% int linesOfOutput = paste.Output.Split(new string[] { "\n" }, StringSplitOptions.None).Length; %>
                    <table class="linenumbers">
                        <%
							for (int i = 1; i <= linesOfOutput; i++)
                            {
                        %>
                        <tr><td><%= i%></td></tr>
                        <%    
                            }
                        %>
                    </table>
            </td>
            <td>
<pre>
<%=  Server.HtmlEncode(paste.Output) %>
</pre>
           </td></tr>
       </table></div>
<% } %>

<% if (paste.Errors.Count() > 0)
   { %>
<div class="errors">
<pre class="errors"><% foreach (var line in paste.Errors)
   { %>
Line <%= line.Line %> : <%= Server.HtmlEncode(line.ErrorText) %><% } %></pre></div>
<% } %>
<hr />

<a href="/EditPaste/
<%= paste.Slug %>
">Fork this code</a>
<hr />
</asp:Content>
