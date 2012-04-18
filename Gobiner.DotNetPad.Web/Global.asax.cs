using System;
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
				"ViewPaste",                                          // Route name
				"{action}/{id}",                                      // URL with parameters
				new { controller = "Home", action = "Paste"}      // Parameter defaults
			);

			routes.MapRoute(
				"RandomAction",
				"{action}",
				new { controller = "Home", action = "Index" }
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