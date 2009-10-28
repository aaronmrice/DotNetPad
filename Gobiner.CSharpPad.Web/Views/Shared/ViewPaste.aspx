<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="code">
        <table>
            <tr><td>
                <% string[] Code = ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code.Split(new string[] { Environment.NewLine}, StringSplitOptions.None); %>
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
                <table>
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

<% if (((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output.Length > 0)
   { %>
<div class="output">
        <table>
            <tr><td>
                <% string[] Output = ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None); %>
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

<a href="/EditPaste/
<%= ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Slug %>
">Fork this code</a>
<hr />
</asp:Content>
