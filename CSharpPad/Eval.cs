using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using Microsoft.CSharp;
using System.Runtime.InteropServices;

namespace Gobiner.CSharpPad
{
	public class Eval : MarshalByRefObject
	{
		private string code;
		private Assembly assembly;
		private CompilerResults compilationResults;

		public Eval() { }

		public string Code { get { return code; } set { code = value; } }

		public CompilerError[] Compile()
		{
			var providerOptions = new Dictionary<string, string>();
			providerOptions.Add("CompilerVersion", "v3.5");
			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(new string[] { });
			compileParams.GenerateExecutable = false;
			compileParams.GenerateInMemory = true;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.code });
			
			this.compilationResults = r;
			this.assembly = r.CompiledAssembly;
			return r.Errors.Cast<CompilerError>().ToArray();
		}

		public string[] Run()
		{
			if (compilationResults.Errors.Count > 0)
				throw new InvalidOperationException("Cannot Run() a program after compilation failed.");

			var oldConsoleOut = Console.Out;
			var consoleCapture = new StringBuilder();
			Console.SetOut(new StringWriter(consoleCapture));

			var allTypes = assembly.GetTypes();
			MethodInfo main = null;
			foreach (var type in allTypes)
			{
				var possibleMethods = type.GetMethods().Where(x => x.Name == "Main" && x.IsStatic && x.IsPublic);
				foreach(var mainMethod in possibleMethods)
				{
					var parameters = mainMethod.GetParameters();
					if (parameters.Count() == 1 && parameters[0].ParameterType.FullName == "System.String[]")
					{
						main = mainMethod;
						break;
					}
				}
			}
			if(main == null)
				return new string[]{ "Could not find method: public static Main(string[])"};
			
			main.Invoke(null,new object[] { new string[] { } });

			Console.SetOut(oldConsoleOut);

			return consoleCapture.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}
