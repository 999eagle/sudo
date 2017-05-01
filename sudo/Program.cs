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
			var id = Process.GetCurrentProcess().Id.ToString();
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sudoRun.exe");
			var subProcInfo = new ProcessStartInfo(path, id + " " + String.Join(" ", args.Select(a => a.Contains(" ") ? $"\"{a}\"" : a)))
			{
				Verb = "runas"
			};
			var subProc = Process.Start(subProcInfo);
			subProc.WaitForExit();
		}
	}
}
