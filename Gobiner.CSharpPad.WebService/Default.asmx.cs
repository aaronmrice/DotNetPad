using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;

namespace Gobiner.CSharpPad.WebService
{
	/// <summary>
	/// Summary description for Default
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class Default : System.Web.Services.WebService
	{

		[WebMethod]
		public string[] CompileAndRun(string code)
		{
			var evaller = new Eval(Server.MapPath("~/App_Data/"));
			evaller.CompileAndEval(code);
			if (evaller.Errors.Count() == 0)
			{
				return evaller.Output;
			}
			else
			{
				return evaller.Errors.Select(x => "Error " + x.ErrorNumber + " on line " + x.Line + " : " + x.ErrorText).ToArray();
			}
		}
	}
}
