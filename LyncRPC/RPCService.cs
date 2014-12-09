using System;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class RPCService: JsonRpcService
	{
		[JsonRpcMethod ("HELLO")]
		public string Hello (string name)
		{
			return "Hello, " + name;
		}

		[JsonRpcMethod ("LOGIN")]
		public void Login ()
		{
			// TODO:
		}
	}
}