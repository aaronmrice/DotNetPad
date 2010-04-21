<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	.NET Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<p>.NET Pad is a <a href="http://www.codepad.org/">codepad</a> clone for .NET languages. Paste your code in and share it!</p>
    
    
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
		
		<label>
			<input type="radio" name="Language" value="CSharp" checked="true" />
			<span>C#</span>
		</label>
		<label>
			<input type="radio" name="Language" value="VisualBasic" />
			<span>Visual Basic</span>
		</label>
		<label>
			<input type="radio" name="Language" value="FSharp" />
			<span>F#</span>
		</label>
		<br />
		<button type="submit" value="Submit">
			<span>Submit</span>
		</button>
	
		<input type="checkbox" name="IsPrivate" id="IsPrivate" />
		<label for="checkbox">Private paste (won't appear in list of recent pastes)</label>
    
    <% } %>

</asp:Content>
