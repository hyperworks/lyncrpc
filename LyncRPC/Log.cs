using System;

namespace LyncRPC
{
	public static class Log
	{
		public static readonly bool VerboseEnabled = false;

		public static void Info (string message)
		{
			Console.Out.WriteLine (message);
		}

		public static void Verbose (string message)
		{
			if (VerboseEnabled)
				Console.Out.WriteLine (message);
		}

		public static void Error (string message)
		{
			Console.Error.WriteLine (message);
		}
	}
}

