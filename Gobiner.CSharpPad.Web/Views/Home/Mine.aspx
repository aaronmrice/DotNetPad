<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	My Recent Pastes
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>You haven't pasted anything from this browser!</h2>
    
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
	
		<input type="checkbox" name="IsPrivate" id="IsPrivate" />
		<label for="checkbox">Private paste (won't appear in list of recent pastes)</label>
    
    <% } %>

</asp:Content>
