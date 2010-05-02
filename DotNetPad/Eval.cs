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
using Gobiner.CSharpPad.Compilers;

namespace Gobiner.CSharpPad
{
	public class Eval : MarshalByRefObject
	{
		private static Random rand = new Random();
		private Exception uncaughtException = null;
		private Thread threadForGuest = null;


		public System.CodeDom.Compiler.CompilerError[] Errors { get; private set; }
		public string[] Output { get; private set; }
		public string[] FormattedILDisassembly { get; private set; }
		public IILFormatter ILFormatter { get; set; }

		private string fullPath { get; set; }

		public Eval(string folder)
		{
			fullPath = folder;
		}

		public void CompileAndEval(string[] code, Language language)
		{
			CompileAndEval(code.Aggregate((x, y) => x + Environment.NewLine + y), language);
		}

		public void CompileAndEval(string code, Language language)
		{
			var compilerDomain = AppDomain.CreateDomain("Gobiner.CSharpPad" + new Random().Next());
			var filename = "Gobiner.CSharpPad.Eval.Evil" + rand.Next() + ".exe";
			var fullpath = Path.Combine(fullPath, filename);

			var currentAssembly = Assembly.GetAssembly(typeof(global::Gobiner.CSharpPad.Compilers.CSharpCompiler)).Location;

			var compiler = (ICompiler)compilerDomain.CreateInstanceFromAndUnwrap(
				GetCompiler(language).GetType().Assembly.Location,
				GetCompiler(language).GetType().FullName);
			compiler.Code = code;
			compiler.Compile(fullpath);
			Errors = compiler.Errors;
			FormattedILDisassembly = compiler.FormattedILDisassembly;
			AppDomain.Unload(compilerDomain);

			if (Errors != null && Errors.Length == 0)
			{
				var startInfo = new ProcessStartInfo();
				startInfo.Arguments = fullpath;
				startInfo.CreateNoWindow = true;
				startInfo.ErrorDialog = false;
				startInfo.FileName = @"C:\Users\Aaron\Desktop\dotnetpad\Gobiner.DotNetPad.Web\bin\Gobiner.DotNetPad.Runner.exe";
				startInfo.RedirectStandardOutput = true;
				startInfo.StandardOutputEncoding = Encoding.Unicode;
				startInfo.WorkingDirectory = @"C:\Users\Aaron\Desktop\dotnetpad\Gobiner.DotNetPad.Web\App_Data";
				startInfo.UseShellExecute = false;

				var process = Process.Start(startInfo);
				var finished = process.WaitForExit(3500);
				Output = process.StandardOutput.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
				process.Kill();
			}
			GC.Collect();
			File.Delete(fullpath);
		}

		private ICompiler GetCompiler(Language language)
		{
			switch (language)
			{
				case Language.CSharp: return new CSharpCompiler();
				case Language.VisualBasic: return new VisualBasicCompiler();
				case Language.FSharp: return new FSharpCompiler();
				default: return null;
			}
		}
	}
}
