<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% var paste = ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model); %>
    <div class="code">
        <table class="outer">
            <tr><td style="width: 30px;">
                <% string[] Code = paste.Code.Split(new string[] { Environment.NewLine}, StringSplitOptions.None); %>
                    <table class="linenumbers">
                        <%
							for (int i = 1; i <= Code.Length; i++)
                            {
                        %>
                        <tr><td><a href="#<%= i %>"><%= i %></a></td></tr>
                        <%    
                            }
                        %>
                    </table>
            </td>
            <td style="background:black; padding: 0 0 0 2px;">
            </td>
            <td>
                <table class="inner">
                    <%
						for (int i = 0; i < Code.Length; i++)
                        {
                    %>
                    <tr><td id="<%= i+1 %>"><pre class="prettyprint"><%= Server.HtmlEncode(Code[i])%></pre></td></tr>
                    <%
                        }
                    %>
                </table>
           </td></tr>
       </table>
    </div>
<hr />

<% if (paste.Output.Length > 0)
   { %>
<div class="output">
        <table>
            <tr><td>
                <% string[] Output = paste.Output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None); %>
                    <table class="linenumbers">
                        <%
							for (int i = 1; i <= Output.Length; i++)
                            {
                        %>
                        <tr><td><%= i%></td></tr>
                        <%    
                            }
                        %>
                    </table>
            </td>
            <td style="background:black; padding: 0 0 0 2px;">
            </td>
            <td>
                <table>
                    <%
						for (int i = 0; i < Output.Length; i++)
                        {
                    %>
                    <tr><td><pre><%= Server.HtmlEncode(Output[i])%></pre></td></tr>
                    <%
                        }
                    %>
                </table>
           </td></tr>
       </table></div>
<% } %>

<% if (paste.Errors.Count() > 0)
   { %>
<div class="errors">
<pre class="errors">
<% foreach (var line in paste.Errors)
   { %>
<%= line.ErrorText%>
<% } %></pre></div>
<% } %>
<hr />

<a href="/EditPaste/
<%= paste.Slug %>
">Fork this code</a>
<hr />
</asp:Content>
