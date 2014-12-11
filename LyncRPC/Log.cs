using System;

namespace LyncRPC
{
	public static class Log
	{
		public static void Info (string message)
		{
			Console.Out.WriteLine (message);
		}

		public static void Error (string message)
		{
			Console.Error.WriteLine (message);
		}
	}
}

