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
			var fullpath = Environment.CurrentDirectory + "\\" + filename;

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
				perms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, fullpath));
				perms.AddPermission(new UIPermission(PermissionState.Unrestricted));

				var consoleCaptureLibrary = Assembly.Load("Gobiner.ConsoleCapture");

				var safeDomain = AppDomain.CreateDomain(
					"Gobiner.CSharpPad.UnsafeProgram+"+filename,
					AppDomain.CurrentDomain.Evidence,
					new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory },
					perms,
					GetStrongName(consoleCaptureLibrary));
				
				var consoleCapture = (ConsoleCapturer)safeDomain.CreateInstanceAndUnwrap(
					consoleCaptureLibrary.FullName, 
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

		/// <summary>
		/// Create a StrongName that matches a specific assembly
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// if <paramref name="assembly"/> is null
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// if <paramref name="assembly"/> does not represent a strongly named assembly
		/// </exception>
		/// <param name="assembly">Assembly to create a StrongName for</param>
		/// <returns>A StrongName that matches the given assembly</returns>
		private static StrongName GetStrongName(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			AssemblyName assemblyName = assembly.GetName();
			Debug.Assert(assemblyName != null, "Could not get assembly name");

			// get the public key blob
			byte[] publicKey = assemblyName.GetPublicKey();
			if (publicKey == null || publicKey.Length == 0)
				throw new InvalidOperationException("Assembly is not strongly named");

			StrongNamePublicKeyBlob keyBlob = new StrongNamePublicKeyBlob(publicKey);

			// and create the StrongName
			return new StrongName(keyBlob, assemblyName.Name, assemblyName.Version);
		}
	}
}

