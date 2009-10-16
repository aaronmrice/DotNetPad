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
	public class Compiler : MarshalByRefObject
	{
		public string Code { get; set; }
		public CompilerError[] Errors { get; private set; }
		public string FileName { get; private set; }
		public bool FoundMainClass { get; private set; }

		public Compiler() { }

		public void Compile(string filename)
		{
			string mainClass;
			try
			{
				mainClass = FindMainClass();
			}
			catch (Exception e)
			{
				return;
			}

			var providerOptions = new Dictionary<string, string>();
			providerOptions.Add("CompilerVersion", "v3.5");
			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(new string[] { "System.dll", "System.Core.dll" });
			compileParams.MainClass = mainClass;
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });

		}

		private string FindMainClass()
		{
			var providerOptions = new Dictionary<string, string>();
			providerOptions.Add("CompilerVersion", "v3.5");
			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(new string[] { "System.dll", "System.Core.dll" });
			compileParams.GenerateExecutable = false;
			compileParams.GenerateInMemory = true;

			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });

			Errors = r.Errors.Cast<CompilerError>().ToArray();

			if (Errors.Count() > 0)
				throw new Exception("Compilation failed");

			var allTypes = r.CompiledAssembly.GetTypes();
			string mainClass = null;
			foreach (var type in allTypes)
			{
				var possibleMethods = type.GetMethods().Where(x => x.Name == "Main" && x.IsStatic && x.IsPublic);
				foreach (var mainMethod in possibleMethods)
				{
					var parameters = mainMethod.GetParameters();
					if (parameters.Count() == 1 && parameters[0].ParameterType.FullName == "System.String[]")
					{
						mainClass = type.FullName;
						break;
					}
				}
			}
			if (mainClass == null)
			{
				throw new Exception("Could not find Main(string[])");
			}
			return mainClass;
		}
	}
}
