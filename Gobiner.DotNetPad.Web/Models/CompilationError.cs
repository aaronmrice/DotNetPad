using System;
using SubSonic.SqlGeneration.Schema;

namespace Gobiner.CSharpPad.Web.Models
{
	public class CompilationError
	{
		[SubSonicPrimaryKey]
		public virtual Guid ID { get; set; }

		public virtual Guid PasteID { get; set; }
		public virtual int Column { get; set; }
		public virtual string ErrorNumber { get; set; }
		public virtual string ErrorText { get; set; }
		public virtual string FileName { get; set; }
		public virtual int Line { get; set; }
		public virtual bool IsWarning { get; set; }

		public CompilationError()
		{
		}

		public CompilationError(System.CodeDom.Compiler.CompilerError error)
		{
			ID = Guid.NewGuid();
			Column = error.Column;
			ErrorNumber = error.ErrorNumber;
			ErrorText = error.ErrorText;
			FileName = error.FileName;
			IsWarning = error.IsWarning;
			Line = error.Line;
		}
	}
}

