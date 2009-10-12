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
using System.Security;
using System.Security.Permissions;
using System.IO;
using Gobiner.CSharpPad;
using System.Security.Policy;

namespace Gobiner.CSharpPad
{
	class EvalProgram
	{
		static void Main(string[] args)
		{
			var shitToCompile = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Dynasdasdasamic{
class asfasdasd{
public static void Main(string[] args)
{
var i = 2;
var j = 1;
Console.WriteLine(i+j);
}}}";
			

			var evaller = new Eval(Environment.CurrentDirectory);
			evaller.CompileAndEval(shitToCompile);
			if (evaller.Errors.Count() == 0)
			{
				foreach (var line in evaller.Output)
				{
					Console.WriteLine(line);
				}
			}
			else
			{
				foreach (var line in evaller.Errors.Select(x => "Error " + x.ErrorNumber + " on line " + x.Line + " : " + x.ErrorText))
				{
					Console.WriteLine(line);
				}
			}
		}
	}
}

