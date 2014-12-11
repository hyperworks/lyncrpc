﻿using System;
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
			_client.SignInConfiguration.IsPasswordSaved = false;

			var credsHandler = new EventHandler<CredentialRequestedEventArgs> ((sender, e) => {
				if (e.Type != CredentialRequestedType.LyncAutodiscover)
					return;

				e.Submit (username, password, false);
			});

			var finish = new Action<Task> (task => {
				_client.CredentialRequested -= credsHandler;
				Log.Info ("lync: signed in " + username);
			});

			Log.Info ("lync: signing in...");
			_client.CredentialRequested += credsHandler;
			return Task.Factory.FromAsync (_client.BeginSignIn, _client.EndSignIn, username, username, password, null)
				.ContinueWith (handleException)
				.ContinueWith (finish);
		}


		private void handleException (Task task)
		{
			if (task.Exception != null)
				throw task.Exception;
		}
	}
}
