using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace Gobiner.CSharpPad.Compilers
{
	public class CSharpCompiler : MarshalByRefObject, ICompiler
	{
		public string Code { get; set; }
		public CompilerError[] Errors { get; private set; }
		public string FileName { get; private set; }
		public bool ProducedExecutable { get; private set; }

		private IDictionary<string, string> providerOptions = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
		private string[] GacAssembliesToCompileAgainst = 
		{ "System.dll", "System.Core.dll", "System.Data.dll",
		"System.Data.DataSetExtensions.dll", "Microsoft.CSharp.dll",
		"System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll",
		"System.Windows.Forms.dll", "System.Numerics.dll" };
														   //@"C:\Users\Aaron\Desktop\.net-pad\Gobiner.DotNetPad.Web\bin\AsyncCtpLibrary.dll", };

		public CSharpCompiler()
		{
			Errors = new CompilerError[] { };
		}

		public void Compile(string filename)
		{
			string mainClass = FindMainClass();
			if (mainClass == null && Errors.Length == 0)
			{
				Errors = new CompilerError[] { new CompilerError(filename, 0, 0, "", "Could not find a static void Main(string[]) method") };
				ProducedExecutable = false;
				return;
			}

			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
			compileParams.MainClass = mainClass;
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });
			Errors = r.Errors.Cast<CompilerError>().ToArray();
			ProducedExecutable = Errors.Length == 0;
		}

		private string FindMainClass()
		{
			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
			compileParams.GenerateExecutable = false;
			compileParams.GenerateInMemory = true;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });

			Errors = r.Errors.Cast<CompilerError>().ToArray();

			if (Errors.Count() > 0)
				return null;

			var allTypes = r.CompiledAssembly.GetTypes();
			foreach (var type in allTypes)
			{
				var possibleMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var mainMethod in possibleMethods)
				{
					var parameters = mainMethod.GetParameters();
					if (parameters.Length == 0 ||
					   (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[])))
					{
						return type.FullName;
					}
				}
			}
			return null;
		}
	}
}
