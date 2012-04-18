using System;
using System.Collections.Generic;

namespace Gobiner.CSharpPad
{
	public interface IILFormatter
	{
		string[] Format(IDictionary<Type, TypeMethodInfo> ILLookup);
	}
}
