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

		private string assemblyPath = "";
		private IDictionary<Type, TypeMethodInfo> ILLookup { get; set; }
		private string[] GacAssembliesToCompileAgainst = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Data.DataSetExtensions.dll", 
															 "FSharp.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll", 
															 "System.Windows.Forms.dll", "System.Numerics.dll" };
		public FSharpCompiler()
		{
		}

		public void Compile(string filename)
		{

			var provider = new FSharpCodeProvider();
			var compileParams = new CompilerParameters(new string[] { @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\mscorlib.dll" });
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, this.Code);
			Errors = r.Errors.Cast<CompilerError>().ToArray();
			ProducedExecutable = Errors.Length == 0;
		}
	}
}
