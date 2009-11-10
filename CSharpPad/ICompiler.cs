using System;
namespace Gobiner.CSharpPad
{
    interface ICompiler
    {
        string Code { get; set; }
        void Compile(string filename);
        System.CodeDom.Compiler.CompilerError[] Errors { get; }
        string FileName { get; }
        bool ProducedExecutable { get; }
    }
}
