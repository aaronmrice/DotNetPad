using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security;

namespace Gobiner.CSharpPad
{
	public class ConsoleCapturer : MarshalByRefObject
	{
		private TextWriter OldConsole;
		private StringBuilder CapturedText = new StringBuilder();

		public ConsoleCapturer()
		{
			new PermissionSet(PermissionState.Unrestricted).Assert();
			OldConsole = Console.Out;
			CodeAccessPermission.RevertAssert();
		}

		public void StartCapture()
		{
			new PermissionSet(PermissionState.Unrestricted).Assert();
			Console.SetOut(new StringWriter(CapturedText));
			CodeAccessPermission.RevertAssert();
		}

		public void StopCapture()
		{
			new PermissionSet(PermissionState.Unrestricted).Assert();
			Console.SetOut(OldConsole);
			CodeAccessPermission.RevertAssert();
		}

		public string[] GetCapturedLines()
		{
			return CapturedText.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}
