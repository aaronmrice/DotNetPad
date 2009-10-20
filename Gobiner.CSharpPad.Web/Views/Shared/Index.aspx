<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<p>C# Pad is a <a href="http://www.codepad.org/">codepad</a> clone for C#. Paste your code in and share it.</p>
    
    
    <% using (Html.BeginForm(new { Controller = "Home", Action = "Submit" }))
	   { %>
		<%= Html.TextArea("Code", @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Default
{
	public static void Main(string[] args)
	{
		
	}	
}
", new { @class="code" } )%>
		
		<br />
		
		<button type="submit" value="Submit">
			<span>Submit</span>
		</button>
    
    
    
    <% } %>

</asp:Content>
