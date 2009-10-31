<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	EditPaste
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm((new { Controller = "Home", Action = "Submit" })))
   { %>
		<%= Html.TextArea("Code", ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code, new { Rows = 15, @class = "code" }) %>
		
		<br />
		<button type="submit" value="Submit">
			<span>Submit</span>
		</button>
		
		<input type="checkbox" name="IsPrivate" id="IsPrivate" />
		<label for="checkbox">Private paste (won't appear in list of recent pastes)</label>
<% } %>

</asp:Content>
