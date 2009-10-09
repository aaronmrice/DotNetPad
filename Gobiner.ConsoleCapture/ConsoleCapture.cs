using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Gobiner.ConsoleCapture
{
	public class ConsoleCapturer : MarshalByRefObject
	{
		private TextWriter OldConsole;
		private StringBuilder CapturedText = new StringBuilder();

		public ConsoleCapturer()
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
