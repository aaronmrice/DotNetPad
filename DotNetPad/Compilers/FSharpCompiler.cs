using System;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.FSharp.Compiler.CodeDom;

namespace Gobiner.CSharpPad.Compilers
{
	class FSharpCompiler : MarshalByRefObject, ICompiler
	{
		public string Code { get; set; }
		public CompilerError[] Errors { get; private set; }
		public string FileName { get; private set; }
		public bool ProducedExecutable { get; private set; }

		private string[] GacAssembliesToCompileAgainst = {
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\mscorlib.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Core.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.DataSetExtensions.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\FSharp\2.0\Runtime\v4.0\FSharp.Core.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Xml.Linq.dll", 
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.Entity.dll", 
			@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Windows.Forms.dll",
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Numerics.dll" };
		public FSharpCompiler()
		{
		}

		public void Compile(string filename)
		{

			var provider = new FSharpCodeProvider();
			var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, this.Code);
			Errors = r.Errors.Cast<CompilerError>().ToArray();
			ProducedExecutable = Errors.Length == 0;
		}
	}
}
