<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	.NET Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% var paste = ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model); %>
	<img src="../../Content/img/blorgons.png" alt="It's 'Blorgons', with an 'r'" />
	<form action="/OhTheHumanity" method="post">
		<input type="hidden" name="Slug" value="<%= paste.Slug %>" />
		<input type="text" name="Email" id="Email" />
        <input type="text" name="Website" id="Website" />
		<button type="submit">
			I am not a number, I'm a free man!
		</button>
	</form>
</asp:Content>
