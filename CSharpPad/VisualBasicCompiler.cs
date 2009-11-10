using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;

namespace Gobiner.CSharpPad
{
    public class VisualBasicCompiler : ICompiler
    {
        public string Code { get; set; }
        public CompilerError[] Errors { get; private set; }
        public string FileName { get; private set; }
        public bool ProducedExecutable { get; private set; }

        private string[] GacAssembliesToCompileAgainst = { "System.dll", "System.Core.dll", "System.Data.dll", "System.Data.DataSetExtensions.dll",
															 "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.Entity.dll", "System.Windows.Forms.dll" };

        public VisualBasicCompiler()
        {
            Errors = new CompilerError[] { };
        }

        public void Compile(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
