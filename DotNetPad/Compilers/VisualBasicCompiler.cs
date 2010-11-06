using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

namespace Gobiner.CSharpPad.Compilers
{
    public class VisualBasicCompiler : MarshalByRefObject, ICompiler
    {
        public string Code { get; set; }
        public CompilerError[] Errors { get; private set; }
        public string FileName { get; private set; }
        public bool ProducedExecutable { get; private set; }
		
        private string[] GacAssembliesToCompileAgainst = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Data.DataSetExtensions.dll", 
															 "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll", "System.Windows.Forms.dll" };
        private IDictionary<string, string> providerOptions = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };

        public VisualBasicCompiler()
        {
            Errors = new CompilerError[] { };
		}

        public void Compile(string filename)
        {
            string mainClass = FindMainClass();
            if (mainClass == null && Errors.Length == 0)
            {
                Errors = new CompilerError[] { new CompilerError(filename, 0, 0, "", "Could not find a Main method") };
                return;
            }

            var provider = new VBCodeProvider(providerOptions);
            var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
            compileParams.MainClass = mainClass;
            compileParams.GenerateExecutable = true;
            compileParams.GenerateInMemory = false;
            compileParams.OutputAssembly = filename;

            CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });
			Errors = r.Errors.Cast<CompilerError>().ToArray();
			ProducedExecutable = Errors.Length == 0;
        }

        private string FindMainClass()
        {
            var provider = new VBCodeProvider(providerOptions);
            var compileParams = new CompilerParameters(GacAssembliesToCompileAgainst);
            compileParams.GenerateExecutable = false;
            compileParams.GenerateInMemory = true;

            CompilerResults r = provider.CompileAssemblyFromSource(compileParams, new string[] { this.Code });

            Errors = r.Errors.Cast<CompilerError>().ToArray();

            if (Errors.Count() > 0)
                return null;

            var allTypes = r.CompiledAssembly.GetTypes();
            string mainClass = null;
            foreach (var type in allTypes)
            {
                var possibleMethods = type.GetMethods().Where(x => x.Name == "Main" && x.IsStatic);
                foreach (var mainMethod in possibleMethods)
                {
                    var parameters = mainMethod.GetParameters();
                    if (parameters.Count() == 0)
                    {
                        mainClass = type.FullName;
                        break;
                    }
                }
            }
            return mainClass;
        }
    }
}
