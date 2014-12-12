using System;
using System.Threading.Tasks;
using System.Xml;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class RPCService: JsonRpcService
	{
		private LyncController _lync;

		public RPCService (LyncController controller) : base ()
		{
			_lync = controller;
		}

		public struct EmptyRequest
		{
		}

		public struct HelloRequest
		{
			public string Name;
		};

		[JsonRpcMethod ("HELLO")]
		public string Hello (HelloRequest request)
		{
			return "Hello, " + request.Name;
		}

		[JsonRpcMethod ("DATE")]
		public string Date (EmptyRequest _)
		{
			var now = DateTime.UtcNow;
			return XmlConvert.ToString (now, XmlDateTimeSerializationMode.RoundtripKind);
		}

		public struct SignInRequest
		{
			public string ServerUrl;
			public string Username;
			public string Password;
		}

		[JsonRpcMethod ("SIGNIN")]
		public Result SignIn (SignInRequest request)
		{
			_lync.SignIn (request.ServerUrl, request.Username, request.Password).Wait ();
			return new Result (true);
		}

		[JsonRpcMethod ("SIGNOUT")]
		public Result SignOut (EmptyRequest _)
		{
			_lync.SignOut ().Wait ();
			return new Result (true);
		}
	}
}