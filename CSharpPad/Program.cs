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
using Gobiner.ConsoleCapture;
using System.Security.Policy;

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
				var perms = new PermissionSet(PermissionState.None);
				perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
				perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				perms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
				perms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, @"C:\Users\Aaron\Documents\Visual Studio 2008\Projects\CSharpPad\CSharpPad\bin\Debug\" + filename));
				perms.AddPermission(new UIPermission(PermissionState.Unrestricted));


				var securePolicy = System.Security.Policy.PolicyLevel.CreateAppDomainLevel();
				securePolicy.Reset();
				securePolicy.NamedPermissionSets.Add(new NamedPermissionSet("safe",perms));

				var safeDomain = AppDomain.CreateDomain(
					"Gobiner.CSharpPad.UnsafeProgram+"+filename,
					AppDomain.CurrentDomain.Evidence,
					new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory },
					perms,
					new StrongName[] { 
						new StrongName(
							new StrongNamePublicKeyBlob(File.ReadAllBytes(@"C:\Users\Aaron\Documents\Visual Studio 2008\Projects\CSharpPad\Gobiner.ConsoleCapture\output.txt")), 
							"Gobiner.ConsoleCapture, Version=1.0.0.0, Culture=neutral, PublicKeyToken=765b1619f6c014ac", 
							new Version("1.0.0.0")
							)}
					);
				
				var consoleCapture = (ConsoleCapturer)safeDomain.CreateInstanceAndUnwrap(
					"Gobiner.ConsoleCapture, Version=1.0.0.0, Culture=neutral, PublicKeyToken=765b1619f6c014ac", 
					"Gobiner.ConsoleCapture.ConsoleCapturer", false, BindingFlags.Default, null, null, null, null, null);
				consoleCapture.StartCapture();
				safeDomain.ExecuteAssembly(filename);
				consoleCapture.StopCapture();
				var outputLines = consoleCapture.GetCapturedLines();
				AppDomain.Unload(safeDomain);

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

