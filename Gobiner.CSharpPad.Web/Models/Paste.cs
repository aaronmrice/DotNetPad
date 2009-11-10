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
		public virtual Guid ID { get; set; }
		public virtual string Code { get; set; }
		public virtual string Password { get; set; }
		public virtual string Slug { get; set; }
		public virtual CompilationError[] Errors { get; set; }
		public virtual string Output { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual bool IsPrivate { get; set; }
		public virtual Guid? Paster { get; set; }
		public virtual Language Language { get; set; }

		public Paste()
		{
			ID = Guid.NewGuid();
			Slug = Convert.ToBase64String(ID.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);
			Code = string.Empty;
			Password = string.Empty;
			Output = string.Empty;
			Created = DateTime.Now;
			IsPrivate = false;
			Language = Models.Language.CSharp;
		}

		public void AddCompilerErrors(CompilerError[] errors)
		{
			this.Errors = errors.Select(x => new CompilationError(x) { PasteID = this.ID }).ToArray();
		}
	}
}
