using System;
using System.Threading.Tasks;
using System.Xml;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class RPCService: JsonRpcService
	{
		public struct HelloRequest
		{
			public string Name;
		};

		[JsonRpcMethod ("HELLO")]
		public string Hello (HelloRequest request)
		{
			return "Hello, " + request.Name;
		}

		public struct DateRequest
		{
		}

		[JsonRpcMethod ("DATE")]
		public string Date (DateRequest request)
		{
			var now = DateTime.UtcNow;
			return XmlConvert.ToString (now, XmlDateTimeSerializationMode.RoundtripKind);
		}

		[JsonRpcMethod ("LOGIN")]
		public void Login ()
		{
			// TODO:
		}
	}
}