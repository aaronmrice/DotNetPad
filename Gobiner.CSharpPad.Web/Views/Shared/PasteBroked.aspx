<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	You Broke .NET Pad! | .NET Pad
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Good Job!</h2>
	<p>
		You pasted in some code that broke .NET pad. It's ok, though. This error and your paste has been logged, so maybe it'll get fixed.
		If you really care, create an issue on bitbucket to let me know.
	</p>

</asp:Content>
