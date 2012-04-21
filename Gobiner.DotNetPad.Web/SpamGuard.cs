using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gobiner.CSharpPad;

namespace Gobiner.DotNetPad.Web
{
    public static class SpamGuard
    {
        static string[] SpammyWords = { "http" };
        //static string[] SafeWords = { "console", "write", "line" };

        static string[] CSharpKeywords = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "in (generic modifier)", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "out (generic modifier)", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while" };
        static string[] FSharpKeywords = { "abstract", "and", "as", "assert", "base", "begin", "class", "default", "delegate", "do", "done", "downcast", "downto", "elif", "else", "end", "exception", "extern", "false", "finally", "for", "fun", "function", "global", "if", "in", "inherit", "inline", "interface", "internal", "lazy", "let", "match", "member", "module", "mutable", "namespace", "new", "not", "null", "of", "open", "or", "override", "private", "public", "rec", "return", "static", "struct", "then", "to", "true", "try", "type", "upcast", "use", "val", "void", "when", "while", "with", "yield", "asr", "land", "lor", "lsl", "lsr", "lxor", "mod", "sig", "atomic", "break", "checked", "component", "const", "constraint", "constructor", "continue", "eager", "event", "external", "fixed", "functor", "include", "method", "mixin", "object", "parallel", "process", "protected", "pure", "sealed", "tailcall", "trait", "virtual", "volatile" };
        static string[] VisualBasicKeywords = { "AddHandler", "AddressOf", "Alias", "And", "AndAlso", "As", "Boolean", "ByRef", "Byte", "ByVal", "Call", "Case", "Catch", "CBool", "CByte", "CChar", "CDate", "CDec", "CDbl", "Char", "CInt", "Class", "CLng", "CObj", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr", "CType", "CUInt", "CULng", "CUShort", "Date", "Decimal", "Declare", "Default", "Delegate", "Dim", "DirectCast", "Do", "Double", "Each", "Else", "ElseIf", "End", "EndIf", "Enum", "Erase", "Error", "Event", "Exit", "False", "Finally", "For", "Friend", "Function", "Get", "GetType", "Global", "GoSub", "GoTo", "Handles", "If", "Implements", "Imports", "In", "Inherits", "Integer", "Interface", "Is", "IsNot", "Let", "Lib", "Like", "Long", "Loop", "Me", "Mod", "Module", "MustInherit", "MustOverride", "MyBase", "MyClass", "Namespace", "Narrowing", "New", "Next", "Not", "Nothing", "NotInheritable", "NotOverridable", "Object", "Of", "On", "Operator", "Option", "Optional", "Or", "OrElse", "Overloads", "Overridable", "Overrides", "ParamArray", "Partial", "Private", "Property", "Protected", "Public", "RaiseEvent", "ReadOnly", "ReDim", "REM", "RemoveHandler", "Resume", "Return", "SByte", "Select", "Set", "Shadows", "Shared", "Short", "Single", "Static", "Step", "Stop", "String", "Structure", "Sub", "SyncLock", "Then", "Throw", "To", "True", "Try", "TryCast", "TypeOf", "Variant", "Wend", "UInteger", "ULong", "UShort", "Using", "When", "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor" };

        static Dictionary<Language, string[]> LanguageKeywords = new Dictionary<Language, string[]>()
        {
            { Language.CSharp, CSharpKeywords },
            { Language.FSharp, FSharpKeywords },
            { Language.VisualBasic, VisualBasicKeywords },
        };

        public static decimal ContentScore(Language language, string content)
        {
            decimal keyword_count = Regex.Replace(content, "[^a-z]", " ", RegexOptions.IgnoreCase).Split(' ')
                .Select(w => w.ToLower())
                .Count(w => LanguageKeywords[language].Contains(w));

            decimal http_count = Regex.Matches(content, string.Join("|", SpammyWords), RegexOptions.IgnoreCase).Count + 1;

            return keyword_count / http_count - 1;
        }
    }
}
