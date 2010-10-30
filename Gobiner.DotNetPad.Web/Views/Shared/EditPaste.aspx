<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit Paste | .NET Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm((new { Controller = "Home", Action = "Submit" })))
   { %>
		<%= Html.TextArea("Code", ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Code, new { Rows = 15, @class = "code" }) %>
		
		<br />
				
		<label>
			<input type="radio" name="Language" value="CSharp" <%= ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Language == Gobiner.CSharpPad.Language.CSharp ? "checked='true'" : "" %> />
			<span>C#</span>
		</label>
		<label>
			<input type="radio" name="Language" value="VisualBasic" <%= ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Language == Gobiner.CSharpPad.Language.VisualBasic ? "checked='true'" : "" %> />
			<span>Visual Basic</span>
		</label>
		<label>
			<input type="radio" name="Language" value="FSharp" <%= ((Gobiner.CSharpPad.Web.Models.Paste)ViewData.Model).Language == Gobiner.CSharpPad.Language.FSharp ? "checked='true'" : "" %> />
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
