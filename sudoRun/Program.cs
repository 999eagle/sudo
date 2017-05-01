using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sudoRun
{
	class Program
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AttachConsole(uint dwProcessId);

		static void Main(string[] args)
		{
			var consoleProcess = UInt32.Parse(args[0]);
			AttachConsole(consoleProcess);
			var info = new ProcessStartInfo(args[1], String.Join(" ", args.Skip(2).Select(a => a.Contains(" ") ? $"\"{a}\"" : a)))
			{
				Verb = "runas",
				UseShellExecute = false
			};
			var proc = Process.Start(info);
			proc.WaitForExit();
		}
	}
}
