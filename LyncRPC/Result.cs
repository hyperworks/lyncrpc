using System;

namespace LyncRPC
{
	public class Result
	{
		public readonly bool Success;

		public Result (bool success)
		{
			Success = success;
		}
	}
}

