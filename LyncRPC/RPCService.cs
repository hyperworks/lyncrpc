using System;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class RPCService: JsonRpcService
	{
		public struct HelloRequest {
			public string Name;
		};

		[JsonRpcMethod ("HELLO")]
		public string Hello (HelloRequest request)
		{
			return "Hello, " + request.Name;
		}

		[JsonRpcMethod ("LOGIN")]
		public void Login ()
		{
			// TODO:
		}
	}
}