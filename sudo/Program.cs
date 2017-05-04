using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace sudo
{
	class Program
	{
		static bool IsElevated { get => WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid); }

		static string EscapeArg(string arg)
		{
			arg = arg.Replace("\"", "\\\"");
			arg = arg.Contains(" ") ? $"\"{arg}\"" : arg;
			return arg;
		}

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: sudo <command> [arg1 [arg2 [arg3 [...]]]");
				return;
			}
			var parent = Interop.GetParentProcess();
			// if we have a parent, close our console and attach to the parent's
			if (parent != null)
			{
				Interop.FreeConsole();
				if (!Interop.AttachConsole(parent.Id) && !Interop.AllocConsole()) return;
			}
			ProcessStartInfo info;
			var commandArgsString = String.Join(" ", args.Skip(1).Select(EscapeArg));
			if (!IsElevated)
			{
				var path = Assembly.GetEntryAssembly().Location;
				info = new ProcessStartInfo(path, $"{EscapeArg(args[0])} {commandArgsString}")
				{
					Verb = "runas"
				};
			}
			else
			{
				info = new ProcessStartInfo(args[0], commandArgsString)
				{
					UseShellExecute = false
				};
			}
			try
			{
				var subProc = Process.Start(info);
				subProc.WaitForExit();
			}
			catch (Win32Exception ex) when (ex.ErrorCode == Interop.HRESULT_E_FAIL && ex.NativeErrorCode == Interop.W32_ERROR_FILE_NOT_FOUND)
			{
				var color = (Console.ForegroundColor, Console.BackgroundColor);
				Console.ForegroundColor = ConsoleColor.Red;
				Console.BackgroundColor = ConsoleColor.Black;
				Console.WriteLine($"The term '{info.FileName}' was not recognized as the name of an executable or file. Please check the spelling and try again.");
				(Console.ForegroundColor, Console.BackgroundColor) = color;
			}
		}
	}

	class Interop
	{
		public const int HRESULT_E_FAIL = unchecked((int)0x80004005);
		public const int W32_ERROR_FILE_NOT_FOUND = unchecked((int)0x00000002);

		[StructLayout(LayoutKind.Sequential)]
		public struct ProcessBasicInformation
		{
			public IntPtr Reserved1;
			public IntPtr PebBaseAddress;
			public IntPtr Reserved2_0;
			public IntPtr Reserved2_1;
			public IntPtr UniqueProcessId;
			public IntPtr InheritedFromUniqueProcessId;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AttachConsole(int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AllocConsole();

		[DllImport("ntdll.dll")]
		public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, out int returnLength);

		public static Process GetParentProcess() => GetParentProcess(Process.GetCurrentProcess().Handle);

		public static Process GetParentProcess(IntPtr handle)
		{
			var pbi = new ProcessBasicInformation();
			if (NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out var returnLength) != 0)
				return null;
			try
			{
				return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
	}
}
