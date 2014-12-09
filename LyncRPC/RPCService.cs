using System;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class RPCService: JsonRpcService
	{
		[JsonRpcMethod]
		void Login()
		{
		}
	}
}