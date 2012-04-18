using System;

namespace Gobiner.CSharpPad
{
	public enum Language
	{
		CSharp,
		VisualBasic,
		FSharp,
	}

	public static class LanguageExtension
	{
		public static Language LanguageFromString(string s)
		{
			switch (s.ToLower())
			{
				case "c#": return Language.CSharp;
				case "f#": return Language.FSharp;
				case "vb": return Language.VisualBasic;
				default: throw new ArgumentException("Language not supported");
			}
		}
	}
}