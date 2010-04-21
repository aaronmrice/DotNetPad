using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SubSonic.SqlGeneration.Schema;

namespace Gobiner.CSharpPad.Web.Models
{
	public class ILDisassembly
	{
		[SubSonicPrimaryKey]
		public virtual Guid ID { get; set; }
		public virtual Guid PasteID { get; set; }
		public virtual int Order { get; set; }
		public virtual string Text { get; set; }
	}
}