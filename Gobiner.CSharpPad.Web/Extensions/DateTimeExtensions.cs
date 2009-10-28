using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace Gobiner.CSharpPad.Web.Extensions
{
	public static class DateTimeExtensions
	{
		public static string PrettyPrintTimeAgo(this DateTime thePast)
		{
			var timespan = DateTime.Now - thePast;
			if (timespan.Days > 1)
			{
				return timespan.Days + " days ago";
			}
			if (timespan.Days == 1)
			{
				return "1 day ago";
			}
			if (timespan.Hours > 1)
			{
				return timespan.Hours + " hours ago";
			}
			if (timespan.Hours == 1)
			{
				return "1 hour ago";
			}
			if (timespan.Minutes > 1)
			{
				return timespan.Minutes + " minutes ago";
			}
			if (timespan.Minutes == 1)
			{
				return "1 minute ago";
			}
			else
			{
				return timespan.Seconds + " seconds ago";
			}
		}
	}
}
