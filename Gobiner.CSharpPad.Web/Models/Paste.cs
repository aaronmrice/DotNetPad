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
using System.CodeDom.Compiler;
using SubSonic.SqlGeneration.Schema;

namespace Gobiner.CSharpPad.Web.Models
{
	public class Paste
	{
		[SubSonicPrimaryKey]
		public Guid ID { get; set; }
		public string Code { get; set; }
		public string Password { get; set; }
		public string Slug { get; set; }
		public CompilationError[] Errors { get; set; }
		public string Output { get; set; }

		public Paste()
		{
			ID = Guid.NewGuid();
			Slug = HttpUtility.UrlEncode(ID.ToString());
			Code = string.Empty;
			Password = string.Empty;
			Output = string.Empty;
		}

		public void AddCompilerErrors(CompilerError[] errors)
		{
			this.Errors = errors.Select(x => new CompilationError(x) { PasteID = this.ID }).ToArray();
		}
	}
}
