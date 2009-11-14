using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Gobiner.CSharpPad.Web.Models;

namespace Gobiner.CSharpPad.Web
{
	/// <summary>
	/// Summary description for WebService
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class WebService : System.Web.Services.WebService
	{

		[WebMethod]
		public string[] CompileAndRun(string code, Language language)
		{
			var evaller = new Eval(Server.MapPath("~/App_Data/"));
			evaller.CompileAndEval(code, language);
			if (evaller.Errors.Length > 0)
			{
				return evaller.Errors.Select(x => x.ErrorText).ToArray();
			}
			else
			{
				return evaller.Output;
			}
		}
	}
}
