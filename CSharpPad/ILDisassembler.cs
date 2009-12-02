using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Reflection;
using System.Reflection;

namespace Gobiner.CSharpPad
{
	public class ILDisassembler
	{
		public IDictionary<Type, IDictionary<MethodInfo, IEnumerable<Instruction>>> GetDisassembly(Assembly assembly)
		{
			var ret = new Dictionary<Type, IDictionary<MethodInfo, IEnumerable<Instruction>>>();
			{
				foreach (var type in assembly.GetTypes())
				{
					ret[type] = new Dictionary<MethodInfo, IEnumerable<Instruction>>();
					foreach (var method in type.GetMethods().Concat(type.GetProperties().Select(x=> x.GetGetMethod())).Concat(type.GetProperties().Select(x=> x.GetGetMethod())).Where(x => x.DeclaringType == type))
					{
						try
						{
							ret[type][method] = method.GetInstructions();
						}
						catch (ArgumentException)
						{ }
					}
				}
			}
			return ret;
		}
	}
}
