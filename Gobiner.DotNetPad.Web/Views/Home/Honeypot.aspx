<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
	Spam Detected
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <h2>Spam detected</h2>
    <p>Looks like you fell into our little trap. If you're not actually a spambot, next time don't put values into our honeypot fields.</p>

</asp:Content>
