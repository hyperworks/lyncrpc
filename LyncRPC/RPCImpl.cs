using System;
using System.Threading.Tasks;

namespace LyncRPC
{
	internal class RPCImpl  : RPC
	{
		public Task Login ()
		{
			var task = new Task(() => {
				// perform rpc call
			});
			return task;
		}
	}
}

