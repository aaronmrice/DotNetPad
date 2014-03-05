using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            if (compiler.ProducedExecutable)
            {
                FormattedILDisassembly = RunProcessAndCaptureOutput(
                                            Path.Combine(fullPath.Replace("App_Data", ""), "bin", "ildasm.exe"),
                                            "/text /unicode /nobar \"" + fullpath + "\"")
                                         .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            }
            else
            {
                FormattedILDisassembly = new string[] { };
            }
            AppDomain.Unload(compilerDomain);

            if (Errors != null && Errors.Length == 0)
            {
                var stdout = RunProcessAndCaptureOutput(Path.Combine(fullPath.Replace("App_Data", ""), "bin", "Gobiner.DotNetPad.Runner.exe"), fullpath);
                var output = new List<string>();
                output.AddRange(stdout.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

                if (output.Count > 0) output.RemoveAt(output.Count - 1); // always seems to have an extra newline
                Output = output.ToArray();
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

        private string RunProcessAndCaptureOutput(string path, string arguments)
        {
            var startInfo = new ProcessStartInfo()
            {
                Arguments = arguments,
                CreateNoWindow = true,
                ErrorDialog = false,
                FileName = path,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
            };
            var process = new Process()
            {
                StartInfo = startInfo,
            };

            var output = new List<string>();
            bool finished = false;

            try
            {
                process.Start();

                finished = process.WaitForExit(6000);

                return process.StandardOutput.ReadToEnd();
            }
            finally
            {
                finished = process.WaitForExit(500);

                if (!finished)
                {
                    process.Kill();
                }
            }
        }
    }
}
