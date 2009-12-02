using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.FSharp.Compiler.CodeDom;

namespace Gobiner.CSharpPad.Compilers
{
	class FSharpCompiler : MarshalByRefObject, ICompiler
	{
		public string Code { get; set; }
		public CompilerError[] Errors { get; private set; }
		public string FileName { get; private set; }
		public bool ProducedExecutable { get; private set; }
		public IILFormatter ILFormatter { get; set; }
		public string[] FormattedILDisassembly { get; set; }

		private IDictionary<Type, IDictionary<System.Reflection.MethodInfo, IEnumerable<Mono.Reflection.Instruction>>> ILLookup { get; set; }
		private string[] GacAssembliesToCompileAgainst = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Data.DataSetExtensions.dll", 
															 "FSharp.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll", 
															 "System.Windows.Forms.dll", "System.Numerics.dll" };
		public FSharpCompiler()
		{
			ILFormatter = new DefaultILFormatter();
		}

		public void Compile(string filename)
		{
			var provider = new FSharpCodeProvider();
			var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });
			Errors = r.Errors.Cast<CompilerError>().ToArray();
			if (Errors.Length > 0)
			{
				ProducedExecutable = false;
			}
			ILLookup = new ILDisassembler().GetDisassembly(r.CompiledAssembly);
			if (ILFormatter != null)
			{
				FormattedILDisassembly = ILFormatter.Format(ILLookup);
			}
		}

		private class DefaultILFormatter : MarshalByRefObject, IILFormatter
		{
			public string[] Format(IDictionary<Type, IDictionary<System.Reflection.MethodInfo, IEnumerable<Mono.Reflection.Instruction>>> ILLookup)
			{
				if (ILLookup == null) return new string[0];

				var ret = new List<string>();
				foreach (var type in ILLookup.Keys)
				{
					ret.Add("Type: " + type.Name);
					ret.Add("{");
					foreach (var method in ILLookup[type].Keys)
					{
						ret.Add("\tMethod: " + method.Name);
						ret.Add("\t{");
						foreach (var instr in ILLookup[type][method])
						{
							ret.Add("\t\t" + instr.ToString());
						}
						ret.Add("\t}");
					}
					ret.Add("}");
				}
				return ret.ToArray();
			}
		}
	}
}
