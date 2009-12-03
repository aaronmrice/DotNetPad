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
using Mono.Reflection;

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

		private IDictionary<Type, TypeMethodInfo> ILLookup { get; set; }
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
			if (r.Errors.HasErrors)
			{
				ILLookup = new Dictionary<Type, TypeMethodInfo>();
				FormattedILDisassembly = new string[0];
			}
			else
			{
				ILLookup = new ILDisassembler().GetDisassembly(r.CompiledAssembly);
				if (ILFormatter != null)
				{
					FormattedILDisassembly = ILFormatter.Format(ILLookup);
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
					if (parameters.Length == 0 ||
					   (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[])))
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
			public string[] Format(IDictionary<Type, TypeMethodInfo> ILLookup)
			{
				if (ILLookup == null)
					return new string[0];

				var ret = new List<string>();
				foreach (var kvp in ILLookup)
				{
					var type = kvp.Key;

					var methodsLocal = new Dictionary<MethodInfo, IEnumerable<Instruction>>(kvp.Value.Methods);
					var getMethods = new Dictionary<PropertyInfo, IEnumerable<Instruction>>();
					var setMethods = new Dictionary<PropertyInfo, IEnumerable<Instruction>>();

					// Figure out which methods correspond to which property
					foreach (var property in kvp.Value.Properties)
					{
						var getMethod = property.GetGetMethod(true);
						var setMethod = property.GetSetMethod(true);
						if (methodsLocal.ContainsKey(getMethod))
						{
							getMethods[property] = methodsLocal[getMethod];
							methodsLocal.Remove(getMethod);
						}
						if (methodsLocal.ContainsKey(setMethod))
						{
							setMethods[property] = methodsLocal[setMethod];
							methodsLocal.Remove(setMethod);
						}
					}

					ret.Add("Type: " + type.Name);
					ret.Add("{");
					ret.AddRange(GetPropertyBodies(kvp.Value.Properties, getMethods, setMethods, 1));
					ret.AddRange(GetMethodBodies(methodsLocal, 1));
					ret.Add("}");
				}
				return ret.ToArray();
			}

			private IList<string> GetPropertyBodies(
			    IList<PropertyInfo> properties,
			    IDictionary<PropertyInfo, IEnumerable<Instruction>> getMethods,
			    IDictionary<PropertyInfo, IEnumerable<Instruction>> setMethods,
			    int indentLevel)
			{
				var result = new List<string>();
				foreach (var prop in properties)
				{
					var getMethod = getMethods.ContainsKey(prop) ? getMethods[prop] : null;
					var setMethod = setMethods.ContainsKey(prop) ? setMethods[prop] : null;

					result.AddRange(GetPropertyBody(prop, getMethod, setMethod, indentLevel));
				}
				return result;
			}

			private IList<string> GetPropertyBody(
			    PropertyInfo info,
			    IEnumerable<Instruction> getMethod,
			    IEnumerable<Instruction> setMethod,
			    int indentLevel)
			{
				string indentString = GetIndentString(indentLevel);
				string biggerIndentString = indentString + "\t";

				var result = new List<string>();
				result.Add(indentString + "Property: " + info.Name);
				result.Add(indentString + "{");
				if (getMethod != null)
				{
					result.Add(biggerIndentString + "Get method: " + info.GetGetMethod(true).Name);
					result.Add(biggerIndentString + "{");
					result.AddRange(GetMethodBody(getMethod, indentLevel + 2));
					result.Add(biggerIndentString + "}");
				}
				if (setMethod != null)
				{
					result.Add(biggerIndentString + "Set method: " + info.GetSetMethod(true).Name);
					result.Add(biggerIndentString + "{");
					result.AddRange(GetMethodBody(setMethod, indentLevel + 2));
					result.Add(biggerIndentString + "}");
				}
				result.Add(indentString + "}");

				return result;
			}

			private static string GetIndentString(int indentLevel)
			{
				string indentString = string.Empty;
				for (int i = 0; i < indentLevel; i++)
					indentString += "\t";
				return indentString;
			}

			private IList<string> GetMethodBodies(IDictionary<MethodInfo, IEnumerable<Instruction>> methods, int indentLevel)
			{
				string indentString = GetIndentString(indentLevel);
				var result = new List<string>();
				foreach (var kvp in methods)
				{
					result.Add(indentString + "Method: " + kvp.Key.Name);
					result.Add(indentString + "{");
					result.AddRange(GetMethodBody(kvp.Value, indentLevel + 1));
					result.Add(indentString + "}");
				}
				return result;
			}

			private IList<string> GetMethodBody(IEnumerable<Instruction> instructions, int indentLevel)
			{
				string indentString = GetIndentString(indentLevel);

				var result = new List<string>();
				foreach (var instr in instructions)
				{
					result.Add(indentString + instr.ToString());
				}
				return result;
			}
		}
	}
}
