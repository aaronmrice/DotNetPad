using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gobiner.CSharpPad.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"ViewPaste",                                              // Route name
				"{action}/{id}",                           // URL with parameters
				new { controller = "Home", action = "Index"}  // Parameter defaults
			);

			routes.MapRoute(
				"RandomAction",
				"{id}",
				new { controller = "Home", action = "Show" }
			);

			routes.MapRoute(
				"Home",
				"",
				new { controller = "Home", action = "Index" }
			);

		}

		protected void Application_Start()
		{
			AppDomain.CurrentDomain.SetData("DataDirectory", Server.MapPath("~/App_Data"));
			RegisterRoutes(RouteTable.Routes);
			//RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
		}
	}
}