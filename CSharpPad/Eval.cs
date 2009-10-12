using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Gobiner.ConsoleCapture;
using System.Diagnostics;

namespace Gobiner.CSharpPad
{
	public class Eval
	{
		private static Random rand = new Random();

		public System.CodeDom.Compiler.CompilerError[] Errors { get; private set; }
		public string[] Output { get; private set; }

		public void CompileAndEval(string code)
		{
			var compilerDomain = AppDomain.CreateDomain("Gobiner.CSharpPad" + new Random().Next());
			var filename = "Gobiner.CSharpPad.Eval.Evil"+rand.Next()+".exe";
			var fullpath = Environment.CurrentDirectory + "\\" + filename;

			var compiler = (Compiler)compilerDomain.CreateInstanceAndUnwrap(
				Assembly.GetEntryAssembly().FullName,
				"Gobiner.CSharpPad.Compiler",
				false, BindingFlags.Default, null, null, null, null, null);
			compiler.Code = code;
			compiler.Compile(filename);
			Errors = compiler.Errors;
			AppDomain.Unload(compilerDomain);

			if (Errors.Count() == 0)
			{
				var safePerms = new PermissionSet(PermissionState.None);
				safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
				safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				safePerms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
				safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, fullpath));
				safePerms.AddPermission(new UIPermission(PermissionState.Unrestricted));

				var consoleCaptureLibrary = Assembly.GetExecutingAssembly();

				var safeDomain = AppDomain.CreateDomain(
					"Gobiner.CSharpPad.UnsafeProgram+" + filename,
					AppDomain.CurrentDomain.Evidence,
					new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory },
					safePerms,
					GetStrongName(consoleCaptureLibrary));

				var consoleCapture = (ConsoleCapturer)safeDomain.CreateInstanceAndUnwrap(
					consoleCaptureLibrary.FullName,
					"Gobiner.ConsoleCapture.ConsoleCapturer",
					false, BindingFlags.Default, null, null, null, null, null);

				consoleCapture.StartCapture();
				safeDomain.ExecuteAssembly(filename);
				consoleCapture.StopCapture();
				Output = consoleCapture.GetCapturedLines();
				AppDomain.Unload(safeDomain);
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
