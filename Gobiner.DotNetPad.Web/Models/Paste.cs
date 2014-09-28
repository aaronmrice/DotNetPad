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
		public virtual Guid Paster
		{
			get
			{
				return _paster;
			}
			set
			{
				_paster = value;
			}
		}
		Guid _paster;
		public virtual Language Language { get; set; }
		public virtual bool IsPrivate { get; set; }
		public virtual ILDisassembly[] ILDisassemblyText { get; set; }
        public virtual decimal ContentScore { get; set; }

        #region CorrectFizzBuzzOutput
        public static string CorrectFizzBuzzOutput = @"1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz
16
17
Fizz
19
Buzz
Fizz
22
23
Fizz
Buzz
26
Fizz
28
29
FizzBuzz
31
32
Fizz
34
Buzz
Fizz
37
38
Fizz
Buzz
41
Fizz
43
44
FizzBuzz
46
47
Fizz
49
Buzz
Fizz
52
53
Fizz
Buzz
56
Fizz
58
59
FizzBuzz
61
62
Fizz
64
Buzz
Fizz
67
68
Fizz
Buzz
71
Fizz
73
74
FizzBuzz
76
77
Fizz
79
Buzz
Fizz
82
83
Fizz
Buzz
86
Fizz
88
89
FizzBuzz
91
92
Fizz
94
Buzz
Fizz
97
98
Fizz
Buzz";
		#endregion

		public Paste()
		{
			ID = Guid.NewGuid();
			Slug = Convert.ToBase64String(ID.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);
			Code = string.Empty;
			Password = string.Empty;
			Output = string.Empty;
			Created = DateTime.Now;
			Paster = Guid.Empty;
			Language = Language.CSharp;
			IsPrivate = false;
            ContentScore = 0;
		}

		public void AddCompilerErrors(CompilerError[] errors)
		{
			this.Errors = errors.Select(x => new CompilationError(x) { PasteID = this.ID }).ToArray();
		}

		public void AddILDisassemblyText(string[] text)
		{
			var order = 0;
			this.ILDisassemblyText = text.Select(x => new ILDisassembly() { ID = Guid.NewGuid(), Order = order++, Text = x, PasteID = this.ID }).ToArray();
		}

		public void Compile(string pasterGuid, bool isPrivate, string path)
		{
			var evaller = new Eval(path);
			evaller.CompileAndEval(this.Code, this.Language);

			this.AddCompilerErrors(evaller.Errors);
			this.AddILDisassemblyText(evaller.FormattedILDisassembly);
			this.Output = string.Join(Environment.NewLine, evaller.Output ?? new string[] { });

			this.Paster = !Guid.TryParse(pasterGuid, out this._paster) ? Guid.NewGuid() : this.Paster;
			this.IsPrivate = isPrivate;
		}
	}
}
