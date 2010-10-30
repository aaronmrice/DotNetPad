using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Diagnostics;

namespace Gobiner.DotNetPad.Runner
{
	class Program
	{
		static void Main(string[] args)
		{
			var fullpath = args[0];

			var safePerms = new PermissionSet(PermissionState.None);
			safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
			safePerms.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
			safePerms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
			safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, fullpath));
			safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Assembly.GetAssembly(typeof(global::Gobiner.DotNetPad.ConsoleCapturer)).Location));
			safePerms.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, Assembly.GetExecutingAssembly().Location));
			safePerms.AddPermission(new UIPermission(PermissionState.Unrestricted)); // required to run an .exe

			
			var safeDomain = AppDomain.CreateDomain(
				"Gobiner.CSharpPad.UnsafeProgram+" + fullpath,
				AppDomain.CurrentDomain.Evidence,
				new AppDomainSetup() { DisallowCodeDownload = true, ApplicationBase = AppDomain.CurrentDomain.BaseDirectory }, 
				safePerms,
				GetStrongName(typeof(global::Gobiner.DotNetPad.ConsoleCapturer).Assembly),
				GetStrongName(Assembly.GetExecutingAssembly()));

			ConsoleCapturer consoleCapture = null;

			consoleCapture = (ConsoleCapturer)safeDomain.CreateInstanceFromAndUnwrap(
				typeof(global::Gobiner.DotNetPad.ConsoleCapturer).Assembly.Location,
				typeof(global::Gobiner.DotNetPad.ConsoleCapturer).FullName);
			
			Exception uncaughtException = null;

			consoleCapture.StartCapture();
			var threadForGuest = new Thread(new ThreadStart(
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
			var finished = threadForGuest.Join(5000);
			if (!finished)
			{
				threadForGuest.Abort();
			}
			Console.OutputEncoding = Encoding.UTF8;
			Console.WriteLine(consoleCapture.GetCapturedLines().Take(1000).Aggregate((x,y) => x + Environment.NewLine + y));
			if (uncaughtException != null)
			{
				Console.WriteLine(uncaughtException.ToString());
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
