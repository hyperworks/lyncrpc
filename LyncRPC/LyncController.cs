using System;
using System.Threading.Tasks;
using Microsoft.Lync.Model;

namespace LyncRPC
{
	public class LyncController
	{
		private LyncClient _client;

		public bool ShouldInitialize { get { return _client.State == ClientState.Uninitialized; } }

		public LyncController ()
		{
			_client = LyncClient.GetClient (true);
			_client.StateChanged += LyncClient_StateChanged;
			_client.SignInDelayed += LyncClient_SignInDelayed;
			_client.CredentialRequested += LyncClient_CredentialRequested;
		}

		public Task Initialize ()
		{
			LAssert.Pre (ShouldInitialize, "already initializing or initialized.");
			return Task.Factory.FromAsync (_client.BeginInitialize, _client.EndInitialize, null);
		}

		private void LyncClient_StateChanged (object sender, ClientStateChangedEventArgs e)
		{
		}

		private void LyncClient_SignInDelayed (object sender, SignInDelayedEventArgs e)
		{
		}

		private void LyncClient_CredentialRequested (object sender, CredentialRequestedEventArgs e)
		{
		}
	}
}

