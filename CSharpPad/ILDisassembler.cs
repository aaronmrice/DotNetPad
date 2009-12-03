using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Reflection;
using System.Reflection;

namespace Gobiner.CSharpPad
{
	public struct TypeMethodInfo
	{
		public IDictionary<MethodInfo, IEnumerable<Instruction>> Methods { get; set; }
		public IList<PropertyInfo> Properties { get; set; }
	}

	public class ILDisassembler
	{
		public IDictionary<Type, TypeMethodInfo> GetDisassembly( Assembly assembly )
		{
			var ret = new Dictionary<Type, TypeMethodInfo>();
			{
				foreach (var type in assembly.GetTypes())
				{
					var methods = new Dictionary<MethodInfo, IEnumerable<Instruction>>();
					foreach (var method in type
					                         .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					                            .Concat(type.GetProperties().Select(x=> x.GetGetMethod(true)))
					                            .Concat(type.GetProperties().Select(x=> x.GetSetMethod(true)))
					                         .Where(x => x.DeclaringType == type))
					{
						try
						{
							methods[method] = method.GetInstructions();
						}
						catch (ArgumentException)
						{ }
					}

					var properties = new List<PropertyInfo>();
					foreach ( var property in type.GetProperties().Where( x => x.DeclaringType == type ) )
					{
						properties.Add( property );
					}

					ret[type] = new TypeMethodInfo { Methods = methods, Properties = properties };
				}
			}
			return ret;
		}
	}
}
