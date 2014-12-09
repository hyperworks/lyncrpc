using System;
using System.Net;

namespace LyncRPC
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// constructing the service causes it to be registered (see JsonRpcMethod.cs source for more info.)
			new RPCService();

			new Server ().Start ();
		}
	}
}