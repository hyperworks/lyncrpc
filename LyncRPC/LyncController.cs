using System;
using System.Threading.Tasks;
using Microsoft.Lync.Model;

namespace LyncRPC
{
	public class LyncController
	{
		private LyncClient _client;

		public bool ShouldInitialize { get { return _client.State == ClientState.Uninitialized; } }

		public bool CanSignIn { get { return _client.State == ClientState.SignedOut; } }

		public LyncController ()
		{
			_client = LyncClient.GetClient (false);
			_client.StateChanged += LyncClient_StateChanged;
			_client.SignInDelayed += LyncClient_SignInDelayed;
			_client.CredentialRequested += LyncClient_CredentialRequested;
		}

		public Task Initialize ()
		{
			LAssert.Pre (ShouldInitialize, "already initializing or initialized.");

			Log.Info ("lync: initializing...");
			return Task.Factory.FromAsync (_client.BeginInitialize, _client.EndInitialize, null)
				.ContinueWith (handleException)
				.ContinueWith (task => {
				Log.Info ("lync: initialized.");
			});
		}

		public Task SignIn (string serverUrl, string username, string password)
		{
			LAssert.Pre (CanSignIn, "not signed out.");
			_client.SignInConfiguration.InternalServerUrl = serverUrl;
			_client.SignInConfiguration.ExternalServerUrl = serverUrl;

			Log.Info ("lync: signing in...");
			return Task.Factory.FromAsync (_client.BeginSignIn, _client.EndSignIn, username, username, password, null)
				.ContinueWith (handleException)
				.ContinueWith (task => {
				Log.Info ("lync: signed in " + username);
			});
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


		private void handleException (Task task)
		{
			if (task.Exception != null)
				throw task.Exception;
		}
	}
}

