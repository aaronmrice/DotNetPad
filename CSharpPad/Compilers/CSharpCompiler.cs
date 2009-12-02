﻿using System;
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

namespace Gobiner.CSharpPad.Compilers
{
	public class CSharpCompiler : MarshalByRefObject, ICompiler
	{
		public string Code { get; set; }
		public CompilerError[] Errors { get; private set; }
		public string FileName { get; private set; }
		public bool ProducedExecutable { get; private set; }
		public IILFormatter ILFormatter { get; set; }
		public string[] FormattedILDisassembly { get; set; }
		
		private IDictionary<Type, IDictionary<System.Reflection.MethodInfo, IEnumerable<Mono.Reflection.Instruction>>> ILLookup { get; set; }
        private IDictionary<string, string> providerOptions = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
		private string[] GacAssembliesToCompileAgainst = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Data.DataSetExtensions.dll", "Microsoft.CSharp.dll",
															 "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll", "System.Windows.Forms.dll" };

		public CSharpCompiler()
        {
            Errors = new CompilerError[] { };
			ILFormatter = new DefaultILFormatter();
        }

		public void Compile(string filename)
		{
			string mainClass = FindMainClass();
			if (mainClass == null && Errors.Length == 0)
			{
				Errors = new CompilerError[] { new CompilerError(filename, 0, 0, "", "Could not find a static void Main(string[]) method") };
				return;
			}

			var provider = new CSharpCodeProvider(providerOptions);
			var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
			compileParams.MainClass = mainClass;
			compileParams.GenerateExecutable = true;
			compileParams.GenerateInMemory = false;
			compileParams.OutputAssembly = filename;
            
			CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });
            if ( r.Errors.HasErrors )
            {
                ILLookup = new Dictionary<Type, IDictionary<MethodInfo, IEnumerable<Mono.Reflection.Instruction>>>();
                FormattedILDisassembly = new string[0];
            }
            else
            {
                ILLookup = new ILDisassembler().GetDisassembly( r.CompiledAssembly );
                if ( ILFormatter != null )
                {
                    FormattedILDisassembly = ILFormatter.Format( ILLookup );
                }
            }

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
			string mainClass = null;
			foreach (var type in allTypes)
			{
				var possibleMethods = type.GetMethods().Where(x => x.Name == "Main" && x.IsStatic);
				foreach (var mainMethod in possibleMethods)
				{
					var parameters = mainMethod.GetParameters();
                    if ( parameters.Length == 0 ||
                        (parameters.Length == 1 && parameters[0].ParameterType == typeof( string[] )) )
                    {
                        mainClass = type.FullName;
                        break;
                    }
				}
			}
			return mainClass;
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
