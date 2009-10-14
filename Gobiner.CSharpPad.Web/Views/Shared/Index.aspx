<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	C# Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <% using (Html.BeginForm(new { Controller = "Home", Action = "Submit" }))
	   { %>
		<%= Html.TextArea("Code", "", 15, 50, null)%>
		
		<br />
		
		<button type="submit" value="Submit">
			<span>Submit</span>
		</button>
    
    
    
    <% } %>

</asp:Content>
