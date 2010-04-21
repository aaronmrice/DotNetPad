using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Reflection;

namespace Gobiner.CSharpPad
{
	internal class DefaultILFormatter : MarshalByRefObject, IILFormatter
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
					if (getMethod != null && methodsLocal.ContainsKey(getMethod))
					{
						getMethods[property] = methodsLocal[getMethod];
						methodsLocal.Remove(getMethod);
					}
					if (setMethod != null && methodsLocal.ContainsKey(setMethod))
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
