using System;
using System.Collections.Generic;
using Mono.Reflection;
namespace Gobiner.CSharpPad.Compilers
{
    interface ICompiler
    {
        string Code { get; set; }
        void Compile(string filename);
        System.CodeDom.Compiler.CompilerError[] Errors { get; }
        string FileName { get; }
        bool ProducedExecutable { get; }
		IILFormatter ILFormatter { get; set; }
		string[] FormattedILDisassembly { get; set; }

    }
}
