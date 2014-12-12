using System;
using System.Net;

namespace LyncRPC
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var controller = new LyncController ();
			if (controller.ShouldInitialize) {
				Log.Info ("lync: initializing...");
				controller.Initialize ().ContinueWith (task => {
					if (task.Exception != null)
						throw task.Exception;
				}).Wait ();
				Log.Info ("lync: initialized.");
			}

			// constructing the service causes it to be registered (see JsonRpcMethod.cs source for more info.)
			new RPCService (controller);
			new Server ().Start ();
		}
	}
}