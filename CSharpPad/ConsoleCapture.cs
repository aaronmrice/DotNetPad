using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gobiner.CSharpPad
{
	public class ConsoleCapture : MarshalByRefObject
	{
		private TextWriter OldConsole;
		private StringBuilder CapturedText = new StringBuilder();

		public ConsoleCapture()
		{
			OldConsole = Console.Out;
		}

		public void StartCapture()
		{
			Console.SetOut(new StringWriter(CapturedText));
		}

		public void StopCapture()
		{
			Console.SetOut(OldConsole);
		}

		public string[] GetCapturedLines()
		{
			return CapturedText.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}
