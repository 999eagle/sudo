using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace sudo
{
	class Program
	{
		static bool IsElevated { get => WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid); }

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: sudo <command> [arg1 [arg2 [arg3 [...]]]");
				return;
			}
			ProcessStartInfo info;
			if (!IsElevated)
			{
				var id = Process.GetCurrentProcess().Id.ToString();
				var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sudoRun.exe");
				info = new ProcessStartInfo(path, id + " " + String.Join(" ", args.Select(a => a.Contains(" ") ? $"\"{a}\"" : a)))
				{
					Verb = "runas"
				};
			}
			else
			{
				info = new ProcessStartInfo(args[0], String.Join(" ", args.Skip(1).Select(a => a.Contains(" ") ? $"\"{a}\"" : a)))
				{
					UseShellExecute = false
				};
			}
			var subProc = Process.Start(info);
			subProc.WaitForExit();
		}
	}
}
