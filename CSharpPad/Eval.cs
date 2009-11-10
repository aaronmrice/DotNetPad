using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Gobiner.CSharpPad;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Gobiner.CSharpPad
{
	public class Eval : MarshalByRefObject
	{
		private static Random rand = new Random();
		private Exception uncaughtException = null;
		private Thread threadForGuest = null;

		public System.CodeDom.Compiler.CompilerError[] Errors { get; private set; }
		public string[] Output { get; private set; }
		private string fullPath { get; set; }

		public Eval(string folder)
		{
			fullPath = folder;
		}

		public void CompileAndEval(string[] code)
		{
			CompileAndEval(code.Aggregate((x, y) => x + Environment.NewLine + y));
		}

		public void CompileAndEval(string code)
		{
			var compilerDomain = AppDomain.CreateDomain("Gobiner.CSharpPad" + new Random().Next());
			var filename = "Gobiner.CSharpPad.Eval.Evil"+rand.Next()+".exe";
			var fullpath = fullPath + filename;

			var currentAssembly = Assembly.GetAssembly(typeof(global::Gobiner.CSharpPad.CSharpCompiler)).Location;
			
			var compiler = (CSharpCompiler)compilerDomain.CreateInstanceFromAndUnwrap(
				Assembly.GetAssembly(typeof(global::Gobiner.CSharpPad.CSharpCompiler)).Location,
				typeof(global::Gobiner.CSharpPad.CSharpCompiler).FullName);
			compiler.Code = code;
			compiler.Compile(fullpath);
			Errors = compiler.Errors;
			AppDomain.Unload(compilerDomain);

			if (Errors != null && Errors.Length == 0)
			{
				var safePerms = new PermissionSet(PermissionState.None);
				safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
				safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				safePerms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
				safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, fullpath));
				safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Assembly.GetAssembly(typeof(global::Gobiner.CSharpPad.ConsoleCapturer)).Location));
				safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Assembly.GetExecutingAssembly().Location));
				safePerms.AddPermission(new UIPermission(PermissionState.Unrestricted)); // required to run an .exe

				var consoleCaptureLibrary = Assembly.GetExecutingAssembly();

				var safeDomain = AppDomain.CreateDomain(
					"Gobiner.CSharpPad.UnsafeProgram+" + filename,
					AppDomain.CurrentDomain.Evidence,
					new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory },
					safePerms,
					GetStrongName(consoleCaptureLibrary),
					GetStrongName(Assembly.GetExecutingAssembly()));


				var consoleCapture = (ConsoleCapturer)safeDomain.CreateInstanceFromAndUnwrap(
					Assembly.GetAssembly(typeof(global::Gobiner.CSharpPad.ConsoleCapturer)).Location,
					typeof(global::Gobiner.CSharpPad.ConsoleCapturer).FullName);

				consoleCapture.StartCapture();
				threadForGuest = new Thread(new ThreadStart(
					delegate() 
					{
						try
						{
							safeDomain.ExecuteAssembly(fullpath);
						}
						catch (Exception e)
						{
							uncaughtException = e;
						}
					}), 1024 * 1024 * 8);
				threadForGuest.Start();
				var finished = threadForGuest.Join(3000);
				if (!finished)
				{
					threadForGuest.Abort();
				}
				
				consoleCapture.StopCapture();
				Output = consoleCapture.GetCapturedLines().Take(1000).ToArray();
				if (uncaughtException != null)
				{
					Output = Output.Concat(uncaughtException.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None)).ToArray();
				}

				AppDomain.Unload(safeDomain);
				
			}
			File.Delete(fullpath);
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
