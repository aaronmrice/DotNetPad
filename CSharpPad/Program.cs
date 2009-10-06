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

namespace Gobiner.CSharpPad
{
	class EvalProgram
	{
		static void Main(string[] args)
		{
			var shitToCompile = @"
using System;
using System.Text;
using System.Collections.Generic;

namespace Dynasdasdasamic{
class asfasdasd{
public static void Main(string[] args)
{
var i = 2;
var j = 1;
Console.WriteLine(i+j);
}}}";
			

			var compilerDomain = AppDomain.CreateDomain("Gobiner.CSharpPad" + new Random().Next());
			var filename = "test.exe";

			var compiler = (Compiler)compilerDomain.CreateInstanceAndUnwrap(Assembly.GetEntryAssembly().FullName, "Gobiner.CSharpPad.Compiler", false, BindingFlags.Default, null, null, null, null, null);
			compiler.Code = shitToCompile;
			compiler.Compile(filename);
			var errors = compiler.Errors;
			AppDomain.Unload(compilerDomain);

			if (errors.Count() == 0)
			{
				var perms = new PermissionSet(null);
				perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
				perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				perms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
				perms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, @"C:\Users\Aaron\Documents\Visual Studio 2008\Projects\Eval\Eval\bin\Debug\test.exe"));
				perms.AddPermission(new UIPermission(PermissionState.Unrestricted));

				var securePolicy = System.Security.Policy.PolicyLevel.CreateAppDomainLevel();
				securePolicy.NamedPermissionSets.Add(new NamedPermissionSet("safe",perms));
				var unsecurePolicy = System.Security.Policy.PolicyLevel.CreateAppDomainLevel();
				

				var safeDomain = AppDomain.CreateDomain(
					"Gobiner.CSharpPad.UnsafeProgram+"+filename,
					AppDomain.CurrentDomain.Evidence,
					new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory },
					perms,
					null
					);
				var consoleCapture = (ConsoleCapture)safeDomain.CreateInstanceAndUnwrap(Assembly.GetEntryAssembly().FullName, "Gobiner.ConsoleCapture.ConsoleCapture", false, BindingFlags.Default, null, null, null, null, null);
				consoleCapture.StartCapture();
				safeDomain.SetAppDomainPolicy(securePolicy);
				safeDomain.ExecuteAssembly(filename);
				safeDomain.SetAppDomainPolicy(unsecurePolicy);
				consoleCapture.StopCapture();
				AppDomain.Unload(safeDomain);

				var outputLines = consoleCapture.GetCapturedLines();

				foreach (var line in outputLines)
				{
					Console.WriteLine(line);
				}
			}
			else
			{
				foreach (var error in errors)
				{
					Console.WriteLine(error.ErrorText);
				}
			}

			
		}
	}
}

